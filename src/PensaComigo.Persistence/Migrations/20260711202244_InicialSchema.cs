using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PensaComigo.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    google_id = table.Column<string>(type: "text", nullable: true),
                    imagem_url = table.Column<string>(type: "text", nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    conteudo = table.Column<string>(type: "jsonb", nullable: false),
                    imagem_capa = table.Column<string>(type: "text", nullable: false),
                    tempo_leitura = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    qtd_curtidas = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    qtd_visualizacoes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    autor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_posts_usuarios_autor_id",
                        column: x => x.autor_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comentarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    autor = table.Column<string>(type: "text", nullable: false),
                    conteudo = table.Column<string>(type: "text", nullable: false),
                    aprovado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comentarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_comentarios_comentarios_parent_id",
                        column: x => x.parent_id,
                        principalTable: "comentarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comentarios_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "likes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    viewer_hash = table.Column<string>(type: "text", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_likes", x => x.id);
                    table.ForeignKey(
                        name: "FK_likes_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_tags",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_tags", x => new { x.post_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_post_tags_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_post_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "id", "data_criacao", "email", "google_id", "imagem_url", "is_admin", "nome" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "antonio-ramon-dev@outlook.com", null, "", true, "Antonio" },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "jessica@pensacomigo.app", null, "", true, "Jéssica" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_comentarios_parent_id",
                table: "comentarios",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_comentarios_post_id",
                table: "comentarios",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_likes_post_id_viewer_hash",
                table: "likes",
                columns: new[] { "post_id", "viewer_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_tags_tag_id",
                table: "post_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_posts_autor_id",
                table: "posts",
                column: "autor_id");

            migrationBuilder.CreateIndex(
                name: "IX_posts_data_criacao",
                table: "posts",
                column: "data_criacao");

            migrationBuilder.CreateIndex(
                name: "IX_posts_slug",
                table: "posts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_nome",
                table: "tags",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_slug",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_google_id",
                table: "usuarios",
                column: "google_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comentarios");

            migrationBuilder.DropTable(
                name: "likes");

            migrationBuilder.DropTable(
                name: "post_tags");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
