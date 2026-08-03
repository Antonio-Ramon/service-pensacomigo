using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensaComigo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaEmailSeedAntonio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000001"),
                column: "email",
                value: "ar7339347@gmail.com");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000001"),
                column: "email",
                value: "antonio-ramon-dev@outlook.com");
        }
    }
}
