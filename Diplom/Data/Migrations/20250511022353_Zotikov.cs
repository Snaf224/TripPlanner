using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Diplom.Data.Migrations
{
    /// <inheritdoc />
    public partial class Zotikov : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TripMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contribution = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripMembers_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidById = table.Column<int>(type: "int", nullable: true),
                    TripId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_TripMembers_PaidById",
                        column: x => x.PaidById,
                        principalTable: "TripMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: true),
                    TripId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_TripMembers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "TripMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TravelTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedToId = table.Column<int>(type: "int", nullable: true),
                    TripId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelTasks_TripMembers_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "TripMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TravelTasks_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4c", "66f28dbc-a241-4d91-81c5-3a75f7be8aec", "admin", "ADMIN" },
                    { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4d", "33871715-66f6-4cce-82e4-a368c2482ea2", "user", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "Gender", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4a", 0, "d9f5842f-ea14-4d54-9427-4ab9870e4c53", "admin@email.em", true, "Male", false, null, "ADMIN@EMAIL.EM", "ADMIN@EMAIL.EM", "AQAAAAIAAYagAAAAED+nWCZ5HtfMa2KHqD+OoUy1xFZgSfPOfZnyWJBndUruGg0RwtLTcj0ZRCNSI2QJ7g==", null, false, "af8c5f68-fde3-4e6b-9427-bf8c0e5671aa", false, "admin@email.em" },
                    { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4b", 0, "2404a032-f223-4927-8ae5-df0c2613be7d", "user@email.em", true, "Male", false, null, "USER@EMAIL.EM", "USER@EMAIL.EM", "AQAAAAIAAYagAAAAEKMVMAryYsuNELvbvCWdKMRy50WK+mQtU5vd3zRu522loUplV9pENvubcB4SkX0qpw==", null, false, "762fe29d-e783-4c57-8120-804af72f3ad9", false, "user@email.em" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4c", "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4a" },
                    { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4d", "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4a" },
                    { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4d", "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4b" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PaidById",
                table: "Expenses",
                column: "PaidById");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TripId",
                table: "Expenses",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReceiverId",
                table: "Notifications",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TripId",
                table: "Notifications",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelTasks_AssignedToId",
                table: "TravelTasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelTasks_TripId",
                table: "TravelTasks",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_TripMembers_TripId",
                table: "TripMembers",
                column: "TripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "TravelTasks");

            migrationBuilder.DropTable(
                name: "TripMembers");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4c", "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4a" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4d", "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4a" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4d", "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4b" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4d");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4a");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4b");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");
        }
    }
}
