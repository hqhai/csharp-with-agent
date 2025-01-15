// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;

    public interface IQuestionShuffleRepository : IRepository<QuestionShuffle>
    {
        Task<VoidMethodResult> SaveQuestionShufflesAsync(IList<QuestionShuffle> questionShuffles);
    }
}
