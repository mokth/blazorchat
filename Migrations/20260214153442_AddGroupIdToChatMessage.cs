using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blazorchat.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupIdToChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "ChatMessages");
        }
    }
}
