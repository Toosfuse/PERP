using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class updatereplay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReplyToMessage",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReplyToMessageId",
                table: "ChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplyToSenderName",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplyToMessage",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ReplyToMessageId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ReplyToSenderName",
                table: "ChatMessages");
        }
    }
}
