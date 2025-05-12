using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EComAPI.Migrations
{
    /// <inheritdoc />
    public partial class averageratingsToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "averageratings",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "averageratings",
                table: "Products");
        }
    }
}
