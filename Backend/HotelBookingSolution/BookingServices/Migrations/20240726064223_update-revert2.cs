using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingServices.Migrations
{
    public partial class updaterevert2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingDetail_Bookings_BookingId",
                table: "BookingDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingDetail",
                table: "BookingDetail");

            migrationBuilder.RenameTable(
                name: "BookingDetail",
                newName: "BookingDetails");

            migrationBuilder.RenameIndex(
                name: "IX_BookingDetail_BookingId",
                table: "BookingDetails",
                newName: "IX_BookingDetails_BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingDetails",
                table: "BookingDetails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingDetails_Bookings_BookingId",
                table: "BookingDetails",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingDetails_Bookings_BookingId",
                table: "BookingDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingDetails",
                table: "BookingDetails");

            migrationBuilder.RenameTable(
                name: "BookingDetails",
                newName: "BookingDetail");

            migrationBuilder.RenameIndex(
                name: "IX_BookingDetails_BookingId",
                table: "BookingDetail",
                newName: "IX_BookingDetail_BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingDetail",
                table: "BookingDetail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingDetail_Bookings_BookingId",
                table: "BookingDetail",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
