using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppWebSistemaComandasDigital.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveColorMarcaFromConfiguracionRestaurante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorMarca",
                table: "ConfiguracionRestaurante");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorMarca",
                table: "ConfiguracionRestaurante",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");
        }
    }
}
