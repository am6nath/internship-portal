using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPreTestAndCoverImageSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPreTestPassed",
                table: "Applications",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreTestPassedAt",
                table: "Applications",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreTestScore",
                table: "Applications",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestType",
                table: "InternshipTestSessions",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "PreTest")
                .Annotation("MySql:CharSet", "utf8mb4");

            // Existing in-progress/completed applications are treated as pre-test passed
            migrationBuilder.Sql(
                "UPDATE Applications SET IsPreTestPassed = 1 WHERE Status IN ('InProgress', 'Completed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPreTestPassed",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "PreTestPassedAt",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "PreTestScore",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "TestType",
                table: "InternshipTestSessions");
        }
    }
}
