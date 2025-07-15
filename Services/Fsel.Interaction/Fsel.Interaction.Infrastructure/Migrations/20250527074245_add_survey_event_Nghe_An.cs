using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_survey_event_Nghe_An : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0bced45a-cc34-47d4-a1e5-97735f52791b"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Centres de langue anglaise\",\"image\":null},{\"id\":2,\"content\":\"Apprentissage en ligne\",\"image\":null},{\"id\":3,\"content\":\"Cours particuliers\",\"image\":null},{\"id\":4,\"content\":\"Cours supplémentaires avec des enseignants\",\"image\":null},{\"id\":5,\"content\":\"Je ne prends pas de cours supplémentaires d'anglais\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2b267643-bb4f-4ac4-a14a-33350fa35108"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"Bạn bè/gia đình\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin tức/báo chí\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Sự kiện/hội thảo\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Trường học\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"Tờ rơi\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Khác....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("30adf928-8e15-4efd-9c35-e31798381a26"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Améliorer vos notes à l'école\",\"image\":null},{\"id\":2,\"content\":\"Passer une certification\",\"image\":null},{\"id\":3,\"content\":\"Intérêt personnel\",\"image\":null},{\"id\":4,\"content\":\"Autres raisons\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4fabc8f2-fcf1-4756-9e2e-64877d8a00da"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Cải thiện điểm số ở trường\",\"image\":null},{\"id\":2,\"content\":\"Thi chứng chỉ\",\"image\":null},{\"id\":3,\"content\":\"Sở thích\",\"image\":null},{\"id\":4,\"content\":\"Lý do khác\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66f876c3-716a-4b81-b790-e13e37fceedc"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Centres de langue anglaise\",\"image\":null},{\"id\":2,\"content\":\"Apprentissage en ligne\",\"image\":null},{\"id\":3,\"content\":\"Cours particuliers\",\"image\":null},{\"id\":4,\"content\":\"Cours supplémentaires avec des enseignants\",\"image\":null},{\"id\":5,\"content\":\"Je ne prends pas de cours supplémentaires d'anglais\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("70c9d600-a6a2-4d56-a740-3d0151efab1a"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Giao tiếp tiếng Anh rất pro trong mắt bạn bè\",\"image\":null},{\"id\":2,\"content\":\"Điểm cao được cô giáo khen ngợi trước lớp\",\"image\":null},{\"id\":3,\"content\":\"Thích vì tiếng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Không thích gì cả\",\"image\":null},{\"id\":5,\"content\":\"Ghét tiếng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7910a8a2-b89d-4579-a657-de2858ad499c"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"Bạn bè/gia đình\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin tức/báo chí\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Sự kiện/hội thảo\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Trường học\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"Tờ rơi\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Khác....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("80f7d9f6-39b2-4dff-a71e-92cf810c7b73"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Trung tâm tiếng Anh\",\"image\":null},{\"id\":2,\"content\":\"Học online\",\"image\":null},{\"id\":3,\"content\":\"Học gia sư\",\"image\":null},{\"id\":4,\"content\":\"Học thêm với giáo viên\",\"image\":null},{\"id\":5,\"content\":\"Không học thêm tiếng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8bb2b7f3-8254-4e83-b13f-f43cdb8d3c18"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Améliorer vos notes à l'école\",\"image\":null},{\"id\":2,\"content\":\"Passer une certification\",\"image\":null},{\"id\":3,\"content\":\"Intérêt personnel\",\"image\":null},{\"id\":4,\"content\":\"Autres raisons\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("97064707-b3d8-4fb5-bf4a-12bb6eb3776e"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Être considéré comme un expert en anglais par vos amis\",\"image\":null},{\"id\":2,\"content\":\"Obtenir de bonnes notes et être félicité par le professeur en classe\",\"image\":null},{\"id\":3,\"content\":\"Aimer l'anglais parce que c'est intéressant\",\"image\":null},{\"id\":4,\"content\":\"Ne rien aimer du tout\",\"image\":null},{\"id\":5,\"content\":\"Détester l'anglais complètement\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b3e92765-8b8d-4bec-a5d4-f9a9e213b53f"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Cải thiện điểm số ở trường\",\"image\":null},{\"id\":2,\"content\":\"Thi chứng chỉ\",\"image\":null},{\"id\":3,\"content\":\"Sở thích\",\"image\":null},{\"id\":4,\"content\":\"Lý do khác\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ba027423-106a-4bc6-a3a6-4b8386a44e51"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Văn hóa\",\"image\":\"castle 1.png\"},{\"id\":2,\"content\":\"Du lịch\",\"image\":\"plane 1.png\"},{\"id\":3,\"content\":\"Kết bạn\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Học tập\",\"image\":\"mortarboard 1.png\"},{\"id\":5,\"content\":\"Cơ hội nghề nghiệp\",\"image\":\"case 1.png\"},{\"id\":6,\"content\":\"Khác....\",\"image\":\"goal 1.png\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d4d2fae7-ae44-4652-883c-68c98a2972de"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Trung tâm tiếng Anh\",\"image\":null},{\"id\":2,\"content\":\"Học online\",\"image\":null},{\"id\":3,\"content\":\"Học gia sư\",\"image\":null},{\"id\":4,\"content\":\"Học thêm với giáo viên\",\"image\":null},{\"id\":5,\"content\":\"Không học thêm tiếng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d823acb4-62d8-4702-b95c-e6c5a8e49e4e"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Nâng cao điểm số\",\"image\":\"goal 1.png\"},{\"id\":2,\"content\":\"Mục tiêu công việc\",\"image\":\"work-target-icon.svg\"},{\"id\":3,\"content\":\"Cải thiện giao tiếp\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Thi chứng chỉ\",\"image\":\"student.svg\"},{\"id\":5,\"content\":\"Du học\",\"image\":\"plane 1.png\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("db2b2556-74cf-423e-8620-2ac1246baf1e"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Giao tiếp tiếng Anh rất pro trong mắt bạn bè\",\"image\":null},{\"id\":2,\"content\":\"Điểm cao được cô giáo khen ngợi trước lớp\",\"image\":null},{\"id\":3,\"content\":\"Thích vì tiếng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Không thích gì cả\",\"image\":null},{\"id\":5,\"content\":\"Ghét tiếng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f12f7c70-b9b5-49c5-8122-cc9c788e3354"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Học sinh, sinh viên\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Người đi làm\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"35a71ae7-49c1-4878-a1db-edcd2834f1cd\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fb59b7d2-ed36-4c5e-9fc5-4f176c9a5873"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Être considéré comme un expert en anglais par vos amis\",\"image\":null},{\"id\":2,\"content\":\"Obtenir de bonnes notes et être félicité par le professeur en classe\",\"image\":null},{\"id\":3,\"content\":\"Aimer l'anglais parce que c'est intéressant\",\"image\":null},{\"id\":4,\"content\":\"Ne rien aimer du tout\",\"image\":null},{\"id\":5,\"content\":\"Détester l'anglais complètement\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("03d12e43-250b-49b7-bd08-b12135e47723"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Học sinh, sinh viên\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Người đi làm\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"35a71ae7-49c1-4878-a1db-edcd2834f1cd\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Máy tính xách tay\",\"referenceQuestionId\":\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\"},{\"id\":2,\"content\":\"Máy tính để bàn\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Cả 2\",\"referenceQuestionId\":\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\"},{\"id\":4,\"content\":\"No\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("14787cbf-cc43-4453-a148-6d11a683f311"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Nâng cao điểm số\",\"image\":\"goal 1.png\"},{\"id\":2,\"content\":\"Mục tiêu công việc\",\"image\":\"work-target-icon.svg\"},{\"id\":3,\"content\":\"Cải thiện giao tiếp\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Thi chứng chỉ\",\"image\":\"student.svg\"},{\"id\":5,\"content\":\"Du học\",\"image\":\"plane 1.png\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Các buổi tối thứ Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Các buổi tối thứ Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Các buổi tối thứ Tư\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"Các buổi tối thứ Năm\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"Các buổi tối thứ Sáu\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"Các buổi sáng thứ Bảy\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"Các buổi chiều thứ Bảy\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"Các buổi tối thứ Bảy\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"Các buổi sáng chủ nhật\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"Các buổi chiều chủ nhật\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"Các buổi tối chủ nhật\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("238cae89-e1d8-408a-bb3c-1a0b27cd0edb"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Cải thiện điểm số ở trường\",\"image\":null},{\"id\":2,\"content\":\"Thi chứng chỉ\",\"image\":null},{\"id\":3,\"content\":\"Sở thích\",\"image\":null},{\"id\":4,\"content\":\"Lý do khác\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("482c87c7-b555-4de2-a418-f990960e0ee6"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Cải thiện điểm số ở trường\",\"image\":null},{\"id\":2,\"content\":\"Thi chứng chỉ\",\"image\":null},{\"id\":3,\"content\":\"Sở thích\",\"image\":null},{\"id\":4,\"content\":\"Lý do khác\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"Bạn bè/gia đình\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin tức/báo chí\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Sự kiện/hội thảo\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Trường học\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"Tờ rơi\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Khác....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("52548bc5-3536-478c-978b-05f98815bf31"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Nam\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Nữ\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Khác\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"1 (Không sẵn lòng chút nào)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (Rất sẵn sàng)\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("651414ad-cbcf-462a-84b9-daf83880b25a"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Các buổi tối thứ Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Các buổi tối thứ Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Các buổi tối thứ Tư\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"Các buổi tối thứ Năm\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"Các buổi tối thứ Sáu\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"Các buổi sáng thứ Bảy\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"Các buổi chiều thứ Bảy\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"Các buổi tối thứ Bảy\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"Các buổi sáng chủ nhật\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"Các buổi chiều chủ nhật\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"Các buổi tối chủ nhật\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                column: "AnswerStr",
                value: "{\"countryCode\":123,\"countryName\":\"Việt Nam\",\"provinceCode\":29,\"provinceName\":\"Hà Nội\"}");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Văn hóa\",\"image\":\"castle 1.png\"},{\"id\":2,\"content\":\"Du lịch\",\"image\":\"plane 1.png\"},{\"id\":3,\"content\":\"Kết bạn\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Học tập\",\"image\":\"mortarboard 1.png\"},{\"id\":5,\"content\":\"Cơ hội nghề nghiệp\",\"image\":\"case 1.png\"},{\"id\":6,\"content\":\"Khác....\",\"image\":\"goal 1.png\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73a7a0c8-1e07-4351-b3f4-3e843623bbaf"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Trung tâm tiếng Anh\",\"image\":null},{\"id\":2,\"content\":\"Học online\",\"image\":null},{\"id\":3,\"content\":\"Học gia sư\",\"image\":null},{\"id\":4,\"content\":\"Học thêm với giáo viên\",\"image\":null},{\"id\":5,\"content\":\"Không học thêm tiếng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Có\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Không\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Bạn đã biết một chút Tiếng Anh\"},{\"id\":2,\"content\":\"Đây là lần đầu bạn học Tiếng Anh\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Có\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Không\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("a6f6b5dc-c72a-496c-859c-2fdc9d97f147"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Có\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Không\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Các buổi tối thứ Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Các buổi tối thứ Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Các buổi tối thứ Tư\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"Các buổi tối thứ Năm\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"Các buổi tối thứ Sáu\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"Các buổi sáng thứ Bảy\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"Các buổi chiều thứ Bảy\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"Các buổi tối thứ Bảy\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"Các buổi sáng chủ nhật\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"Các buổi chiều chủ nhật\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"Các buổi tối chủ nhật\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("c6bda257-0757-4290-86c1-794d0ba6ae3b"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Giao tiếp tiếng Anh rất pro trong mắt bạn bè\",\"image\":null},{\"id\":2,\"content\":\"Điểm cao được cô giáo khen ngợi trước lớp\",\"image\":null},{\"id\":3,\"content\":\"Thích vì tiếng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Không thích gì cả\",\"image\":null},{\"id\":5,\"content\":\"Ghét tiếng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("c88e157b-1496-4aae-b81f-dd0718524e38"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Giao tiếp tiếng Anh rất pro trong mắt bạn bè\",\"image\":null},{\"id\":2,\"content\":\"Điểm cao được cô giáo khen ngợi trước lớp\",\"image\":null},{\"id\":3,\"content\":\"Thích vì tiếng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Không thích gì cả\",\"image\":null},{\"id\":5,\"content\":\"Ghét tiếng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("da618b05-f9b8-4bfd-8bae-7a5adb668d16"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Trung tâm tiếng Anh\",\"image\":null},{\"id\":2,\"content\":\"Học online\",\"image\":null},{\"id\":3,\"content\":\"Học gia sư\",\"image\":null},{\"id\":4,\"content\":\"Học thêm với giáo viên\",\"image\":null},{\"id\":5,\"content\":\"Không học thêm tiếng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"1 (Không sẵn sàng)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (Rất sẵn sàng)\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"33 Lạc Trung\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"125 Hoàng Ngân\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Tôi ổn với cả hai\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f563da40-d609-4922-90b9-44e4290edfef"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"Bạn bè/gia đình\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin tức/báo chí\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Sự kiện/hội thảo\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Trường học\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"Tờ rơi\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Khác....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Ba Đình\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Hoàn Kiếm\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Tây Hồ\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"Long Biên\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"Cầu Giấy\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"Đống Đa\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"Hai Bà Trưng\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"Hoàng Mai\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"Thanh Xuân\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"Hà Đông\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"Bắc Từ Liêm\",\"referenceQuestionId\":null},{\"id\":12,\"content\":\"Nam Từ Liêm\",\"referenceQuestionId\":null},{\"id\":13,\"content\":\"Ba Vì\",\"referenceQuestionId\":null},{\"id\":14,\"content\":\"Chương Mỹ\",\"referenceQuestionId\":null},{\"id\":15,\"content\":\"Đan Phượng\",\"referenceQuestionId\":null},{\"id\":16,\"content\":\"Đông Anh\",\"referenceQuestionId\":null},{\"id\":17,\"content\":\"Gia Lâm\",\"referenceQuestionId\":null},{\"id\":18,\"content\":\"Hoài Đức\",\"referenceQuestionId\":null},{\"id\":19,\"content\":\"Mê Linh\",\"referenceQuestionId\":null},{\"id\":20,\"content\":\"Phú Xuyên\",\"referenceQuestionId\":null},{\"id\":21,\"content\":\"Phúc Thọ\",\"referenceQuestionId\":null},{\"id\":22,\"content\":\"Quốc Oai\",\"referenceQuestionId\":null},{\"id\":23,\"content\":\"Thạch Thất\",\"referenceQuestionId\":null},{\"id\":24,\"content\":\"Thanh Oai\",\"referenceQuestionId\":null},{\"id\":25,\"content\":\"Thanh Trì\",\"referenceQuestionId\":null},{\"id\":26,\"content\":\"Thường Tín\",\"referenceQuestionId\":null},{\"id\":27,\"content\":\" Ứng Hòa\",\"referenceQuestionId\":null},{\"id\":28,\"content\":\"khác\",\"referenceQuestionId\":null}]");

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CompetitionEventId", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "SurveyFormType", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("69b1a8ab-2811-450d-b9bf-12cd91e44193"), "[{\"id\":1,\"content\":\"Trung tâm tiếng Anh\",\"image\":null},{\"id\":2,\"content\":\"Học online\",\"image\":null},{\"id\":3,\"content\":\"Học gia sư\",\"image\":null},{\"id\":4,\"content\":\"Học thêm với giáo viên\",\"image\":null},{\"id\":5,\"content\":\"Không học thêm tiếng Anh\",\"image\":null}]", new Guid("a0aa2d10-65b3-4df1-b773-3d63d8a03230"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 2, 1f, null, false, false, "Bạn học thêm Tiếng Anh ở đâu?", "Event", "MultipleChoiceVertical", null, null, null },
                    { new Guid("6fe5a134-6379-4244-96f1-69cc71962368"), "[{\"id\":1,\"content\":\"Giao tiếp tiếng Anh rất pro trong mắt bạn bè\",\"image\":null},{\"id\":2,\"content\":\"Điểm cao được cô giáo khen ngợi trước lớp\",\"image\":null},{\"id\":3,\"content\":\"Thích vì tiếng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Không thích gì cả\",\"image\":null},{\"id\":5,\"content\":\"Ghét tiếng Anh\",\"image\":null}]", new Guid("a0aa2d10-65b3-4df1-b773-3d63d8a03230"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 2, 2f, null, false, false, "Điều bạn thích nhất ở việc học tiếng Anh là gì?", "Event", "MultipleChoiceVertical", null, null, null },
                    { new Guid("92d64860-c647-4359-a326-78a39de366e1"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new Guid("a0aa2d10-65b3-4df1-b773-3d63d8a03230"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 1, 2f, null, false, false, "Bạn đánh giá mức độ quan trọng của việc học tiếng Anh như thế nào (thang điểm 5 từ thấp đến cao)?", "Event", "MultipleChoiceVertical", null, null, null },
                    { new Guid("eb9ddecf-15fc-4393-af86-eb2b14f64eb7"), "[{\"id\":1,\"content\":\"Cải thiện điểm số ở trường\",\"image\":null},{\"id\":2,\"content\":\"Thi chứng chỉ\",\"image\":null},{\"id\":3,\"content\":\"Sở thích\",\"image\":null},{\"id\":4,\"content\":\"Lý do khác\",\"image\":null}]", new Guid("a0aa2d10-65b3-4df1-b773-3d63d8a03230"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 1, 1f, null, false, false, "Mục tiêu của bạn khi học Tiếng Anh là gì?", "Event", "MultipleChoiceVertical", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "SurveyQuestionTranslations",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Language", "Question", "SurveyQuestionId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("1092914c-2706-4226-8f97-579c7ae9c500"), "[{\"id\":1,\"content\":\"Improving school grades\",\"image\":null},{\"id\":2,\"content\":\"Taking certification exams\",\"image\":null},{\"id\":3,\"content\":\"Personal interest\",\"image\":null},{\"id\":4,\"content\":\"Other reasons\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "What is your goal in learning English?", new Guid("eb9ddecf-15fc-4393-af86-eb2b14f64eb7"), null, null, null },
                    { new Guid("1ad583b8-ee62-4bdb-ba56-e228062929b4"), "[{\"id\":1,\"content\":\"Centres de langue anglaise\",\"image\":null},{\"id\":2,\"content\":\"Apprentissage en ligne\",\"image\":null},{\"id\":3,\"content\":\"Cours particuliers\",\"image\":null},{\"id\":4,\"content\":\"Cours supplémentaires avec des enseignants\",\"image\":null},{\"id\":5,\"content\":\"Je ne prends pas de cours supplémentaires d'anglais\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Où prenez-vous des cours supplémentaires d'anglais ?", new Guid("69b1a8ab-2811-450d-b9bf-12cd91e44193"), null, null, null },
                    { new Guid("2ca57663-8c08-43ef-b9f3-1f044e724fca"), "[{\"id\":1,\"content\":\"Être considéré comme un expert en anglais par vos amis\",\"image\":null},{\"id\":2,\"content\":\"Obtenir de bonnes notes et être félicité par le professeur en classe\",\"image\":null},{\"id\":3,\"content\":\"Aimer l'anglais parce que c'est intéressant\",\"image\":null},{\"id\":4,\"content\":\"Ne rien aimer du tout\",\"image\":null},{\"id\":5,\"content\":\"Détester l'anglais complètement\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Qu'aimez-vous le plus dans l'apprentissage de l'anglais ?", new Guid("6fe5a134-6379-4244-96f1-69cc71962368"), null, null, null },
                    { new Guid("337a1505-8221-4967-93a9-285a31ad5da2"), "[{\"id\":1,\"content\":\"English language centers\",\"image\":null},{\"id\":2,\"content\":\"Online learning\",\"image\":null},{\"id\":3,\"content\":\"Private tutoring\",\"image\":null},{\"id\":4,\"content\":\"Extra classes with teachers\",\"image\":null},{\"id\":5,\"content\":\"Do not take additional English lessons\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "Where do you take additional English lessons?", new Guid("69b1a8ab-2811-450d-b9bf-12cd91e44193"), null, null, null },
                    { new Guid("36c3bef7-7fef-4999-a553-49fa53eed5c1"), "[{\"id\":1,\"content\":\"Améliorer vos notes à l'école\",\"image\":null},{\"id\":2,\"content\":\"Passer une certification\",\"image\":null},{\"id\":3,\"content\":\"Intérêt personnel\",\"image\":null},{\"id\":4,\"content\":\"Autres raisons\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Quels sont vos objectifs pour apprendre l'anglais ?", new Guid("eb9ddecf-15fc-4393-af86-eb2b14f64eb7"), null, null, null },
                    { new Guid("3eae31f3-4e19-4e76-8c89-a571558f5ece"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Bạn đánh giá mức độ quan trọng của việc học tiếng Anh như thế nào (thang điểm 5 từ thấp đến cao)?", new Guid("92d64860-c647-4359-a326-78a39de366e1"), null, null, null },
                    { new Guid("52248d53-305e-49b6-8260-bf78f1c91889"), "[{\"id\":1,\"content\":\"Cải thiện điểm số ở trường\",\"image\":null},{\"id\":2,\"content\":\"Thi chứng chỉ\",\"image\":null},{\"id\":3,\"content\":\"Sở thích\",\"image\":null},{\"id\":4,\"content\":\"Lý do khác\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Mục tiêu của bạn khi học Tiếng Anh là gì?", new Guid("eb9ddecf-15fc-4393-af86-eb2b14f64eb7"), null, null, null },
                    { new Guid("5c29c250-58cd-4675-ac3c-41a13d6c6055"), "[{\"id\":1,\"content\":\"Giao tiếp tiếng Anh rất pro trong mắt bạn bè\",\"image\":null},{\"id\":2,\"content\":\"Điểm cao được cô giáo khen ngợi trước lớp\",\"image\":null},{\"id\":3,\"content\":\"Thích vì tiếng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Không thích gì cả\",\"image\":null},{\"id\":5,\"content\":\"Ghét tiếng Anh\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Điều bạn thích nhất ở việc học tiếng Anh là gì?", new Guid("6fe5a134-6379-4244-96f1-69cc71962368"), null, null, null },
                    { new Guid("836f815f-955b-4e08-a1ee-c7685ccec230"), "[{\"id\":1,\"content\":\"Being seen as a pro at English by friends\",\"image\":null},{\"id\":2,\"content\":\"High grades and being praised by the teacher in class\",\"image\":null},{\"id\":3,\"content\":\"Enjoying English because it is interesting\",\"image\":null},{\"id\":4,\"content\":\"Do not like anything about it\",\"image\":null},{\"id\":5,\"content\":\"Dislike English entirely\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "What do you like most about learning English?", new Guid("6fe5a134-6379-4244-96f1-69cc71962368"), null, null, null },
                    { new Guid("8b3ceaa3-64d0-4070-b26e-56633396ca0d"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Comment évaluez-vous l'importance de l'apprentissage de l'anglais ? (échelle de 1 à 5, de faible à élevé)?", new Guid("92d64860-c647-4359-a326-78a39de366e1"), null, null, null },
                    { new Guid("93f2689e-d1fb-4ef8-ab4a-c4cc52795ee8"), "[{\"id\":1,\"content\":\"Trung tâm tiếng Anh\",\"image\":null},{\"id\":2,\"content\":\"Học online\",\"image\":null},{\"id\":3,\"content\":\"Học gia sư\",\"image\":null},{\"id\":4,\"content\":\"Học thêm với giáo viên\",\"image\":null},{\"id\":5,\"content\":\"Không học thêm tiếng Anh\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Bạn học thêm Tiếng Anh ở đâu?", new Guid("69b1a8ab-2811-450d-b9bf-12cd91e44193"), null, null, null },
                    { new Guid("9edf0f4f-e12c-4e3f-aa24-31a97b3e2d80"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "How would you rate the importance of learning English (on a scale of 1 to 5, from least to most important)?", new Guid("92d64860-c647-4359-a326-78a39de366e1"), null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1092914c-2706-4226-8f97-579c7ae9c500"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1ad583b8-ee62-4bdb-ba56-e228062929b4"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2ca57663-8c08-43ef-b9f3-1f044e724fca"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("337a1505-8221-4967-93a9-285a31ad5da2"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("36c3bef7-7fef-4999-a553-49fa53eed5c1"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("3eae31f3-4e19-4e76-8c89-a571558f5ece"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("52248d53-305e-49b6-8260-bf78f1c91889"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("5c29c250-58cd-4675-ac3c-41a13d6c6055"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("836f815f-955b-4e08-a1ee-c7685ccec230"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8b3ceaa3-64d0-4070-b26e-56633396ca0d"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("93f2689e-d1fb-4ef8-ab4a-c4cc52795ee8"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("9edf0f4f-e12c-4e3f-aa24-31a97b3e2d80"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("69b1a8ab-2811-450d-b9bf-12cd91e44193"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("6fe5a134-6379-4244-96f1-69cc71962368"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("92d64860-c647-4359-a326-78a39de366e1"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("eb9ddecf-15fc-4393-af86-eb2b14f64eb7"));

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0bced45a-cc34-47d4-a1e5-97735f52791b"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Centres de langue anglaise\",\"image\":null},{\"id\":2,\"content\":\"Apprentissage en ligne\",\"image\":null},{\"id\":3,\"content\":\"Cours particuliers\",\"image\":null},{\"id\":4,\"content\":\"Cours suppl\\u00E9mentaires avec des enseignants\",\"image\":null},{\"id\":5,\"content\":\"Je ne prends pas de cours suppl\\u00E9mentaires d\\u0027anglais\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2b267643-bb4f-4ac4-a14a-33350fa35108"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("30adf928-8e15-4efd-9c35-e31798381a26"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Am\\u00E9liorer vos notes \\u00E0 l\\u0027\\u00E9cole\",\"image\":null},{\"id\":2,\"content\":\"Passer une certification\",\"image\":null},{\"id\":3,\"content\":\"Int\\u00E9r\\u00EAt personnel\",\"image\":null},{\"id\":4,\"content\":\"Autres raisons\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4fabc8f2-fcf1-4756-9e2e-64877d8a00da"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u1EA3i thi\\u1EC7n \\u0111i\\u1EC3m s\\u1ED1 \\u1EDF tr\\u01B0\\u1EDDng\",\"image\":null},{\"id\":2,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":null},{\"id\":3,\"content\":\"S\\u1EDF th\\u00EDch\",\"image\":null},{\"id\":4,\"content\":\"L\\u00FD do kh\\u00E1c\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66f876c3-716a-4b81-b790-e13e37fceedc"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Centres de langue anglaise\",\"image\":null},{\"id\":2,\"content\":\"Apprentissage en ligne\",\"image\":null},{\"id\":3,\"content\":\"Cours particuliers\",\"image\":null},{\"id\":4,\"content\":\"Cours suppl\\u00E9mentaires avec des enseignants\",\"image\":null},{\"id\":5,\"content\":\"Je ne prends pas de cours suppl\\u00E9mentaires d\\u0027anglais\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("70c9d600-a6a2-4d56-a740-3d0151efab1a"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Giao ti\\u1EBFp ti\\u1EBFng Anh r\\u1EA5t pro trong m\\u1EAFt b\\u1EA1n b\\u00E8\",\"image\":null},{\"id\":2,\"content\":\"\\u0110i\\u1EC3m cao \\u0111\\u01B0\\u1EE3c c\\u00F4 gi\\u00E1o khen ng\\u1EE3i tr\\u01B0\\u1EDBc l\\u1EDBp\",\"image\":null},{\"id\":3,\"content\":\"Th\\u00EDch v\\u00EC ti\\u1EBFng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Kh\\u00F4ng th\\u00EDch g\\u00EC c\\u1EA3\",\"image\":null},{\"id\":5,\"content\":\"Gh\\u00E9t ti\\u1EBFng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7910a8a2-b89d-4579-a657-de2858ad499c"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("80f7d9f6-39b2-4dff-a71e-92cf810c7b73"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Trung t\\u00E2m ti\\u1EBFng Anh\",\"image\":null},{\"id\":2,\"content\":\"H\\u1ECDc online\",\"image\":null},{\"id\":3,\"content\":\"H\\u1ECDc gia s\\u01B0\",\"image\":null},{\"id\":4,\"content\":\"H\\u1ECDc th\\u00EAm v\\u1EDBi gi\\u00E1o vi\\u00EAn\",\"image\":null},{\"id\":5,\"content\":\"Kh\\u00F4ng h\\u1ECDc th\\u00EAm ti\\u1EBFng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8bb2b7f3-8254-4e83-b13f-f43cdb8d3c18"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Am\\u00E9liorer vos notes \\u00E0 l\\u0027\\u00E9cole\",\"image\":null},{\"id\":2,\"content\":\"Passer une certification\",\"image\":null},{\"id\":3,\"content\":\"Int\\u00E9r\\u00EAt personnel\",\"image\":null},{\"id\":4,\"content\":\"Autres raisons\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("97064707-b3d8-4fb5-bf4a-12bb6eb3776e"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"\\u00CAtre consid\\u00E9r\\u00E9 comme un expert en anglais par vos amis\",\"image\":null},{\"id\":2,\"content\":\"Obtenir de bonnes notes et \\u00EAtre f\\u00E9licit\\u00E9 par le professeur en classe\",\"image\":null},{\"id\":3,\"content\":\"Aimer l\\u0027anglais parce que c\\u0027est int\\u00E9ressant\",\"image\":null},{\"id\":4,\"content\":\"Ne rien aimer du tout\",\"image\":null},{\"id\":5,\"content\":\"D\\u00E9tester l\\u0027anglais compl\\u00E8tement\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b3e92765-8b8d-4bec-a5d4-f9a9e213b53f"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u1EA3i thi\\u1EC7n \\u0111i\\u1EC3m s\\u1ED1 \\u1EDF tr\\u01B0\\u1EDDng\",\"image\":null},{\"id\":2,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":null},{\"id\":3,\"content\":\"S\\u1EDF th\\u00EDch\",\"image\":null},{\"id\":4,\"content\":\"L\\u00FD do kh\\u00E1c\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ba027423-106a-4bc6-a3a6-4b8386a44e51"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"V\\u0103n h\\u00F3a\",\"image\":\"castle 1.png\"},{\"id\":2,\"content\":\"Du l\\u1ECBch\",\"image\":\"plane 1.png\"},{\"id\":3,\"content\":\"K\\u1EBFt b\\u1EA1n\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"H\\u1ECDc t\\u1EADp\",\"image\":\"mortarboard 1.png\"},{\"id\":5,\"content\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\",\"image\":\"case 1.png\"},{\"id\":6,\"content\":\"Kh\\u00E1c....\",\"image\":\"goal 1.png\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d4d2fae7-ae44-4652-883c-68c98a2972de"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Trung t\\u00E2m ti\\u1EBFng Anh\",\"image\":null},{\"id\":2,\"content\":\"H\\u1ECDc online\",\"image\":null},{\"id\":3,\"content\":\"H\\u1ECDc gia s\\u01B0\",\"image\":null},{\"id\":4,\"content\":\"H\\u1ECDc th\\u00EAm v\\u1EDBi gi\\u00E1o vi\\u00EAn\",\"image\":null},{\"id\":5,\"content\":\"Kh\\u00F4ng h\\u1ECDc th\\u00EAm ti\\u1EBFng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d823acb4-62d8-4702-b95c-e6c5a8e49e4e"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"N\\u00E2ng cao \\u0111i\\u1EC3m s\\u1ED1\",\"image\":\"goal 1.png\"},{\"id\":2,\"content\":\"M\\u1EE5c ti\\u00EAu c\\u00F4ng vi\\u1EC7c\",\"image\":\"work-target-icon.svg\"},{\"id\":3,\"content\":\"C\\u1EA3i thi\\u1EC7n giao ti\\u1EBFp\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":\"student.svg\"},{\"id\":5,\"content\":\"Du h\\u1ECDc\",\"image\":\"plane 1.png\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("db2b2556-74cf-423e-8620-2ac1246baf1e"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Giao ti\\u1EBFp ti\\u1EBFng Anh r\\u1EA5t pro trong m\\u1EAFt b\\u1EA1n b\\u00E8\",\"image\":null},{\"id\":2,\"content\":\"\\u0110i\\u1EC3m cao \\u0111\\u01B0\\u1EE3c c\\u00F4 gi\\u00E1o khen ng\\u1EE3i tr\\u01B0\\u1EDBc l\\u1EDBp\",\"image\":null},{\"id\":3,\"content\":\"Th\\u00EDch v\\u00EC ti\\u1EBFng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Kh\\u00F4ng th\\u00EDch g\\u00EC c\\u1EA3\",\"image\":null},{\"id\":5,\"content\":\"Gh\\u00E9t ti\\u1EBFng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f12f7c70-b9b5-49c5-8122-cc9c788e3354"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"35a71ae7-49c1-4878-a1db-edcd2834f1cd\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fb59b7d2-ed36-4c5e-9fc5-4f176c9a5873"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"\\u00CAtre consid\\u00E9r\\u00E9 comme un expert en anglais par vos amis\",\"image\":null},{\"id\":2,\"content\":\"Obtenir de bonnes notes et \\u00EAtre f\\u00E9licit\\u00E9 par le professeur en classe\",\"image\":null},{\"id\":3,\"content\":\"Aimer l\\u0027anglais parce que c\\u0027est int\\u00E9ressant\",\"image\":null},{\"id\":4,\"content\":\"Ne rien aimer du tout\",\"image\":null},{\"id\":5,\"content\":\"D\\u00E9tester l\\u0027anglais compl\\u00E8tement\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("03d12e43-250b-49b7-bd08-b12135e47723"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"35a71ae7-49c1-4878-a1db-edcd2834f1cd\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"M\\u00E1y t\\u00EDnh x\\u00E1ch tay\",\"referenceQuestionId\":\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\"},{\"id\":2,\"content\":\"M\\u00E1y t\\u00EDnh \\u0111\\u1EC3 b\\u00E0n\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u1EA3 2\",\"referenceQuestionId\":\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\"},{\"id\":4,\"content\":\"No\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("14787cbf-cc43-4453-a148-6d11a683f311"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"N\\u00E2ng cao \\u0111i\\u1EC3m s\\u1ED1\",\"image\":\"goal 1.png\"},{\"id\":2,\"content\":\"M\\u1EE5c ti\\u00EAu c\\u00F4ng vi\\u1EC7c\",\"image\":\"work-target-icon.svg\"},{\"id\":3,\"content\":\"C\\u1EA3i thi\\u1EC7n giao ti\\u1EBFp\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":\"student.svg\"},{\"id\":5,\"content\":\"Du h\\u1ECDc\",\"image\":\"plane 1.png\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 T\\u01B0\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 N\\u0103m\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 S\\u00E1u\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("238cae89-e1d8-408a-bb3c-1a0b27cd0edb"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u1EA3i thi\\u1EC7n \\u0111i\\u1EC3m s\\u1ED1 \\u1EDF tr\\u01B0\\u1EDDng\",\"image\":null},{\"id\":2,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":null},{\"id\":3,\"content\":\"S\\u1EDF th\\u00EDch\",\"image\":null},{\"id\":4,\"content\":\"L\\u00FD do kh\\u00E1c\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("482c87c7-b555-4de2-a418-f990960e0ee6"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u1EA3i thi\\u1EC7n \\u0111i\\u1EC3m s\\u1ED1 \\u1EDF tr\\u01B0\\u1EDDng\",\"image\":null},{\"id\":2,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":null},{\"id\":3,\"content\":\"S\\u1EDF th\\u00EDch\",\"image\":null},{\"id\":4,\"content\":\"L\\u00FD do kh\\u00E1c\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("52548bc5-3536-478c-978b-05f98815bf31"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Nam\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"N\\u1EEF\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Kh\\u00E1c\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"1 (Kh\\u00F4ng s\\u1EB5n l\\u00F2ng ch\\u00FAt n\\u00E0o)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (R\\u1EA5t s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("651414ad-cbcf-462a-84b9-daf83880b25a"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 T\\u01B0\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 N\\u0103m\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 S\\u00E1u\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                column: "AnswerStr",
                value: "{\"countryCode\":123,\"countryName\":\"Vi\\u1EC7t Nam\",\"provinceCode\":29,\"provinceName\":\"H\\u00E0 N\\u1ED9i\"}");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"V\\u0103n h\\u00F3a\",\"image\":\"castle 1.png\"},{\"id\":2,\"content\":\"Du l\\u1ECBch\",\"image\":\"plane 1.png\"},{\"id\":3,\"content\":\"K\\u1EBFt b\\u1EA1n\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"H\\u1ECDc t\\u1EADp\",\"image\":\"mortarboard 1.png\"},{\"id\":5,\"content\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\",\"image\":\"case 1.png\"},{\"id\":6,\"content\":\"Kh\\u00E1c....\",\"image\":\"goal 1.png\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73a7a0c8-1e07-4351-b3f4-3e843623bbaf"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Trung t\\u00E2m ti\\u1EBFng Anh\",\"image\":null},{\"id\":2,\"content\":\"H\\u1ECDc online\",\"image\":null},{\"id\":3,\"content\":\"H\\u1ECDc gia s\\u01B0\",\"image\":null},{\"id\":4,\"content\":\"H\\u1ECDc th\\u00EAm v\\u1EDBi gi\\u00E1o vi\\u00EAn\",\"image\":null},{\"id\":5,\"content\":\"Kh\\u00F4ng h\\u1ECDc th\\u00EAm ti\\u1EBFng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"B\\u1EA1n \\u0111\\u00E3 bi\\u1EBFt m\\u1ED9t ch\\u00FAt Ti\\u1EBFng Anh\"},{\"id\":2,\"content\":\"\\u0110\\u00E2y l\\u00E0 l\\u1EA7n \\u0111\\u1EA7u b\\u1EA1n h\\u1ECDc Ti\\u1EBFng Anh\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("a6f6b5dc-c72a-496c-859c-2fdc9d97f147"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 T\\u01B0\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 N\\u0103m\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 S\\u00E1u\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("c6bda257-0757-4290-86c1-794d0ba6ae3b"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Giao ti\\u1EBFp ti\\u1EBFng Anh r\\u1EA5t pro trong m\\u1EAFt b\\u1EA1n b\\u00E8\",\"image\":null},{\"id\":2,\"content\":\"\\u0110i\\u1EC3m cao \\u0111\\u01B0\\u1EE3c c\\u00F4 gi\\u00E1o khen ng\\u1EE3i tr\\u01B0\\u1EDBc l\\u1EDBp\",\"image\":null},{\"id\":3,\"content\":\"Th\\u00EDch v\\u00EC ti\\u1EBFng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Kh\\u00F4ng th\\u00EDch g\\u00EC c\\u1EA3\",\"image\":null},{\"id\":5,\"content\":\"Gh\\u00E9t ti\\u1EBFng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("c88e157b-1496-4aae-b81f-dd0718524e38"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Giao ti\\u1EBFp ti\\u1EBFng Anh r\\u1EA5t pro trong m\\u1EAFt b\\u1EA1n b\\u00E8\",\"image\":null},{\"id\":2,\"content\":\"\\u0110i\\u1EC3m cao \\u0111\\u01B0\\u1EE3c c\\u00F4 gi\\u00E1o khen ng\\u1EE3i tr\\u01B0\\u1EDBc l\\u1EDBp\",\"image\":null},{\"id\":3,\"content\":\"Th\\u00EDch v\\u00EC ti\\u1EBFng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Kh\\u00F4ng th\\u00EDch g\\u00EC c\\u1EA3\",\"image\":null},{\"id\":5,\"content\":\"Gh\\u00E9t ti\\u1EBFng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("da618b05-f9b8-4bfd-8bae-7a5adb668d16"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Trung t\\u00E2m ti\\u1EBFng Anh\",\"image\":null},{\"id\":2,\"content\":\"H\\u1ECDc online\",\"image\":null},{\"id\":3,\"content\":\"H\\u1ECDc gia s\\u01B0\",\"image\":null},{\"id\":4,\"content\":\"H\\u1ECDc th\\u00EAm v\\u1EDBi gi\\u00E1o vi\\u00EAn\",\"image\":null},{\"id\":5,\"content\":\"Kh\\u00F4ng h\\u1ECDc th\\u00EAm ti\\u1EBFng Anh\",\"image\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"1 (Kh\\u00F4ng s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (R\\u1EA5t s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"33 L\\u1EA1c Trung\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"125 Ho\\u00E0ng Ng\\u00E2n\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"T\\u00F4i \\u1ED5n v\\u1EDBi c\\u1EA3 hai\",\"referenceQuestionId\":null}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f563da40-d609-4922-90b9-44e4290edfef"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Ba \\u0110\\u00ECnh\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Ho\\u00E0n Ki\\u1EBFm\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"T\\u00E2y H\\u1ED3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"Long Bi\\u00EAn\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u1EA7u Gi\\u1EA5y\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"\\u0110\\u1ED1ng \\u0110a\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"Hai B\\u00E0 Tr\\u01B0ng\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"Ho\\u00E0ng Mai\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"Thanh Xu\\u00E2n\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"H\\u00E0 \\u0110\\u00F4ng\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"B\\u1EAFc T\\u1EEB Li\\u00EAm\",\"referenceQuestionId\":null},{\"id\":12,\"content\":\"Nam T\\u1EEB Li\\u00EAm\",\"referenceQuestionId\":null},{\"id\":13,\"content\":\"Ba V\\u00EC\",\"referenceQuestionId\":null},{\"id\":14,\"content\":\"Ch\\u01B0\\u01A1ng M\\u1EF9\",\"referenceQuestionId\":null},{\"id\":15,\"content\":\"\\u0110an Ph\\u01B0\\u1EE3ng\",\"referenceQuestionId\":null},{\"id\":16,\"content\":\"\\u0110\\u00F4ng Anh\",\"referenceQuestionId\":null},{\"id\":17,\"content\":\"Gia L\\u00E2m\",\"referenceQuestionId\":null},{\"id\":18,\"content\":\"Ho\\u00E0i \\u0110\\u1EE9c\",\"referenceQuestionId\":null},{\"id\":19,\"content\":\"M\\u00EA Linh\",\"referenceQuestionId\":null},{\"id\":20,\"content\":\"Ph\\u00FA Xuy\\u00EAn\",\"referenceQuestionId\":null},{\"id\":21,\"content\":\"Ph\\u00FAc Th\\u1ECD\",\"referenceQuestionId\":null},{\"id\":22,\"content\":\"Qu\\u1ED1c Oai\",\"referenceQuestionId\":null},{\"id\":23,\"content\":\"Th\\u1EA1ch Th\\u1EA5t\",\"referenceQuestionId\":null},{\"id\":24,\"content\":\"Thanh Oai\",\"referenceQuestionId\":null},{\"id\":25,\"content\":\"Thanh Tr\\u00EC\",\"referenceQuestionId\":null},{\"id\":26,\"content\":\"Th\\u01B0\\u1EDDng T\\u00EDn\",\"referenceQuestionId\":null},{\"id\":27,\"content\":\" \\u1EE8ng H\\u00F2a\",\"referenceQuestionId\":null},{\"id\":28,\"content\":\"kh\\u00E1c\",\"referenceQuestionId\":null}]");
        }
    }
}
