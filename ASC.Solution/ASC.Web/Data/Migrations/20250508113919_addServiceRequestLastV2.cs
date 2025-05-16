using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASC.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class addServiceRequestLastV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VechicleType",
                table: "ServiceRequests",
                newName: "VehicleType");

            migrationBuilder.RenameColumn(
                name: "VechicleName",
                table: "ServiceRequests",
                newName: "VehicleName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VehicleType",
                table: "ServiceRequests",
                newName: "VechicleType");

            migrationBuilder.RenameColumn(
                name: "VehicleName",
                table: "ServiceRequests",
                newName: "VechicleName");
        }
    }
}
