using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PensaComigo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EtapasMoodsDekAgendado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.AddColumn<string>(
                name: "dek",
                table: "posts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "etapa_id",
                table: "posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "moods",
                table: "posts",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.CreateTable(
                name: "etapas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    refs = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_etapas", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "etapas",
                columns: new[] { "id", "descricao", "numero", "refs", "titulo" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), "Nomear o que aperta: a dúvida, a dor, o que não cala.", 1, null, "A Pergunta" },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), "Levar a pergunta ao texto: ler devagar, sem atalho.", 2, null, "A Busca" },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), "Deixar o texto responder — e mudar a pergunta.", 3, null, "O Encontro" },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), "Guardar o que foi dado e descansar nele.", 4, null, "O Descanso" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_posts_etapa_id",
                table: "posts",
                column: "etapa_id");

            migrationBuilder.CreateIndex(
                name: "IX_etapas_numero",
                table: "etapas",
                column: "numero",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_posts_etapas_etapa_id",
                table: "posts",
                column: "etapa_id",
                principalTable: "etapas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_posts_etapas_etapa_id",
                table: "posts");

            migrationBuilder.DropTable(
                name: "etapas");

            migrationBuilder.DropIndex(
                name: "IX_posts_etapa_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "dek",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "etapa_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "moods",
                table: "posts");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}
