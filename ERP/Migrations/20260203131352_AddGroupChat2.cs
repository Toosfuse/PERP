using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupChat2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ForwardedFromMessageId",
                table: "GroupMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplyToMessage",
                table: "GroupMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReplyToMessageId",
                table: "GroupMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplyToSenderName",
                table: "GroupMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForwardedFromMessageId",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "ReplyToMessage",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "ReplyToMessageId",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "ReplyToSenderName",
                table: "GroupMessages");
        }
    }
}
