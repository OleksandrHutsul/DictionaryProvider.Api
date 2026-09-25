using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DictionaryProvider.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveAndListTestResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ArchivedAt",
                table: "vocabulary_lists",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "vocabulary_lists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "vocabulary_list_test_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyListId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary_list_test_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vocabulary_list_test_results_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vocabulary_list_test_results_vocabulary_lists_VocabularyLis~",
                        column: x => x.VocabularyListId,
                        principalTable: "vocabulary_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_list_test_results_UserId_VocabularyListId_Comple~",
                table: "vocabulary_list_test_results",
                columns: new[] { "UserId", "VocabularyListId", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_list_test_results_VocabularyListId",
                table: "vocabulary_list_test_results",
                column: "VocabularyListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vocabulary_list_test_results");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "vocabulary_lists");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "vocabulary_lists");
        }
    }
}
