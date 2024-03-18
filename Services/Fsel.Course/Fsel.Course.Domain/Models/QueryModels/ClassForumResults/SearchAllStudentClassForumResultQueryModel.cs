// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ClassForumResults
{
    using Fsel.Core.Base.BaseModels;

    public class SearchAllStudentClassForumResultQueryModel : BaseQueryModel
    {
        public Guid LessonResultId { get; set; }

        public Guid? ClassForumResultId { get; set; }
    }
}
