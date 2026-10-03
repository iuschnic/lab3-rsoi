using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rentals.Storage.Context.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rental",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rental_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    payment_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    car_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rental", x => x.id);
                    table.CheckConstraint("ck_rental_status", "status IN ('IN_PROGRESS', 'FINISHED', 'CANCELED')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_rental_rental_uid",
                table: "rental",
                column: "rental_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rental_username",
                table: "rental",
                column: "username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rental");
        }
    }
}
