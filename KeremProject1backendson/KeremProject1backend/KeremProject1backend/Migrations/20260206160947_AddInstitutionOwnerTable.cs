using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeremProject1backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionOwnerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassroomStudents_InstitutionUsers_StudentId",
                table: "ClassroomStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_Institutions_Users_ManagerUserId",
                table: "Institutions");

            migrationBuilder.DropIndex(
                name: "IX_Institutions_ManagerUserId",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                table: "Institutions");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "ClassroomStudents",
                newName: "InstitutionUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassroomStudents_StudentId",
                table: "ClassroomStudents",
                newName: "IX_ClassroomStudents_InstitutionUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassroomStudents_ClassroomId_StudentId",
                table: "ClassroomStudents",
                newName: "IX_ClassroomStudents_ClassroomId_InstitutionUserId");

            migrationBuilder.CreateTable(
                name: "InstitutionOwners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    InstitutionId = table.Column<int>(type: "int", nullable: false),
                    IsPrimaryOwner = table.Column<bool>(type: "bit", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionOwners_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstitutionOwners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionOwners_InstitutionId",
                table: "InstitutionOwners",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionOwners_UserId_InstitutionId",
                table: "InstitutionOwners",
                columns: new[] { "UserId", "InstitutionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassroomStudents_InstitutionUsers_InstitutionUserId",
                table: "ClassroomStudents",
                column: "InstitutionUserId",
                principalTable: "InstitutionUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassroomStudents_InstitutionUsers_InstitutionUserId",
                table: "ClassroomStudents");

            migrationBuilder.DropTable(
                name: "InstitutionOwners");

            migrationBuilder.RenameColumn(
                name: "InstitutionUserId",
                table: "ClassroomStudents",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassroomStudents_InstitutionUserId",
                table: "ClassroomStudents",
                newName: "IX_ClassroomStudents_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassroomStudents_ClassroomId_InstitutionUserId",
                table: "ClassroomStudents",
                newName: "IX_ClassroomStudents_ClassroomId_StudentId");

            migrationBuilder.AddColumn<int>(
                name: "ManagerUserId",
                table: "Institutions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_ManagerUserId",
                table: "Institutions",
                column: "ManagerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassroomStudents_InstitutionUsers_StudentId",
                table: "ClassroomStudents",
                column: "StudentId",
                principalTable: "InstitutionUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Institutions_Users_ManagerUserId",
                table: "Institutions",
                column: "ManagerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
