using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_ChangeColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InputModel",
                table: "AiPromptManagers",
                newName: "AiModel");

            migrationBuilder.RenameColumn(
                name: "AiModelName",
                table: "AiPromptManagers",
                newName: "Name");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AiPromptManagers",
                newName: "AiModelName");

            migrationBuilder.RenameColumn(
                name: "AiModel",
                table: "AiPromptManagers",
                newName: "InputModel");
        }
    }
}
