// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.LessonItemServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;

    public interface ILessonItemInitializer
    {
        /// <summary>
        /// Tạo item đầu tiên cho lesson (Video/ClassForum/HomeWork/Document...).
        /// </summary>
        Task<VoidMethodResult> InitializeAsync(LessonModule lessonModule, LessonResult lessonResult, CancellationToken cancellationToken);
    }
}
