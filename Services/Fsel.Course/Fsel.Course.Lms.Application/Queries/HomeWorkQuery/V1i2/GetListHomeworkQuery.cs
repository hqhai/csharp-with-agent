// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery.V1i2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
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

        private async Task<IList<ModuleHomeWorkResult>> GetHomeWorkResultsAsync(Guid? lessonModuleId, LessonResult lessonResult)
        {
            var query = from baseQ in _homeWorkRepository.Queryable.Include(x => x.HomeWorkQuestions)

                        join lessonModule in _lessonModuleRepository.Queryable on baseQ.OriginalId equals lessonModule.OriginalId
                        where lessonModule.LessonId == lessonResult.LessonId && lessonModule.LessonConfigType == EnumLessonConfigType.HomeWork
                        && (!lessonModuleId.HasValue || lessonModule.Id == lessonModuleId)

                        join homeWorkResult in _homeWorkResultRepository.Queryable.Where(x => x.LessonResultId == lessonResult.Id)
                                on lessonModule.Id equals homeWorkResult.LessonModuleId into homeWorkResultJoin
                        from homeWorkResult in homeWorkResultJoin.DefaultIfEmpty()
                        select new ModuleHomeWorkResult
                        {
                            HomeWork = baseQ,
                            HomeWorkResult = homeWorkResult,
                            LessonModule = lessonModule,
                        };
            var data = await query.ToListAsync();
            return data;
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
