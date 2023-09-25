using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionTableV8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"min\":0,\"max\":10,\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":2,\"min\":11,\"max\":15,\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"651414ad-cbcf-462a-84b9-daf83880b25a\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":7,\"min\":16,\"max\":18,\"referenceQuestionId\":[\"a6f6b5dc-c72a-496c-859c-2fdc9d97f147\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":10,\"min\":19,\"max\":1000,\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]}]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"min\":\"0\",\"max\":\"10\",\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":2,\"min\":\"11\",\"max\":\"15\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"651414ad-cbcf-462a-84b9-daf83880b25a\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":7,\"min\":\"16\",\"max\":\"18\",\"referenceQuestionId\":[\"a6f6b5dc-c72a-496c-859c-2fdc9d97f147\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":10,\"min\":\"19\",\"max\":\"1000\",\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]}]");
        }
    }
}
