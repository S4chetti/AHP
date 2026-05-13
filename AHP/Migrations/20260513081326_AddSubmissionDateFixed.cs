using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AHP.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionDateFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubmissionDate",
                table: "Answers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8177bdcf-4512-4043-baeb-cd0324b94a11",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "96e36b67-c4ee-4886-ba94-df151a209eac", "AQAAAAIAAYagAAAAEEH7BjFl82qzdhW2GKre7j2efndIGiFTaqS9EASd2D7ZmmQbilXlPag0waktlkR1sg==", "b39489c7-f9b5-4aa6-b961-7e78ffb5379f" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmissionDate",
                table: "Answers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8177bdcf-4512-4043-baeb-cd0324b94a11",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "04e399d3-a257-4c61-a003-36b22981915b", "AQAAAAIAAYagAAAAEBoawEJhrwTP7awub2cnAr0MEDKsP6rUsRivfBgopYbejimouIcehuwmMThui1Mf7w==", "c4c9ad73-e69e-4cde-b729-13b1df3a9d92" });
        }
    }
}
