using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DateBoard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PrivateMessages",
                table: "PrivateMessages");

            migrationBuilder.RenameTable(
                name: "PrivateMessages",
                newName: "PrivateMessage");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "PrivateMessage",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "PrivateMessage",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PrivateMessage",
                table: "PrivateMessage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateMessage_ReceiverId",
                table: "PrivateMessage",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateMessage_SenderId",
                table: "PrivateMessage",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateMessage_AspNetUsers_ReceiverId",
                table: "PrivateMessage",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateMessage_AspNetUsers_SenderId",
                table: "PrivateMessage",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrivateMessage_AspNetUsers_ReceiverId",
                table: "PrivateMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateMessage_AspNetUsers_SenderId",
                table: "PrivateMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PrivateMessage",
                table: "PrivateMessage");

            migrationBuilder.DropIndex(
                name: "IX_PrivateMessage_ReceiverId",
                table: "PrivateMessage");

            migrationBuilder.DropIndex(
                name: "IX_PrivateMessage_SenderId",
                table: "PrivateMessage");

            migrationBuilder.RenameTable(
                name: "PrivateMessage",
                newName: "PrivateMessages");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "PrivateMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "PrivateMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PrivateMessages",
                table: "PrivateMessages",
                column: "Id");
        }
    }
}
