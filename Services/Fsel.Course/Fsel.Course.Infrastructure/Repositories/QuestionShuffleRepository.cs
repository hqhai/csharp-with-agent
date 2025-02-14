// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class QuestionShuffleRepository : BaseRepository<QuestionShuffle>, IQuestionShuffleRepository
    {
        public QuestionShuffleRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<VoidMethodResult> SaveQuestionShufflesAsync(IList<QuestionShuffle> questionShuffles)
        {
            var createQuestionShuffles = questionShuffles.Where(x => x.Id == Guid.Empty).ToList();
            var updateQuestionShuffles = questionShuffles.Where(x => x.Id != Guid.Empty).ToList();

            VoidMethodResult methodResult = new VoidMethodResult();
            try
            {
                if (createQuestionShuffles != null && createQuestionShuffles.Any())
                {
                    await AddList(createQuestionShuffles);
                    await UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                }
                await ExecuteTransactionAsync(async () =>
                {
                    if (updateQuestionShuffles != null && updateQuestionShuffles.Any())
                    {
                        UpdateList(updateQuestionShuffles);
                        await UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                    }
                    return methodResult;
                });
            }
            catch { }

            return methodResult;
        }
    }
}
