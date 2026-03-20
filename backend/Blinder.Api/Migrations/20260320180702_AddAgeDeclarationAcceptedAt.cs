using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blinder.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAgeDeclarationAcceptedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "age_declaration_accepted_at",
                table: "asp_net_users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "age_declaration_accepted_at",
                table: "asp_net_users");
        }
    }
}
