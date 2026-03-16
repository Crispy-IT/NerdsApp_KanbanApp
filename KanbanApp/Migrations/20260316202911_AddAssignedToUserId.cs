using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KanbanApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedToUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                table: "Cards",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "Cards");
        }
    }
}
