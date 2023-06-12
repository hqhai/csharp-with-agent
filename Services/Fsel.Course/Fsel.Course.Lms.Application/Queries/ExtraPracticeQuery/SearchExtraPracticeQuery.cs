// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ExtraPractices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchExtraPracticeQuery : SearchExtraPracticeLmsQueryModel, IRequest<MethodResult<PagingItemsModel<ExtraPracticeModel>>>
    {
    }

    public class SearchExtraPracticeQueryHandler : IRequestHandler<SearchExtraPracticeQuery, MethodResult<PagingItemsModel<ExtraPracticeModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public SearchExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ExtraPracticeModel>>> Handle(SearchExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<ExtraPracticeModel>>();
            var extraPracticeQuery = _extraPracticeRepository.Queryable.Include(x => x.LessonExtraPractices)
                                                        .ThenInclude(x => x.Lesson)
                                                        .ThenInclude(x => x!.UnitLessons.Where(n => !n.IsDeleted))
                                                        .ThenInclude(x => x.Unit)
                                                        .Include(x => x.ExtraPracticeChapters.Where(n => !n.IsDeleted))
                                                        .ThenInclude(x => x.ExtraPracticeExercises.Where(n => !n.IsDeleted))
                                                        .ThenInclude(x => x.Exercise)
                                                        .Include(x => x.ExtraPracticeExercises.Where(n => !n.IsDeleted))
                                                        .ThenInclude(x => x.Exercise)
                                                        .Include(x => x.Video)
                                                        .ThenInclude(x => x!.VideoTimeCodes.Where(x => !x.IsDeleted))
                                                        .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                                        .ThenInclude(x => x.Exercise)
                                                        .Select(x => new ExtraPracticeModel
                                                        {
                                                            Id = x.Id,
                                                            Code = x.Code,
                                                            Name = x.Name,
                                                            Type = x.Type,
                                                            FilePaths = x.FilePaths,
                                                            CourseLevel = x.CourseLevel,
                                                            Exercises = x.ExtraPracticeChapters != null ? x.ExtraPracticeChapters.Where(x => !x.IsDeleted).SelectMany(x => x.ExtraPracticeExercises.Where(x => x.Exercise != null && !x.IsDeleted)).Select(x => x.Exercise).Select(x => new ExerciseModel
                                                            {
                                                                CourseSkill = x!.CourseSkill
                                                            }).ToList() : x.ExtraPracticeExercises != null ? x.ExtraPracticeExercises.Where(x => x.Exercise != null && !x.IsDeleted).Select(x => x.Exercise).Select(x => new ExerciseModel
                                                            {
                                                                CourseSkill = x!.CourseSkill
                                                            }).ToList() : x.Video != null ? x.Video.VideoTimeCodes.SelectMany(x => x.TimeCodeExercises.Where(x => x.Exercise != null && !x.IsDeleted)).Select(x => x.Exercise).Select(x => new ExerciseModel
                                                            {
                                                                CourseSkill = x!.CourseSkill
                                                            }).ToList() : default,

                                                        });
            int totalItem = await extraPracticeQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await extraPracticeQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<ExtraPracticeModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
