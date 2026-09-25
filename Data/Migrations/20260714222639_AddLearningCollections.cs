using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DictionaryProvider.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "learning_collection_profiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_collection_profiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_learning_collection_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "learning_collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Accent = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_collections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_learning_collections_learning_collection_profiles_ProfileUs~",
                        column: x => x.ProfileUserId,
                        principalTable: "learning_collection_profiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "learning_collection_words",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Word = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NormalizedWord = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Definition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Translation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EasyCount = table.Column<int>(type: "integer", nullable: false),
                    HardCount = table.Column<int>(type: "integer", nullable: false),
                    AgainCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_collection_words", x => x.Id);
                    table.ForeignKey(
                        name: "FK_learning_collection_words_learning_collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "learning_collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_learning_collection_words_CollectionId_NormalizedWord",
                table: "learning_collection_words",
                columns: new[] { "CollectionId", "NormalizedWord" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_learning_collections_ProfileUserId_Name",
                table: "learning_collections",
                columns: new[] { "ProfileUserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "learning_collection_words");

            migrationBuilder.DropTable(
                name: "learning_collections");

            migrationBuilder.DropTable(
                name: "learning_collection_profiles");
        }
    }
}
