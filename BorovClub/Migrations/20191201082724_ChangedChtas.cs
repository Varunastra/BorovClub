using Microsoft.EntityFrameworkCore.Migrations;

namespace BorovClub.Migrations
{
    public partial class ChangedChtas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_AspNetUsers_RecieverId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_AspNetUsers_SenderId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_RecieverId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_SenderId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "RecieverId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "Chats");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecieverId",
                table: "Chats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderId",
                table: "Chats",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_RecieverId",
                table: "Chats",
                column: "RecieverId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_SenderId",
                table: "Chats",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_AspNetUsers_RecieverId",
                table: "Chats",
                column: "RecieverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_AspNetUsers_SenderId",
                table: "Chats",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
