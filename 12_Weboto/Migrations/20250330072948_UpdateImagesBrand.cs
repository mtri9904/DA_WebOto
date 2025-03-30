using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _12_Weboto.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImagesBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrandImages");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Brands",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Brands");

            migrationBuilder.CreateTable(
                name: "BrandImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrandImages_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrandImages_BrandId",
                table: "BrandImages",
                column: "BrandId");
        }
    }
}
