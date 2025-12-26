using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Menu_WeeklyProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("68365a87-73fa-4926-acb6-24f1403f77ad"),
                column: "ConfigStr",
                value: "{\"id\":25,\"code_title\":\"Dashboard\",\"link\":\"/dashboard\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":250,\"code_title\":\"Báo cáo tiến độ học tập\",\"link\":\"/dashboard/progress-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":251,\"code_title\":\"Báo cáo kết quả đánh giá đầu vào\",\"link\":\"/dashboard/result-pt\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":252,\"code_title\":\"Báo cáo kết quả học tập\",\"link\":\"/dashboard/result-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":253,\"code_title\":\"Chuyên cần\",\"link\":\"/dashboard/diligence\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":254,\"code_title\":\"Báo cáo mục tiêu tuần\",\"link\":\"/dashboard/week-progress\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"Admin,CSO,AdminSchool\",\"children\":null}]}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("68365a87-73fa-4926-acb6-24f1403f77ad"),
                column: "ConfigStr",
                value: "{\"id\":25,\"code_title\":\"Dashboard\",\"link\":\"/dashboard\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":250,\"code_title\":\"Báo cáo tiến độ học tập\",\"link\":\"/dashboard/progress-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":251,\"code_title\":\"Báo cáo kết quả đánh giá đầu vào\",\"link\":\"/dashboard/result-pt\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":252,\"code_title\":\"Báo cáo kết quả học tập\",\"link\":\"/dashboard/result-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":253,\"code_title\":\"Chuyên cần\",\"link\":\"/dashboard/diligence\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null}]}");
        }
    }
}
