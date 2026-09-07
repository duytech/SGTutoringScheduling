using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutoringScheduling.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LessonId = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FromDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    FromStartTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    FromRoomId = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    ToDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ToStartTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    ToRoomId = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AfterCutoff = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonEvents_Bookings_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonEvents_LessonId",
                table: "LessonEvents",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonEvents");
        }
    }
}
