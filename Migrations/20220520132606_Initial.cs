using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SettleDown.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettleDownCredential",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salt = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettleDownCredential", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SettleDownGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettleDownGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SettleDownUser",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CredentialsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettleDownUser", x => x.UserName);
                    table.ForeignKey(
                        name: "FK_SettleDownUser_SettleDownCredential_CredentialsId",
                        column: x => x.CredentialsId,
                        principalTable: "SettleDownCredential",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettleDownMember",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettleDownMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettleDownMember_SettleDownGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "SettleDownGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SettleDownMember_SettleDownUser_UserId",
                        column: x => x.UserId,
                        principalTable: "SettleDownUser",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettleDownDebt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettleDownDebt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettleDownDebt_SettleDownGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "SettleDownGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettleDownDebt_SettleDownMember_MemberId",
                        column: x => x.MemberId,
                        principalTable: "SettleDownMember",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SettleDownTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettleDownTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettleDownTransaction_SettleDownGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "SettleDownGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettleDownTransaction_SettleDownMember_MemberId",
                        column: x => x.MemberId,
                        principalTable: "SettleDownMember",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SettleDownDebt_GroupId",
                table: "SettleDownDebt",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SettleDownDebt_MemberId",
                table: "SettleDownDebt",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_SettleDownMember_GroupId",
                table: "SettleDownMember",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SettleDownMember_UserId",
                table: "SettleDownMember",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SettleDownTransaction_GroupId",
                table: "SettleDownTransaction",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SettleDownTransaction_MemberId",
                table: "SettleDownTransaction",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_SettleDownUser_CredentialsId",
                table: "SettleDownUser",
                column: "CredentialsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettleDownDebt");

            migrationBuilder.DropTable(
                name: "SettleDownTransaction");

            migrationBuilder.DropTable(
                name: "SettleDownMember");

            migrationBuilder.DropTable(
                name: "SettleDownGroup");

            migrationBuilder.DropTable(
                name: "SettleDownUser");

            migrationBuilder.DropTable(
                name: "SettleDownCredential");
        }
    }
}
