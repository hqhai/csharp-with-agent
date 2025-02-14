using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Data_DisplayOrderConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("08db065d-689f-42bc-95bd-5181fb4d7024"),
                column: "DisplayOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("29f6b274-6edd-427b-b1be-51a8ba5b23cd"),
                column: "DisplayOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("5f787b73-36cf-4db9-8378-74ffacef6df1"),
                column: "DisplayOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("858c0ac1-41a2-461b-9252-32f56914caf0"),
                column: "DisplayOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("ac124367-9b60-46dd-964f-aa85efc91ada"),
                column: "DisplayOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("dac69796-079e-4090-9b8f-e786856d4a20"),
                column: "DisplayOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("f9ee8b6b-d3b5-4183-9b4f-62c373c80020"),
                column: "DisplayOrder",
                value: 3);

            migrationBuilder.InsertData(
                table: "DisplayOrderConfig",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayOrder", "IsDeleted", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("f619cd35-cd55-4931-a7b4-2b1c992d1306"), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 1, false, "Disconnected", true, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("f619cd35-cd55-4931-a7b4-2b1c992d1306"));

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("08db065d-689f-42bc-95bd-5181fb4d7024"),
                column: "DisplayOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("29f6b274-6edd-427b-b1be-51a8ba5b23cd"),
                column: "DisplayOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("5f787b73-36cf-4db9-8378-74ffacef6df1"),
                column: "DisplayOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("858c0ac1-41a2-461b-9252-32f56914caf0"),
                column: "DisplayOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("ac124367-9b60-46dd-964f-aa85efc91ada"),
                column: "DisplayOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("dac69796-079e-4090-9b8f-e786856d4a20"),
                column: "DisplayOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DisplayOrderConfig",
                keyColumn: "Id",
                keyValue: new Guid("f9ee8b6b-d3b5-4183-9b4f-62c373c80020"),
                column: "DisplayOrder",
                value: 2);
        }
    }
}
