using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TodoLists.App.Migrations
{
    /// <inheritdoc />
    public partial class CreateV100 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<long>(name: "created_at", type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "super_users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    usernamelowercase = table.Column<string>(name: "username_lower_case", type: "text", nullable: false),
                    passwordhash = table.Column<byte[]>(name: "password_hash", type: "bytea", nullable: false),
                    passwordsalt = table.Column<byte[]>(name: "password_salt", type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_super_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "todo_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    profileid = table.Column<long>(name: "profile_id", type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    iscomplete = table.Column<bool>(name: "is_complete", type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_todo_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_todo_items_profiles_profile_id",
                        column: x => x.profileid,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    profileid = table.Column<long>(name: "profile_id", type: "bigint", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    usernamelowercase = table.Column<string>(name: "username_lower_case", type: "text", nullable: false),
                    passwordhash = table.Column<byte[]>(name: "password_hash", type: "bytea", nullable: false),
                    passwordsalt = table.Column<byte[]>(name: "password_salt", type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_profiles_profile_id",
                        column: x => x.profileid,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profiles_name",
                table: "profiles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_super_users_username_lower_case",
                table: "super_users",
                column: "username_lower_case",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_todo_items_profile_id",
                table: "todo_items",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_profile_id_username_lower_case",
                table: "users",
                columns: new[] { "profile_id", "username_lower_case" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "super_users");

            migrationBuilder.DropTable(
                name: "todo_items");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "profiles");
        }
    }
}
