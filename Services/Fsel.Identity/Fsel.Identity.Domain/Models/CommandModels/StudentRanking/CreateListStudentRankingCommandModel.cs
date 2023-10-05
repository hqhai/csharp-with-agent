// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.StudentRanking
{
    public class CreateListStudentRankingCommandModel
    {
        public IList<CreateStudentRankingCommandModel>? StudentRankings { get; set; }
    }
}
