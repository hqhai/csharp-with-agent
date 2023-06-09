// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ExtraPractices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchExtraPracticeQuery : SearchExtraPracticeLmsQueryModel, IRequest<MethodResult<IList<ExtraPracticeModel>>>
    {
    }

    public class SearchExtraPracticeQueryHandler : IRequestHandler<SearchExtraPracticeQuery, MethodResult<IList<ExtraPracticeModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public SearchExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
        }

        public async Task<MethodResult<IList<ExtraPracticeModel>>> Handle(SearchExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<ExtraPracticeModel>>();
            var query =  _extraPracticeRepository.Queryable.Include(x => x.LessonExtraPractices)
                                                        .ThenInclude(x => x.Lesson)
                                                        .ThenInclude(x => x.UnitLessons)
                                                        .ThenInclude(x => x.Unit)
                                                        .Include(x => x.ExtraPracticeChapters.Where(n => !n.IsDeleted))
                                                        .ThenInclude(x => x.ExtraPracticeExercises.Where(n => !n.IsDeleted))
                                                        .ThenInclude(x => x.Exercise)
                                                        .ThenInclude(x => x!.ExerciseQuestions.Where(n => !n.IsDeleted))
                                                        .ThenInclude(x => x.Question)
                                                        .Include(x => x.LessonExtraPractices.Where(n => !n.IsDeleted))
                                                        .Include(x => x.Video)
                                                        .ThenInclude(x => x!.VideoTimeCodes.Where(x => !x.IsDeleted))
                                                        .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                                        .ThenInclude(x => x.Exercise)
                                                        .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                                        .ThenInclude(x => x.Question)
                                                        .Select(x => new )
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
