using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cars.Storage.Context.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "cars",
                columns: new[] { "id", "availability", "brand", "car_uid", "model", "power", "price", "registration_number", "type" },
                values: new object[] { 1, true, "Mercedes Benz", new Guid("109b42f3-198d-4c89-9276-a7520a7120ab"), "GLA 250", 249, 3500, "ЛО777Х799", "SEDAN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "cars",
                keyColumn: "id",
                keyValue: 1);
        }
    }
}
