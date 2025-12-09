using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_commerce.EF.Migrations
{
    /// <inheritdoc />
    public partial class addemialrefrnce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefrenceNewEmail",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefrenceNewEmail",
                table: "AspNetUsers");
        }
    }
}
