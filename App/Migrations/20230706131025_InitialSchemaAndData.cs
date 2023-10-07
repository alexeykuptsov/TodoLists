using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TodoLists.App.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchemaAndData : Migration
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
                name: "projects",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    profileid = table.Column<long>(name: "profile_id", type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_profiles_profile_id",
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

            migrationBuilder.CreateTable(
                name: "todo_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    profileid = table.Column<long>(name: "profile_id", type: "bigint", nullable: false),
                    projectid = table.Column<long>(name: "project_id", type: "bigint", nullable: false),
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
                    table.ForeignKey(
                        name: "fk_todo_items_projects_project_id",
                        column: x => x.projectid,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profiles_name",
                table: "profiles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_projects_profile_id",
                table: "projects",
                column: "profile_id");

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
                name: "ix_todo_items_project_id",
                table: "todo_items",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_profile_id_username_lower_case",
                table: "users",
                columns: new[] { "profile_id", "username_lower_case" },
                unique: true);

            SeedLocalDevData(migrationBuilder);
        }

        private void SeedLocalDevData(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                "super_users",
                new[] { "username", "username_lower_case", "password_hash", "password_salt" },
                new object[]
                {
                    "admin",
                    "admin",
                    Convert.FromHexString("AC87E0C07F80E642396FD5F393B59498776F6BB0D8EC32821AB0C2D3C193A251B727980EA772C216BF81194BBBBA8C6B947D55ED7D9A4EA02C52919973EF3FDB"),
                    Convert.FromHexString("DC54366BC7E3EA815035514D8929CF04898C8281FF1FAE51C443CF9AF5F04BB62E560E0D742B4486514A4A9A1CAF12A005110A52F70E795E1E5FF58108D5A6C2F5A3D9FBA7FE88B2C535A4C5C347DD9033FE670223090D7469FFD8632C8400F523E0246DC131E6F260EFEA18E683A9112461EC0DB787267A784B1A5B749DA9EF")
                });
            migrationBuilder.InsertData(
                "profiles", new[] { "name", "created_at" }, new object[] { "dev", 1669497925237 });
            migrationBuilder.InsertData("projects", new[] { "profile_id", "name" }, new object[] { 1, "Inbox" });
            migrationBuilder.InsertData(
                "users",
                new[] { "profile_id", "username", "username_lower_case", "password_hash", "password_salt" },
                new object[]
                {
                    1,
                    "user",
                    "user",
                    Convert.FromHexString("2B2E2D28530F1127D97EC25C84894D64358AFCC840A73C3EAFC20F7DE256317384642962ACCE58B735FDC01A63EB59CC231DAD94A5622853B04496254BDE9EFA"),
                    Convert.FromHexString("4AC0B7576779D25BFCAB3B322177541527466A0E0E2BB6AFC73A35199CD166EF487CCF907926A0D6200A8827E738209B2D1F03E4934E31012C67E02CE672772E9B9C997A787478D0C810F781FD2535BD1A722EA912C13E0D1C476DFD1D310AF076E537D9D1C7FA672F7B41F64435013E03337399D436CE8B9C7F358FA809B318")
                });
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
                name: "projects");

            migrationBuilder.DropTable(
                name: "profiles");
        }
    }
}
