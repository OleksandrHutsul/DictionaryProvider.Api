using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DictionaryProvider.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackReportStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "feedback_reports",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "feedback_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_feedback_reports_Status",
                table: "feedback_reports",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_feedback_reports_Status",
                table: "feedback_reports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "feedback_reports");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "feedback_reports");
        }
    }
}
