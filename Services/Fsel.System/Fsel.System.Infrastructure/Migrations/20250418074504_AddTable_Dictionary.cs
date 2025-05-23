using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_Dictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dictionaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Word = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Phonetic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartOfSpeech = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Meaning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExampleStr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dictionaries", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("bc261be5-f658-415a-9498-8b813742708c"),
                column: "ConfigStr",
                value: "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15' continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30' continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45' continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60' continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90' continues\",\"totalActions\":720,\"displayOrder\":5}]");

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f08945d0-3b03-4299-ae22-e3ce2408b74b"),
                column: "ConfigStr",
                value: "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15' continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30' continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45' continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60' continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90' continues\",\"totalActions\":720,\"displayOrder\":5}]");

            migrationBuilder.CreateIndex(
                name: "IX_Dictionaries_Word",
                table: "Dictionaries",
                column: "Word");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dictionaries");

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("bc261be5-f658-415a-9498-8b813742708c"),
                column: "ConfigStr",
                value: "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15\\u0027 continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30\\u0027 continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45\\u0027 continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60\\u0027 continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90\\u0027 continues\",\"totalActions\":720,\"displayOrder\":5}]");

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f08945d0-3b03-4299-ae22-e3ce2408b74b"),
                column: "ConfigStr",
                value: "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15\\u0027 continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30\\u0027 continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45\\u0027 continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60\\u0027 continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90\\u0027 continues\",\"totalActions\":720,\"displayOrder\":5}]");
        }
    }
}
