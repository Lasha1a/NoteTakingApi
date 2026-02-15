using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteTaking.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameNoteUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Users_userId",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Notes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_userId",
                table: "Notes",
                newName: "IX_Notes_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Users_UserId",
                table: "Notes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Users_UserId",
                table: "Notes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notes",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_UserId",
                table: "Notes",
                newName: "IX_Notes_userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Users_userId",
                table: "Notes",
                column: "userId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
