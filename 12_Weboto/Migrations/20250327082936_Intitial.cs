using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _12_Weboto.Migrations
{
    /// <inheritdoc />
    public partial class Intitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenXe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NamSanXuat = table.Column<int>(type: "int", nullable: false),
                    NhienLieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoKM = table.Column<int>(type: "int", nullable: false),
                    SoChoNgoi = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    PhienBan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KieuDang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    XuatXu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DongXe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DongCo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HopSo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChieuDai = table.Column<int>(type: "int", nullable: false),
                    ChieuRong = table.Column<int>(type: "int", nullable: false),
                    ChieuCao = table.Column<int>(type: "int", nullable: false),
                    CoSoBanhXe = table.Column<int>(type: "int", nullable: false),
                    TrongLuongKhongTai = table.Column<int>(type: "int", nullable: false),
                    LopTruoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LopSau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MucTieuThuNgoaiDoThi = table.Column<float>(type: "real", nullable: false),
                    MucTieuThuTrongDoThi = table.Column<float>(type: "real", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cars_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarImages_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarImages_CarId",
                table: "CarImages",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_BrandId",
                table: "Cars",
                column: "BrandId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarImages");

            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "Brands");
        }
    }
}
