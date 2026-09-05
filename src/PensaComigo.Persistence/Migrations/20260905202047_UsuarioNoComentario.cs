using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PensaComigo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioNoComentario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "usuario_id",
                table: "comentarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_comentarios_usuario_id",
                table: "comentarios",
                column: "usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_comentarios_usuarios_usuario_id",
                table: "comentarios",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comentarios_usuarios_usuario_id",
                table: "comentarios");

            migrationBuilder.DropIndex(
                name: "IX_comentarios_usuario_id",
                table: "comentarios");

            migrationBuilder.DropColumn(
                name: "usuario_id",
                table: "comentarios");
        }
    }
}
