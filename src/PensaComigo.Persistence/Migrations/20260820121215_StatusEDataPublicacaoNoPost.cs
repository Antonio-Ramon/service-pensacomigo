using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensaComigo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StatusEDataPublicacaoNoPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "data_publicacao",
                table: "posts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Posts que já estavam no ar continuam no ar, datados pela criação.
            migrationBuilder.Sql("UPDATE posts SET status = 1, data_publicacao = data_criacao;");

            migrationBuilder.CreateIndex(
                name: "IX_posts_data_publicacao",
                table: "posts",
                column: "data_publicacao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_posts_data_publicacao",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "data_publicacao",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "status",
                table: "posts");
        }
    }
}
