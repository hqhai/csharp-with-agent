// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class QuestionShuffleRepository : BaseRepository<QuestionShuffle>, IQuestionShuffleRepository
    {
        public QuestionShuffleRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
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
                    await BulkMergeAsync(createQuestionShuffles, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.StudentId, entity.QuestionId };
                    });
                }
                if (updateQuestionShuffles != null && updateQuestionShuffles.Any())
                {
                    await ExecuteTransactionAsync(async () =>
                    {
                        await BulkUpdateList(updateQuestionShuffles, bulk =>
                        {
                            bulk.IgnoreOnUpdateExpression = entity => new { entity.StudentId, entity.QuestionId, entity.IsDeleted };
                        });
                        return methodResult;
                    });
                }
            }
            catch { }

            return methodResult;
        }
    }
}
