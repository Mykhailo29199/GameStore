using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class StoreMoneyAsDecimal : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Field14",
            table: "Orders");

        migrationBuilder.AlterColumn<decimal>(
            name: "UnitPrice",
            table: "Products",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(double),
            oldType: "float");

        migrationBuilder.AlterColumn<decimal>(
            name: "Freight",
            table: "Orders",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(double),
            oldType: "float",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "OrderDetails",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            oldClrType: typeof(double),
            oldType: "float");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<double>(
            name: "UnitPrice",
            table: "Products",
            type: "float",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldPrecision: 18,
            oldScale: 2);

        migrationBuilder.AlterColumn<double>(
            name: "Freight",
            table: "Orders",
            type: "float",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Field14",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AlterColumn<double>(
            name: "Price",
            table: "OrderDetails",
            type: "float",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldPrecision: 18,
            oldScale: 2);
    }
}
