// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListHomeworkQuery : IRequest<MethodResult<IList<LessonHomeWorkResultModel>>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetListHomeworkQueryHandler : IRequestHandler<GetListHomeworkQuery, MethodResult<IList<LessonHomeWorkResultModel>>>
    {
        private readonly IUserService _userService;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetListHomeworkQueryHandler(AuthContext authContext,
            IUserService userService,
            IHomeWorkRepository homeWorkRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            ILessonResultRepository lessonResultRepository,
            IMapper mapper
            )
        {
            _authContext = authContext;
            _userService = userService;
            _homeWorkRepository = homeWorkRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _lessonResultRepository = lessonResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonHomeWorkResultModel>>> Handle(GetListHomeworkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonHomeWorkResultModel>>();
            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId && x.StudentId == studentId, cancellationToken);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }
            if (lessonResult.Status == EnumResultStatus.Unfinished || lessonResult.Status == EnumResultStatus.New)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var homeWorks = await _homeWorkRepository.GetListAsync(lessonResult);
            var homeWorkResultIds = homeWorks.SelectMany(x => x.HomeWorkResults).Select(x => x.Id).ToList();

            List<HomeWorkAnswer> homeWorkResultAnswerQuerys = new List<HomeWorkAnswer>();

            if (homeWorkResultIds != null)
            {
                homeWorkResultAnswerQuerys = await _homeWorkAnswerRepository.Queryable
                                                                            .WhereBulkContains(homeWorkResultIds, x => x.HomeWorkResultId)
                                                                            .Where(x => x.CreatedDate >= lessonResult.CreatedDate)
                                                                            .Where(x => !(lessonResult.UpdatedDate.HasValue && lessonResult.Status == EnumResultStatus.Done) || x.CreatedDate <= lessonResult.UpdatedDate)
                                                                            .Where(x => x.IsCorrect.HasValue)
                                                                            .ToListAsync(cancellationToken);
            }

            var homeWorkResultAnswers = homeWorkResultAnswerQuerys.GroupBy(x => x.HomeWorkResultId)
                                                                  .Select(x => new
                                                                  {
                                                                      HomeWorkResultId = x.Key,
                                                                      QuestionCompleted = x.Select(x => x).Where(x => x.IsCorrect.HasValue).Count()
                                                                  }).ToList();

            methodResult.Result = homeWorks.Select(x =>
            {
                var homeWorkResult = x.HomeWorkResults.FirstOrDefault();
                var homeWorkResultAnswer = homeWorkResultAnswers.FirstOrDefault(x => homeWorkResult != null && x.HomeWorkResultId == homeWorkResult.Id);
                var homeWork = GetHomeWork(x, homeWorkResultAnswer?.QuestionCompleted, homeWorkResult);
                return homeWork;
            }).OrderBy(x => x.CreatedDate).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private LessonHomeWorkResultModel GetHomeWork(HomeWork h, int? questionCompleted, HomeWorkResult? homeWorkResult)
        {
            var homeWork = _mapper.Map<LessonHomeWorkResultModel>(h);
            homeWork.CreatedDate = h.LessonHomeWorks.FirstOrDefault()?.CreatedDate;
            homeWork.QuestionTotal = h.HomeWorkQuestions.Count;
            homeWork.QuestionCompleted = questionCompleted ?? default;
            homeWork.HomeWorkResult = _mapper.Map<HomeWorkResultModel>(homeWorkResult);
            return homeWork;
        }
    }
}
