using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeremProject1backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFieldsAndModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Classrooms_ClassroomId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_ClassroomId",
                table: "Conversations");

            migrationBuilder.RenameColumn(
                name: "Link",
                table: "Notifications",
                newName: "ActionUrl");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Exams",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Exams",
                newName: "ExamDate");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Conversations",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "JoinedAt",
                table: "ClassroomStudents",
                newName: "AssignedAt");

            migrationBuilder.AlterColumn<string>(
                name: "AttachmentUrl",
                table: "Messages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttachedExamId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttachedExamResultId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Messages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InstitutionUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TotalCorrect",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalEmpty",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWrong",
                table: "ExamResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "Type",
                table: "Conversations",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                table: "ClassroomStudents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeadTeacherId",
                table: "Classrooms",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVerifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TimeFormat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    PushNotifications = table.Column<bool>(type: "bit", nullable: false),
                    ExamResultNotifications = table.Column<bool>(type: "bit", nullable: false),
                    MessageNotifications = table.Column<bool>(type: "bit", nullable: false),
                    AccountLinkNotifications = table.Column<bool>(type: "bit", nullable: false),
                    ProfileLayout = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ShowStatistics = table.Column<bool>(type: "bit", nullable: false),
                    ShowActivity = table.Column<bool>(type: "bit", nullable: false),
                    ShowAchievements = table.Column<bool>(type: "bit", nullable: false),
                    DashboardLayout = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    VisibleWidgets = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AttachedExamId",
                table: "Messages",
                column: "AttachedExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AttachedExamResultId",
                table: "Messages",
                column: "AttachedExamResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SentAt",
                table: "Messages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ClassroomId",
                table: "Conversations",
                column: "ClassroomId",
                unique: true,
                filter: "[ClassroomId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomStudents_RemovedAt",
                table: "ClassroomStudents",
                column: "RemovedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_HeadTeacherId",
                table: "Classrooms",
                column: "HeadTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifications_Token",
                table: "EmailVerifications",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifications_UserId",
                table: "EmailVerifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_InstitutionUsers_HeadTeacherId",
                table: "Classrooms",
                column: "HeadTeacherId",
                principalTable: "InstitutionUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Classrooms_ClassroomId",
                table: "Conversations",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ExamResults_AttachedExamResultId",
                table: "Messages",
                column: "AttachedExamResultId",
                principalTable: "ExamResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Exams_AttachedExamId",
                table: "Messages",
                column: "AttachedExamId",
                principalTable: "Exams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_InstitutionUsers_HeadTeacherId",
                table: "Classrooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Classrooms_ClassroomId",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ExamResults_AttachedExamResultId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Exams_AttachedExamId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_Messages_AttachedExamId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_AttachedExamResultId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SentAt",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_ClassroomId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_ClassroomStudents_RemovedAt",
                table: "ClassroomStudents");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_HeadTeacherId",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "AttachedExamId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AttachedExamResultId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InstitutionUsers");

            migrationBuilder.DropColumn(
                name: "TotalCorrect",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "TotalEmpty",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "TotalWrong",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "ClassroomStudents");

            migrationBuilder.DropColumn(
                name: "HeadTeacherId",
                table: "Classrooms");

            migrationBuilder.RenameColumn(
                name: "ActionUrl",
                table: "Notifications",
                newName: "Link");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Exams",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ExamDate",
                table: "Exams",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Conversations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AssignedAt",
                table: "ClassroomStudents",
                newName: "JoinedAt");

            migrationBuilder.AlterColumn<string>(
                name: "AttachmentUrl",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ClassroomId",
                table: "Conversations",
                column: "ClassroomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Classrooms_ClassroomId",
                table: "Conversations",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id");
        }
    }
}
