using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensaComigo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BioNosAutores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bio",
                table: "usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000001"),
                column: "bio",
                value: "Escreve em Pensa Comigo sobre fé que se pensa — meditações que se aproximam de pregações escritas.");

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000002"),
                column: "bio",
                value: "Escreve em Pensa Comigo meditações reflexivas — a fé que te obriga a pensar.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bio",
                table: "usuarios");
        }
    }
}
