using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConexaoSolidaria.Infrastructure.Doacoes.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationInitialDoacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "doacao");

            migrationBuilder.CreateTable(
                name: "doacoes",
                schema: "doacao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_campanha = table.Column<Guid>(type: "uuid", nullable: false),
                    id_doador = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_doacao = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doacoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "doacoes_processadas",
                schema: "doacao",
                columns: table => new
                {
                    id_doacao = table.Column<Guid>(type: "uuid", nullable: false),
                    processada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doacoes_processadas", x => x.id_doacao);
                });

            migrationBuilder.CreateIndex(
                name: "ix_doacoes_id_campanha",
                schema: "doacao",
                table: "doacoes",
                column: "id_campanha");

            migrationBuilder.CreateIndex(
                name: "ix_doacoes_id_doador",
                schema: "doacao",
                table: "doacoes",
                column: "id_doador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "doacoes",
                schema: "doacao");

            migrationBuilder.DropTable(
                name: "doacoes_processadas",
                schema: "doacao");
        }
    }
}
