using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cloud_atlas.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Markers_PhotoDetailsLinks_PhotosLinkId",
                table: "Markers");

            migrationBuilder.DropIndex(
                name: "IX_Markers_PhotosLinkId",
                table: "Markers");

            migrationBuilder.DropColumn(
                name: "PhotosLinkId",
                table: "Markers");

            migrationBuilder.AddColumn<Guid>(
                name: "MarkerId",
                table: "PhotoDetailsLinks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PhotoDetailsLinks_MarkerId",
                table: "PhotoDetailsLinks",
                column: "MarkerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoDetailsLinks_Markers_MarkerId",
                table: "PhotoDetailsLinks",
                column: "MarkerId",
                principalTable: "Markers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotoDetailsLinks_Markers_MarkerId",
                table: "PhotoDetailsLinks");

            migrationBuilder.DropIndex(
                name: "IX_PhotoDetailsLinks_MarkerId",
                table: "PhotoDetailsLinks");

            migrationBuilder.DropColumn(
                name: "MarkerId",
                table: "PhotoDetailsLinks");

            migrationBuilder.AddColumn<Guid>(
                name: "PhotosLinkId",
                table: "Markers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Markers_PhotosLinkId",
                table: "Markers",
                column: "PhotosLinkId");

            migrationBuilder.AddForeignKey(
                name: "FK_Markers_PhotoDetailsLinks_PhotosLinkId",
                table: "Markers",
                column: "PhotosLinkId",
                principalTable: "PhotoDetailsLinks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
