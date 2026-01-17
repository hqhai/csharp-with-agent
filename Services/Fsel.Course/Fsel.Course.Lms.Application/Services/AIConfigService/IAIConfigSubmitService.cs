// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.AIConfigService
{
    using Fsel.Course.Domain.Enums;

    /// <summary>
    /// Service gửi request lên AI dựa trên config từ AICriteriaConfigs
    /// </summary>
    public interface IAIConfigSubmitService
    {
        /// <summary>
        /// Gửi request lên AI dựa trên config từ AICriteriaConfigs theo ObjectId
        /// </summary>
        /// <param name="objectId">ObjectId của business entity (Question, ClassForum, HomeWork, etc.)</param>
        /// <param name="content">Nội dung cần AI xử lý (answer, response, data, etc.)</param>
        /// <param name="subFeatureType">Loại sub-feature (VideoLesson, HomeWork, ClassForumSpeaking, etc.)</param>
        /// <param name="featureMultiple">Loại feature lớn (Unit, Lesson, Test)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>AI Response string, null nếu không tìm thấy config</returns>
        Task<string?> SubmitByObjectIdAsync(
            Guid? objectId,
            string content,
            EnumSubFeatureType subFeatureType,
            EnumFeatureMultiple featureMultiple,
            CancellationToken cancellationToken);
    }
}
