using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _2280613193_webdocongnghe.Migrations
{
    /// <inheritdoc />
    public partial class AddReasonReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReturnReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnReason",
                table: "Orders");
        }
    }
}
