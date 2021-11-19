using Microsoft.EntityFrameworkCore.Migrations;

namespace CryptoTracker.DAL.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    trade_timestamp = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<float>(type: "real", nullable: false),
                    amount = table.Column<float>(type: "real", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    market = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
