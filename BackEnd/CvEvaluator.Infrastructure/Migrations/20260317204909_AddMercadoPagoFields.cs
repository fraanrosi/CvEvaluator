using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvEvaluator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMercadoPagoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MpPayerId",
                table: "UserSubscriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MpPreapprovalId",
                table: "UserSubscriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MpSubscriptionId",
                table: "UserSubscriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Plans",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ARS");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Plans",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "Currency", "Price" },
                values: new object[] { "ARS", 0m });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                columns: new[] { "Currency", "Price" },
                values: new object[] { "ARS", 9999m });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                columns: new[] { "Currency", "Price" },
                values: new object[] { "ARS", 24999m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MpPayerId",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "MpPreapprovalId",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "MpSubscriptionId",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Plans");
        }
    }
}
