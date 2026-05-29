using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.API.Migrations
{
    /// <inheritdoc />
    public partial class FixFeedbackRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InternshipId1",
                table: "Applications",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_InternshipId1",
                table: "Applications",
                column: "InternshipId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Internships_InternshipId1",
                table: "Applications",
                column: "InternshipId1",
                principalTable: "Internships",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Internships_InternshipId1",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_InternshipId1",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "InternshipId1",
                table: "Applications");
        }
    }
}
