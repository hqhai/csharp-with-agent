// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.ClassForumResultServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class ClassForumResultModel : BaseModel
    {
        public string? Content { get; set; }

        public ClassForumModel? ClassForum { get; set; }

    }

    public class ClassForumModel : BaseModel
    {
        public IList<ClassForumResultModel>? ClassForumResults { get; set; }
    }


}
