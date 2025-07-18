using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cloud_atlas.Migrations
{
    /// <inheritdoc />
    public partial class removemarkerphotolink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhotoLinks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhotoLinks",
                columns: table => new
                {
                    PhotoLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarkerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoLinks", x => x.PhotoLinkId);
                    table.ForeignKey(
                        name: "FK_PhotoLinks_Markers_MarkerId",
                        column: x => x.MarkerId,
                        principalTable: "Markers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhotoLinks_MarkerId",
                table: "PhotoLinks",
                column: "MarkerId",
                unique: true);
        }
    }
}
