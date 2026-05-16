using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureGate.Web.Migrations
{
    /// <inheritdoc />
    public partial class camera_unique_code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Cameras_CameraCode",
                table: "Cameras",
                column: "CameraCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cameras_CameraCode",
                table: "Cameras");
        }
    }
}
