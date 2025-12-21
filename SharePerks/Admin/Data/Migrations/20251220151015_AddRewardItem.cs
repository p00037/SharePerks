using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "M_RewardItem",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredPoints = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M_RewardItem", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "M_Shareholder",
                columns: table => new
                {
                    ShareholderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShareholderNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShareholderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShareholderNameKana = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Holdings = table.Column<int>(type: "int", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrantedPoints = table.Column<int>(type: "int", nullable: false),
                    UsedPoints = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M_Shareholder", x => x.ShareholderId);
                });

            migrationBuilder.CreateTable(
                name: "T_RewardOrder",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShareholderId = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    IsExported = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_RewardOrder", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_T_RewardOrder_M_Shareholder_ShareholderId",
                        column: x => x.ShareholderId,
                        principalTable: "M_Shareholder",
                        principalColumn: "ShareholderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_RewardOrderItem",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SubtotalPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_RewardOrderItem", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_T_RewardOrderItem_M_RewardItem_ItemId",
                        column: x => x.ItemId,
                        principalTable: "M_RewardItem",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_T_RewardOrderItem_T_RewardOrder_OrderId",
                        column: x => x.OrderId,
                        principalTable: "T_RewardOrder",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_RewardOrder_ShareholderId",
                table: "T_RewardOrder",
                column: "ShareholderId");

            migrationBuilder.CreateIndex(
                name: "IX_T_RewardOrderItem_ItemId",
                table: "T_RewardOrderItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_T_RewardOrderItem_OrderId",
                table: "T_RewardOrderItem",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_RewardOrderItem");

            migrationBuilder.DropTable(
                name: "M_RewardItem");

            migrationBuilder.DropTable(
                name: "T_RewardOrder");

            migrationBuilder.DropTable(
                name: "M_Shareholder");
        }
    }
}
