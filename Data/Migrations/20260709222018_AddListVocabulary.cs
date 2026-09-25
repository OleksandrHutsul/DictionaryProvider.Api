using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiFlow.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddListVocabulary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dictionary_words",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Word = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NormalizedWord = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dictionary_words", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_lists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary_lists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vocabulary_lists_users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_list_shares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyListId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SharedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary_list_shares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vocabulary_list_shares_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vocabulary_list_shares_vocabulary_lists_VocabularyListId",
                        column: x => x.VocabularyListId,
                        principalTable: "vocabulary_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_list_words",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyListId = table.Column<Guid>(type: "uuid", nullable: false),
                    DictionaryWordId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayWord = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NormalizedWord = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary_list_words", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vocabulary_list_words_dictionary_words_DictionaryWordId",
                        column: x => x.DictionaryWordId,
                        principalTable: "dictionary_words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_vocabulary_list_words_vocabulary_lists_VocabularyListId",
                        column: x => x.VocabularyListId,
                        principalTable: "vocabulary_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_words_NormalizedWord",
                table: "dictionary_words",
                column: "NormalizedWord",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_list_shares_UserId",
                table: "vocabulary_list_shares",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_list_shares_VocabularyListId_UserId",
                table: "vocabulary_list_shares",
                columns: new[] { "VocabularyListId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_list_words_DictionaryWordId",
                table: "vocabulary_list_words",
                column: "DictionaryWordId");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_list_words_VocabularyListId_NormalizedWord",
                table: "vocabulary_list_words",
                columns: new[] { "VocabularyListId", "NormalizedWord" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_lists_OwnerId_Name",
                table: "vocabulary_lists",
                columns: new[] { "OwnerId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vocabulary_list_shares");

            migrationBuilder.DropTable(
                name: "vocabulary_list_words");

            migrationBuilder.DropTable(
                name: "dictionary_words");

            migrationBuilder.DropTable(
                name: "vocabulary_lists");
        }
    }
}
