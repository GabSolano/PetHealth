# 🐾 PetHealth Ecosystem API
- Gabriel Costa Solano - RM562325
- Kaiky Pereira Rodrigues - RM564578
- Leandro Guarido - RM561760

## 📖 Descrição do Projeto
O PetHealth Ecosystem é uma plataforma voltada para a saúde proativa de pets. Atuando nos pilares preventivo e terapêutico, esta API serve como o motor para um ecossistema omnichannel que gerencia lembretes de vacinas inteligentes, acompanhamento de pós-operatórios e retenção de tutores.

## 🚀 Como instalar e executar

**Pré-requisitos:**
- .NET 8 SDK instalado.
- Acesso a um banco de dados Oracle (FIAP).

**Passo a passo:**
1. Clone o repositório: `git clone https://github.com/GabSolano/PetHealth`
2. Navegue até a pasta do projeto: `cd PetHealthEcosystem.Api`
3. Restaure os pacotes: `dotnet restore`
4. Configure a Connection String do Oracle no arquivo `appsettings.json`.
5. Execute as migrations para gerar as tabelas: `dotnet ef database update`
6. Rode o projeto: `dotnet run`
7. Acesse o Swagger no navegador: `https://localhost:porta/swagger`

## 🛣️ Documentação das Rotas (Endpoints)

A API segue o padrão REST. Abaixo estão os endpoints disponíveis:

### Pet
* `GET /api/Pets` - Retorna a lista de todos os pets cadastrados (Status 200).
* `GET /api/Pets/{id}` - Retorna um pet específico pelo ID (Status 200 ou 404).
* `GET /api/Pets/breed/{breed}` - Retorna pets filtrados por raça (Status 200 ou 404).
* `GET /api/Pets/post-op/{needsCare}` - Retorna pets filtrados pela necessidade de cuidado pós-operatório (Status 200).
* `GET /api/Pets/tutor/{tutorName}` - Retorna pets buscando pelo nome do tutor (Status 200 ou 404).
* `POST /api/Pets` - Cadastra um novo pet. Requer payload no body (Status 201 ou 400).
* `PUT /api/Pets/{id}` - Atualiza os dados de um pet existente (Status 204, 400 ou 404).
* `DELETE /api/Pets/{id}` - Deleta um pet do sistema (Status 204 ou 404).
