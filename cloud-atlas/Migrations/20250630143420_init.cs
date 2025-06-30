using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cloud_atlas.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Atlases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atlases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhotoDetailsLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoDetailsLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Markers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    AtlasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhotosLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Markers_Atlases_AtlasId",
                        column: x => x.AtlasId,
                        principalTable: "Atlases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Markers_PhotoDetailsLinks_PhotosLinkId",
                        column: x => x.PhotosLinkId,
                        principalTable: "PhotoDetailsLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtlasUsers",
                columns: table => new
                {
                    AtlasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtlasUsers", x => new { x.AtlasId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AtlasUsers_Atlases_AtlasId",
                        column: x => x.AtlasId,
                        principalTable: "Atlases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtlasUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtlasUsers_UserId",
                table: "AtlasUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Markers_AtlasId",
                table: "Markers",
                column: "AtlasId");

            migrationBuilder.CreateIndex(
                name: "IX_Markers_PhotosLinkId",
                table: "Markers",
                column: "PhotosLinkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtlasUsers");

            migrationBuilder.DropTable(
                name: "Markers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Atlases");

            migrationBuilder.DropTable(
                name: "PhotoDetailsLinks");
        }
    }
}
