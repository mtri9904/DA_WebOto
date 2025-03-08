using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _12_Weboto.Migrations
{
    /// <inheritdoc />
    public partial class AddGiaTienToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GiaTien",
                table: "Cars",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiaTien",
                table: "Cars");
        }
    }
}
