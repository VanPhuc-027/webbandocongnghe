using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _2280613193_webdocongnghe.Migrations
{
    /// <inheritdoc />
    public partial class themthongsophandanhmuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategorySpecificationAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SpecificationAttributeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorySpecificationAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategorySpecificationAttributes_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategorySpecificationAttributes_SpecificationAttributes_SpecificationAttributeId",
                        column: x => x.SpecificationAttributeId,
                        principalTable: "SpecificationAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategorySpecificationAttributes_CategoryId",
                table: "CategorySpecificationAttributes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySpecificationAttributes_SpecificationAttributeId",
                table: "CategorySpecificationAttributes",
                column: "SpecificationAttributeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategorySpecificationAttributes");
        }
    }
}
