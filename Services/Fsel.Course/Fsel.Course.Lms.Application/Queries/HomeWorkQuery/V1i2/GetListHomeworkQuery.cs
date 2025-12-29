// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery.V1i2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListHomeworkQuery : IRequest<MethodResult<IList<LessonHomeWorkResultModel>>>
    {
        public Guid LessonResultId { get; set; }
        public Guid? LessonModuleId { get; set; }
    }

    public class GetListHomeworkQueryHandler : IRequestHandler<GetListHomeworkQuery, MethodResult<IList<LessonHomeWorkResultModel>>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IMapper _mapper;

        public GetListHomeworkQueryHandler(
            IHomeWorkRepository homeWorkRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            ILessonResultRepository lessonResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IMapper mapper
            )
        {
            _homeWorkRepository = homeWorkRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonHomeWorkResultModel>>> Handle(GetListHomeworkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonHomeWorkResultModel>>();

            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }

            if (lessonResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var moduleResults = await GetHomeWorkResultsAsync(request.LessonModuleId, lessonResult);
            var homeWorkResultIds = moduleResults.Select(x => x.HomeWorkResult).Where(x => x != null).Select(x => x!.Id).ToList();

            List<HomeWorkAnswer> homeWorkResultAnswerQuerys = new List<HomeWorkAnswer>();

            if (homeWorkResultIds != null)
            {
                homeWorkResultAnswerQuerys = await _homeWorkAnswerRepository.Queryable
                                                                            .WhereBulkContains(homeWorkResultIds, x => x.HomeWorkResultId)
                                                                            .Where(x => x.IsCorrect.HasValue)
                                                                            .ToListAsync(cancellationToken);
            }

            var homeWorkResultAnswers = homeWorkResultAnswerQuerys.GroupBy(x => x.HomeWorkResultId)
                                                                  .Select(x => new
                                                                  {
                                                                      HomeWorkResultId = x.Key,
                                                                      QuestionCompleted = x.Select(x => x).Where(x => x.IsCorrect.HasValue).Count()
                                                                  }).ToList();

            methodResult.Result = moduleResults.Where(x => x.LessonModule != null).OrderBy(x => x.LessonModule!.DisplayOrder).Select(x =>
            {
                var homeWorkResult = x.HomeWorkResult;
                var homeWorkResultAnswer = homeWorkResultAnswers.FirstOrDefault(x => homeWorkResult != null && x.HomeWorkResultId == homeWorkResult.Id);
                var homeWork = GetHomeWork(x.HomeWork, homeWorkResultAnswer?.QuestionCompleted, homeWorkResult);
                homeWork.LessonModuleId = x.LessonModule!.Id;
                return homeWork;
            }).OrderBy(x => x.CreatedDate).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public class ModuleHomeWorkResult
        {
            public HomeWork? HomeWork { get; set; }
            public HomeWorkResult? HomeWorkResult { get; set; }
            public LessonModule? LessonModule { get; set; }
        }

        private async Task<IList<ModuleHomeWorkResult>> GetHomeWorkResultsAsync(
        Guid? lessonModuleId,
        LessonResult lessonResult)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);

            // 1) LessonModules (HomeWork type)
            var lessonModules = await _lessonModuleRepository.ReadQueryable
                .AsNoTracking()
                .Where(m => m.LessonId == lessonResult.LessonId
                            && m.LessonConfigType == EnumLessonConfigType.HomeWork
                            && (!lessonModuleId.HasValue || m.Id == lessonModuleId.Value))
                .Select(m => new
                {
                    m.Id,
                    m.OriginalId,
                    Entity = m
                })
                .ToListAsync();

            if (lessonModules.Count == 0)
            {
                return new List<ModuleHomeWorkResult>();
            }
            var moduleIds = lessonModules.Select(x => x.Id).ToList();
            var originalIds = lessonModules.Select(x => x.OriginalId).Distinct().ToList();

            // 2) HomeWorkResults by LessonModuleId
            var homeWorkResults = await _homeWorkResultRepository.ReadQueryable
                .AsNoTracking()
                .Where(r => r.LessonResultId == lessonResult.Id)
                .ToListAsync();

            var resultByModuleId = homeWorkResults.Where(x => x.LessonModuleId.HasValue)
                .GroupBy(r => r.LessonModuleId!.Value)
                .ToDictionary(g => g.Key, g => g
                    .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                    .ThenByDescending(x => x.Id)
                    .First());

            // 3) HomeWork snapshot theo Result.HomeWorkId (ưu tiên đúng version lúc làm)
            var homeWorkIdsFromResults = homeWorkResults
                .Select(x => x.HomeWorkId)
                .Distinct()
                .ToList();

            var homeWorksById = await _homeWorkRepository.ReadQueryable
                .AsNoTracking()
                .Include(x => x.HomeWorkQuestions)
                .Where(hw => homeWorkIdsFromResults.Contains(hw.Id))
                .ToDictionaryAsync(hw => hw.Id);

            // 4) HomeWork LastVersion theo OriginalId (fallback)
            var lastVersionHomeWorks = await _homeWorkRepository.ReadQueryable
                .AsNoTracking()
                .Include(x => x.HomeWorkQuestions)
                .Where(hw => originalIds.Contains(hw.OriginalId) && hw.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync();

            var lastHomeWorkByOriginalId = lastVersionHomeWorks
                .Where(x => x != null)
                .ToDictionary(x => x.OriginalId, x => x);

            // 5) Map
            var results = new List<ModuleHomeWorkResult>(lessonModules.Count);

            foreach (var m in lessonModules)
            {
                resultByModuleId.TryGetValue(m.Id, out var r);

                HomeWork? hw = null;

                // Ưu tiên theo HomeWorkId trong result
                if (r != null && homeWorksById.TryGetValue(r.HomeWorkId, out var hwById))
                {
                    hw = hwById;
                }
                else
                {
                    // fallback LastVersion theo OriginalId
                    lastHomeWorkByOriginalId.TryGetValue(m.OriginalId, out hw);
                }

                results.Add(new ModuleHomeWorkResult
                {
                    LessonModule = m.Entity,
                    HomeWorkResult = r,
                    HomeWork = hw
                });
            }

            return results;
        }

        private LessonHomeWorkResultModel GetHomeWork(HomeWork? h, int? questionCompleted, HomeWorkResult? homeWorkResult)
        {
            var homeWork = _mapper.Map<LessonHomeWorkResultModel>(h);
            homeWork.QuestionTotal = h.HomeWorkQuestions.Count;
            homeWork.QuestionCompleted = questionCompleted ?? default;
            homeWork.HomeWorkResult = _mapper.Map<HomeWorkResultModel>(homeWorkResult);
            return homeWork;
        }
    }
}
