using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_MockTestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseUnits");

            migrationBuilder.DropTable(
                name: "UnitMockFinalTests");

            migrationBuilder.DropTable(
                name: "MockFinalTests");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "VideoTimeCodes",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "VideoTimeCodes",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "VideoTimeCodes",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "Videos",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "Videos",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "Videos",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "Units",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "Units",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "Units",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "UnitLessons",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "UnitLessons",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "UnitLessons",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "TimeCodeExcercises",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "TimeCodeExcercises",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "TimeCodeExcercises",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "Questions",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "Questions",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "Questions",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "PlacementTests",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "PlacementTests",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "PlacementTests",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "LessonVideos",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "LessonVideos",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "LessonVideos",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "Lessons",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "Lessons",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "Lessons",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "LessonHomeWorks",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "LessonHomeWorks",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "LessonHomeWorks",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "LessonExtraPractices",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "LessonExtraPractices",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "LessonExtraPractices",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "HomeWorks",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "HomeWorks",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "HomeWorks",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "ExtraPractices",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "ExtraPractices",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "ExtraPractices",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "Excercises",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "Excercises",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "Excercises",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "ExcerciseQuestions",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "ExcerciseQuestions",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "ExcerciseQuestions",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "Courses",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "Courses",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "Courses",
                newName: "CreatedFullName");

            migrationBuilder.RenameColumn(
                name: "UpdatedUserName",
                table: "ClassForums",
                newName: "UpdatedFullName");

            migrationBuilder.RenameColumn(
                name: "DeletedUserName",
                table: "ClassForums",
                newName: "DeletedFullName");

            migrationBuilder.RenameColumn(
                name: "CreatedUserName",
                table: "ClassForums",
                newName: "CreatedFullName");

            migrationBuilder.CreateTable(
                name: "MockTests",
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
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CourseType = table.Column<int>(type: "int", nullable: false),
                    MockTestType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockTests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseUnitMockTests",
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
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MockTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseUnitMockTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseUnitMockTests_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseUnitMockTests_MockTests_MockTestId",
                        column: x => x.MockTestId,
                        principalTable: "MockTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseUnitMockTests_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitSkillMockTests",
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
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MockTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitSkillMockTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitSkillMockTests_MockTests_MockTestId",
                        column: x => x.MockTestId,
                        principalTable: "MockTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnitSkillMockTests_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseUnitMockTests_CourseId",
                table: "CourseUnitMockTests",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseUnitMockTests_MockTestId",
                table: "CourseUnitMockTests",
                column: "MockTestId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseUnitMockTests_UnitId",
                table: "CourseUnitMockTests",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitSkillMockTests_MockTestId",
                table: "UnitSkillMockTests",
                column: "MockTestId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitSkillMockTests_UnitId",
                table: "UnitSkillMockTests",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseUnitMockTests");

            migrationBuilder.DropTable(
                name: "UnitSkillMockTests");

            migrationBuilder.DropTable(
                name: "MockTests");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "VideoTimeCodes",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "VideoTimeCodes",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "VideoTimeCodes",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "Videos",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "Videos",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "Videos",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "Units",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "Units",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "Units",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "UnitLessons",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "UnitLessons",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "UnitLessons",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "TimeCodeExcercises",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "TimeCodeExcercises",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "TimeCodeExcercises",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "Questions",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "Questions",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "Questions",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "PlacementTests",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "PlacementTests",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "PlacementTests",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "LessonVideos",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "LessonVideos",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "LessonVideos",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "Lessons",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "Lessons",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "Lessons",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "LessonHomeWorks",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "LessonHomeWorks",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "LessonHomeWorks",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "LessonExtraPractices",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "LessonExtraPractices",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "LessonExtraPractices",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "HomeWorks",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "HomeWorks",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "HomeWorks",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "ExtraPractices",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "ExtraPractices",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "ExtraPractices",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "Excercises",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "Excercises",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "Excercises",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "ExcerciseQuestions",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "ExcerciseQuestions",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "ExcerciseQuestions",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "Courses",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "Courses",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "Courses",
                newName: "CreatedUserName");

            migrationBuilder.RenameColumn(
                name: "UpdatedFullName",
                table: "ClassForums",
                newName: "UpdatedUserName");

            migrationBuilder.RenameColumn(
                name: "DeletedFullName",
                table: "ClassForums",
                newName: "DeletedUserName");

            migrationBuilder.RenameColumn(
                name: "CreatedFullName",
                table: "ClassForums",
                newName: "CreatedUserName");

            migrationBuilder.CreateTable(
                name: "CourseUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseUnits_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseUnits_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockFinalTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CourseLevel = table.Column<int>(type: "int", nullable: false),
                    CourseSkill = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockFinalTests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitMockFinalTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MockFinalTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitMockFinalTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitMockFinalTests_MockFinalTests_MockFinalTestId",
                        column: x => x.MockFinalTestId,
                        principalTable: "MockFinalTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnitMockFinalTests_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseUnits_CourseId",
                table: "CourseUnits",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseUnits_UnitId",
                table: "CourseUnits",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitMockFinalTests_MockFinalTestId",
                table: "UnitMockFinalTests",
                column: "MockFinalTestId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitMockFinalTests_UnitId",
                table: "UnitMockFinalTests",
                column: "UnitId");
        }
    }
}
