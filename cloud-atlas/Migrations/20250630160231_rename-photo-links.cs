using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cloud_atlas.Migrations
{
    /// <inheritdoc />
    public partial class renamephotolinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotoDetailsLinks_Markers_MarkerId",
                table: "PhotoDetailsLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhotoDetailsLinks",
                table: "PhotoDetailsLinks");

            migrationBuilder.RenameTable(
                name: "PhotoDetailsLinks",
                newName: "PhotoLinks");

            migrationBuilder.RenameIndex(
                name: "IX_PhotoDetailsLinks_MarkerId",
                table: "PhotoLinks",
                newName: "IX_PhotoLinks_MarkerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhotoLinks",
                table: "PhotoLinks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoLinks_Markers_MarkerId",
                table: "PhotoLinks",
                column: "MarkerId",
                principalTable: "Markers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotoLinks_Markers_MarkerId",
                table: "PhotoLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhotoLinks",
                table: "PhotoLinks");

            migrationBuilder.RenameTable(
                name: "PhotoLinks",
                newName: "PhotoDetailsLinks");

            migrationBuilder.RenameIndex(
                name: "IX_PhotoLinks_MarkerId",
                table: "PhotoDetailsLinks",
                newName: "IX_PhotoDetailsLinks_MarkerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhotoDetailsLinks",
                table: "PhotoDetailsLinks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoDetailsLinks_Markers_MarkerId",
                table: "PhotoDetailsLinks",
                column: "MarkerId",
                principalTable: "Markers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
