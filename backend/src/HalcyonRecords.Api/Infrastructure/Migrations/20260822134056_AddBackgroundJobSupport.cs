using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HalcyonRecords.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBackgroundJobSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShowcaseAccount",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastActiveAt",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(
                    new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                    new TimeSpan(0, 0, 0, 0, 0)
                )
            );

            migrationBuilder.AddColumn<int>(
                name: "RestockUnitsInStock",
                table: "Albums",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_Albums_RestockUnitsInStock_NotNegative",
                table: "Albums",
                sql: "RestockUnitsInStock >= 0"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Albums_RestockUnitsInStock_NotNegative",
                table: "Albums"
            );

            migrationBuilder.DropColumn(name: "IsShowcaseAccount", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "LastActiveAt", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "RestockUnitsInStock", table: "Albums");
        }
    }
}
