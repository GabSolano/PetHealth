using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetHealthEcosystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PETS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Breed = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Age = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TutorName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    NeedsPostOpCare = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PETS", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PETS");
        }
    }
}
