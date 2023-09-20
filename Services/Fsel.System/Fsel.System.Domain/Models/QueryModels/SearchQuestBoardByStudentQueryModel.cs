// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchQuestBoardByStudentQueryModel : BaseQueryModel
    {
        public EnumQuestBoardType? Type { get; set; }
    }
}
