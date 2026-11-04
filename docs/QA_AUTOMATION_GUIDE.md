# 🧪 Guia de Automação de Testes e Estratégia de QA — PetHealth

Este documento estabelece as diretrizes de integração entre o time de **Desenvolvimento** e o time de **Quality Assurance (QA)** do ecossistema **PetHealth**, fornecendo arquitetura de dados de teste, utilitários reutilizáveis e especificações formais em **BDD (Behavior-Driven Development / Gherkin)** para a criação contínua de novos testes automatizados.

---

## 🎯 1. Estratégia e Pirâmide de Testes de QA

Para garantir alta confiabilidade na medicina preventiva e no acompanhamento pós-operatório dos animais, a estratégia de testes do PetHealth é dividida em três níveis:

```
          / \
         / E2E \         -> Testes End-to-End & Simulação de Canais (WhatsApp / Mobile)
        /-------\
       / Integr. \       -> Testes de Integração com WebApplicationFactory (Ciclo HTTP Real)
      /-----------\
     /  Unitários  \     -> Testes Unitários de Domínio, Validações e Regras Clínicas (AAA)
    /---------------\
```

- **Testes Unitários:** Validam regras clínicas isoladas (cálculo de doses e reforços vacinais, classificação de sintomas críticos pós-operatórios, validações cadastrais).
- **Testes de Integração:** Validam controladores, rotas autenticadas por perfil (RBAC), serialização de links HATEOAS, paginação e persistência cruzada (SQL Server + MongoDB).
- **Testes de Aceitação / Contrato (QA):** Automações com Postman/Newman, Playwright, RestAssured ou Reqnroll baseadas nos cenários Gherkin descritos neste guia.

---

## 🧱 2. Ferramentas e Utilitários de QA Disponíveis no Projeto

Para evitar repetição de código e dependência de dados voláteis, o projeto fornece utilitários prontos no padrão **Object Mother / Test Data Builder**:

### 🐶 `PetTestDataBuilder`
Permite à equipe de QA instanciar cenários de teste rapidamente com sintaxe fluente:
```csharp
// Pet saudável em pós-operatório
var petPosOp = PetTestDataBuilder.CreateDefault()
    .WithName("Luke")
    .WithSpecies("Canino")
    .InPostOpRecovery("Castração")
    .Build();

// Pet em estado crítico
var petCritico = PetTestDataBuilder.CreateDefault()
    .WithName("Spike")
    .InCriticalAlert("Cirurgia Ortopédica")
    .Build();
```

### 📋 `PostOpCheckInBuilder`
Cria payloads de check-in para simulação de respostas de tutores via WhatsApp:
```csharp
// Cenário 1: Recuperação saudável
var checkInNormal = PostOpCheckInBuilder.CreateHealthy().Build();

// Cenário 2: Complicação cirúrgica severa
var checkInCritico = PostOpCheckInBuilder.CreateCritical()
    .WithSeverePain(9)
    .WithWoundBleeding()
    .WithVomiting(true)
    .Build();
```

### 🔑 `QaAuthTokenHelper`
Gera tokens JWT assinados sob demanda para testes de autorização sem necessidade de chamadas prévias a `/api/auth/login`:
```csharp
string adminToken = QaAuthTokenHelper.GetAdminToken();
string vetToken   = QaAuthTokenHelper.GetVeterinarioToken();
string tutorToken = QaAuthTokenHelper.GetTutorToken();
```

---

## 📑 3. Especificação de Cenários de Teste em Formato BDD (Gherkin)

Os cenários abaixo estão formalizados para implementação em frameworks de automação de QA (ex: Reqnroll, SpecFlow, Cucumber, Newman):

### Funcionalidade: Acompanhamento de Pós-Operatório e Triagem de Alertas Críticos

```gherkin
Funcionalidade: Check-in de Pós-Operatório do PetHealth
  Como médico veterinário responsável
  Quero que a plataforma avalie automaticamente as respostas de check-in enviadas pelo tutor
  Para que casos de risco clínico sejam sinalizados imediatamente no painel de emergência

  Cenário: Check-in de rotina com recuperação estável
    Dado que existe um pet cadastrado com status "Em Recuperação"
    Quando o tutor envia o check-in pós-operatório com ingestão de água "Normal", dor nível 1 e incisão "Normal"
    Então o sistema deve registrar o check-in no histórico NoSQL (MongoDB)
    E o status de recuperação do pet deve permanecer como "Em Recuperação"
    E a resposta deve conter mensagem indicando andamento normal

  Cenário: Alerta crítico disparado por sangramento e dor severa
    Dado que existe um pet cadastrado com necessidade de cuidados pós-operatórios
    Quando o tutor envia o check-in com nível de dor 9, vômito "true" e incisão com "Sangramento"
    Então o sistema deve registrar o check-in marcando "IsCriticalAlert = true"
    E o status do pet no banco de dados deve ser atualizado para "Alerta Crítico"
    E o pet deve aparecer imediatamente na lista de triagem em "/api/pets/post-op/critical-alerts"
```

### Funcionalidade: Carteira Vacinal e Pilar Preventivo

```gherkin
Funcionalidade: Registro de Vacinação Preventiva
  Como médico veterinário
  Quero registrar aplicações de vacinas na carteira digital do pet
  Para que a data do próximo reforço seja calculada e agendada na rotina de alertas da IA

  Cenário: Aplicação de vacina polivalente com agendamento automático de reforço anual
    Dado que sou um usuário autenticado com perfil "Veterinario"
    E existe um pet cadastrado no sistema
    Quando registro a vacina "V10 PoliValente" aplicada na data de hoje
    Então o registro deve ser gravado na coleção de prontuários NoSQL
    E a data de próximo reforço deve ser calculada automaticamente para 365 dias após a aplicação
    E o prontuário do pet deve listar a vacina na carteira digital
```

### Funcionalidade: Controle de Acesso e Segurança (RBAC)

```gherkin
Funcionalidade: Segurança de Endpoints e Perfis de Acesso
  Como administrador do sistema
  Quero assegurar que apenas usuários com perfis autorizados executem operações sensíveis
  Para proteger o histórico médico e os dados dos tutores em conformidade com a LGPD

  Cenário: Tutor tenta excluir um pet cadastrado
    Dado que sou um usuário autenticado com perfil "Tutor"
    Quando tento enviar uma requisição DELETE para "/api/pets/{id}"
    Então o sistema deve recusar a operação retornando código HTTP 403 Forbidden

  Cenário: Administrador remove um pet do ecossistema
    Dado que sou um usuário autenticado com perfil "Admin"
    Quando envio uma requisição DELETE para um pet existente
    Então o sistema deve remover o pet retornando código HTTP 204 NoContent
    E deve gravar um evento imutável de auditoria de exclusão no MongoDB
```

---

## 🔬 4. Rastreabilidade e Diagnóstico para QA

Para facilitar a depuração de falhas nos pipelines de QA:
1. **CorrelationId:** Todas as respostas contêm o header `X-Correlation-Id`. Se um teste falhar, copie o ID retornado e busque diretamente nos logs estruturados do Serilog (`logs/log-*.txt`).
2. **RFC 7807 (ProblemDetails):** Qualquer erro 4xx ou 5xx retorna payload padronizado com `title`, `detail`, `status` e `instance`.
3. **Métricas de Latência:** As chamadas são cronometradas pelo middleware de métricas do OpenTelemetry (`PetMetrics`).

---

## 🚀 5. Execução de Testes Automatizados no Pipeline

Comandos para executar as suítes no ambiente local ou no runner de CI/CD:

```bash
# 1. Execução de toda a suíte de testes (44 testes com relatório em console)
dotnet test --logger "console;verbosity=normal"

# 2. Execução com geração de relatório de cobertura de código
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# 3. Execução focada nos testes de integração de QA
dotnet test tests/PetHealthEcosystem.Tests.Integration --filter "FullyQualifiedName~PetHealthClinicalQaIntegrationTests"
```
