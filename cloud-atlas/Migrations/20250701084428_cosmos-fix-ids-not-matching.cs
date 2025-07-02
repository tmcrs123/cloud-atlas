using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cloud_atlas.Migrations
{
    /// <inheritdoc />
    public partial class cosmosfixidsnotmatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PhotoLinks",
                newName: "PhotoLinkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhotoLinkId",
                table: "PhotoLinks",
                newName: "Id");
        }
    }
}
