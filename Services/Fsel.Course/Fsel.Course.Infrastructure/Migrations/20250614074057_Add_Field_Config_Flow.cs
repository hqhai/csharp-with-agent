using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Field_Config_Flow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults");

            migrationBuilder.AddColumn<Guid>(
                name: "ActionFlowId",
                table: "PlacementTestResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StepFlowId",
                table: "PlacementTestResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FlowId",
                table: "PlacementTestGroupResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigStr",
                table: "Flows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_ActionFlowId",
                table: "PlacementTestResults",
                column: "ActionFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "ActionFlowId", "CorrectCount", "CorrectTotal", "CountQuestion", "CreatedDate", "CreatedFullName", "IsDeleted", "Level", "Percent", "PlacementTestGroupResultId", "PlacementTestId", "SkillScoresStr", "Status", "StepFlowId", "StudentId", "TotalQuestion", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_StepFlowId",
                table: "PlacementTestResults",
                column: "StepFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestGroupResults_FlowId",
                table: "PlacementTestGroupResults",
                column: "FlowId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTestGroupResults_Flows_FlowId",
                table: "PlacementTestGroupResults",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTestResults_ActionFlows_ActionFlowId",
                table: "PlacementTestResults",
                column: "ActionFlowId",
                principalTable: "ActionFlows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTestResults_StepFlows_StepFlowId",
                table: "PlacementTestResults",
                column: "StepFlowId",
                principalTable: "StepFlows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTestGroupResults_Flows_FlowId",
                table: "PlacementTestGroupResults");

            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTestResults_ActionFlows_ActionFlowId",
                table: "PlacementTestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTestResults_StepFlows_StepFlowId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_ActionFlowId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_StepFlowId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestGroupResults_FlowId",
                table: "PlacementTestGroupResults");

            migrationBuilder.DropColumn(
                name: "ActionFlowId",
                table: "PlacementTestResults");

            migrationBuilder.DropColumn(
                name: "StepFlowId",
                table: "PlacementTestResults");

            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "PlacementTestGroupResults");

            migrationBuilder.DropColumn(
                name: "ConfigStr",
                table: "Flows");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CountQuestion", "CreatedDate", "CreatedFullName", "IsDeleted", "Level", "Percent", "PlacementTestGroupResultId", "PlacementTestId", "SkillScoresStr", "Status", "StudentId", "TotalQuestion", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }
    }
}
