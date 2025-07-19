using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cloud_atlas.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalsToMarker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Journal",
                table: "Markers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Journal",
                table: "Markers");
        }
    }
}
