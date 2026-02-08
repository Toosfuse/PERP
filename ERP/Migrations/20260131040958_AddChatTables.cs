using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class AddChatTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetHistories_AssestUsers_AssestId",
                table: "AssetHistories");

            migrationBuilder.RenameColumn(
                name: "AssestId",
                table: "AssetHistories",
                newName: "AssestUserId");

            migrationBuilder.RenameIndex(
                name: "IX_AssetHistories_AssestId",
                table: "AssetHistories",
                newName: "IX_AssetHistories_AssestUserId");

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId",
                table: "ChatMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetHistories_AssestUsers_AssestUserId",
                table: "AssetHistories",
                column: "AssestUserId",
                principalTable: "AssestUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetHistories_AssestUsers_AssestUserId",
                table: "AssetHistories");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "AssestUserId",
                table: "AssetHistories",
                newName: "AssestId");

            migrationBuilder.RenameIndex(
                name: "IX_AssetHistories_AssestUserId",
                table: "AssetHistories",
                newName: "IX_AssetHistories_AssestId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetHistories_AssestUsers_AssestId",
                table: "AssetHistories",
                column: "AssestId",
                principalTable: "AssestUsers",
                principalColumn: "Id");
        }
    }
}
