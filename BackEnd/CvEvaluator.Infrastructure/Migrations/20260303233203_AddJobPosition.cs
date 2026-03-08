using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvEvaluator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobPositionId",
                table: "Evaluations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "JobPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPositions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_JobPositionId",
                table: "Evaluations",
                column: "JobPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPositions_CreatedAt",
                table: "JobPositions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobPositions_UserId",
                table: "JobPositions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_JobPositions_JobPositionId",
                table: "Evaluations",
                column: "JobPositionId",
                principalTable: "JobPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_JobPositions_JobPositionId",
                table: "Evaluations");

            migrationBuilder.DropTable(
                name: "JobPositions");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_JobPositionId",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "JobPositionId",
                table: "Evaluations");
        }
    }
}
