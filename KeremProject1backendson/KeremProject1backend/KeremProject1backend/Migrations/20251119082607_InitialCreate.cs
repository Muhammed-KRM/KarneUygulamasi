using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeremProject1backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "App");

            migrationBuilder.CreateTable(
                name: "AppUsers",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserRole = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    UserImageLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModUser = table.Column<int>(type: "int", nullable: false),
                    MalUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MalAccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MalRefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MalTokenExpires = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataLogs",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    OldModUser = table.Column<int>(type: "int", nullable: true),
                    OldModTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModUser = table.Column<int>(type: "int", nullable: false),
                    ModTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnimeLists",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AppUserId = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    ShareToken = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    IsTemplate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimeList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimeLists_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalSchema: "App",
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelatedListId = table.Column<int>(type: "int", nullable: true),
                    RelatedUserId = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalSchema: "App",
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFollows",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowerId = table.Column<int>(type: "int", nullable: false),
                    FollowingId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFollows_AppUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalSchema: "App",
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFollows_AppUsers_FollowingId",
                        column: x => x.FollowingId,
                        principalSchema: "App",
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ListComments",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimeListId = table.Column<int>(type: "int", nullable: false),
                    AppUserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListComments_AnimeLists_AnimeListId",
                        column: x => x.AnimeListId,
                        principalSchema: "App",
                        principalTable: "AnimeLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListComments_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalSchema: "App",
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ListLikes",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimeListId = table.Column<int>(type: "int", nullable: false),
                    AppUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListLike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListLikes_AnimeLists_AnimeListId",
                        column: x => x.AnimeListId,
                        principalSchema: "App",
                        principalTable: "AnimeLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListLikes_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalSchema: "App",
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tiers",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    AnimeListId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tiers_AnimeLists_AnimeListId",
                        column: x => x.AnimeListId,
                        principalSchema: "App",
                        principalTable: "AnimeLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RankedItems",
                schema: "App",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimeMalId = table.Column<int>(type: "int", nullable: false),
                    TierId = table.Column<int>(type: "int", nullable: false),
                    RankInTier = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankedItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankedItems_Tiers_TierId",
                        column: x => x.TierId,
                        principalSchema: "App",
                        principalTable: "Tiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimeLists_AppUserId",
                schema: "App",
                table: "AnimeLists",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimeLists_ShareToken",
                schema: "App",
                table: "AnimeLists",
                column: "ShareToken",
                unique: true,
                filter: "[ShareToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_UserName",
                schema: "App",
                table: "AppUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListComments_AnimeListId",
                schema: "App",
                table: "ListComments",
                column: "AnimeListId");

            migrationBuilder.CreateIndex(
                name: "IX_ListComments_AppUserId",
                schema: "App",
                table: "ListComments",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ListLikes_AnimeListId_AppUserId",
                schema: "App",
                table: "ListLikes",
                columns: new[] { "AnimeListId", "AppUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListLikes_AppUserId",
                schema: "App",
                table: "ListLikes",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppUserId",
                schema: "App",
                table: "Notifications",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RankedItems_AnimeMalId",
                schema: "App",
                table: "RankedItems",
                column: "AnimeMalId");

            migrationBuilder.CreateIndex(
                name: "IX_RankedItems_TierId",
                schema: "App",
                table: "RankedItems",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_Tiers_AnimeListId",
                schema: "App",
                table: "Tiers",
                column: "AnimeListId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowerId_FollowingId",
                schema: "App",
                table: "UserFollows",
                columns: new[] { "FollowerId", "FollowingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowingId",
                schema: "App",
                table: "UserFollows",
                column: "FollowingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataLogs",
                schema: "App");

            migrationBuilder.DropTable(
                name: "ListComments",
                schema: "App");

            migrationBuilder.DropTable(
                name: "ListLikes",
                schema: "App");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "App");

            migrationBuilder.DropTable(
                name: "RankedItems",
                schema: "App");

            migrationBuilder.DropTable(
                name: "UserFollows",
                schema: "App");

            migrationBuilder.DropTable(
                name: "Tiers",
                schema: "App");

            migrationBuilder.DropTable(
                name: "AnimeLists",
                schema: "App");

            migrationBuilder.DropTable(
                name: "AppUsers",
                schema: "App");
        }
    }
}
