using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changedPasswordAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                column: "Password",
                value: "AQAAAAIAAYagAAAAENWUApdzPmQWudXPT/eH43MRNkXC5P5E3Uq5JF4uSxxuCaf2pXJY5EzEFzUtY+VnYA==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                column: "Password",
                value: "$2a$11$He2A3p7CAqW1IfchSvzK1.IDGojzhoxG9Qtqg9QmOmVH5y05GasvO");
        }
    }
}
