using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blinder.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAppSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'app') THEN
                        RAISE EXCEPTION 'Schema "app" does not exist. Run the postgres/init bootstrap script before applying migrations.';
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateTable(
                name: "schema_markers",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schema_markers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "schema_markers",
                schema: "app");
        }
    }
}
