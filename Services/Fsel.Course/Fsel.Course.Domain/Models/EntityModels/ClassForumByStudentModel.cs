// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;

    public class ClassForumByStudentModel : ClassForumModel
    {
        public ClassForumResultModel? ClassForumResultCurrentStudent { get; set; }

        public IList<ClassForumResultModel>? ClassForumResultAllStudents { get; set; }
    }
}
