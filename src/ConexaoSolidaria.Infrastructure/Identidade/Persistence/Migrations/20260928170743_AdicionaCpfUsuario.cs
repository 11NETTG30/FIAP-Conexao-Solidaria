using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConexaoSolidaria.Infrastructure.Identidade.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCpfUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cpf",
                schema: "identidade",
                table: "usuarios",
                type: "char(11)",
                nullable: true);

            // CPF fixo para o usuário admin de seed (criado na migration inicial,
            // antes de este campo existir) — sem isso o ALTER COLUMN abaixo, que
            // torna a coluna obrigatória, quebra por causa da linha já existente.
            migrationBuilder.UpdateData(
                schema: "identidade",
                table: "usuarios",
                keyColumn: "id",
                keyValue: Guid.Parse("0ea5d907-6ce6-4167-b165-8aa42b023ee4"),
                column: "cpf",
                value: "11144477735");

            migrationBuilder.AlterColumn<string>(
                name: "cpf",
                schema: "identidade",
                table: "usuarios",
                type: "char(11)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(11)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_cpf",
                schema: "identidade",
                table: "usuarios",
                column: "cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuarios_cpf",
                schema: "identidade",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "cpf",
                schema: "identidade",
                table: "usuarios");
        }
    }
}
