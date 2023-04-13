// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListHomeworkQuery : IRequest<MethodResult<IList<LessonHomeworkSearchModel>>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetListHomeworkQueryHandler : IRequestHandler<GetListHomeworkQuery, MethodResult<IList<LessonHomeworkSearchModel>>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUserService _userService;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetListHomeworkQueryHandler(AuthContext authContext,
            ILessonResultRepository lessonResultRepository,
            IUserService userService,
            IHomeWorkRepository homeWorkRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IQuestionRepository questionRepository,
            IMapper mapper
            )
        {
            _authContext = authContext;
            _lessonResultRepository = lessonResultRepository;
            _userService = userService;
            _homeWorkRepository = homeWorkRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _questionRepository = questionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonHomeworkSearchModel>>> Handle(GetListHomeworkQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<LessonHomeworkSearchModel>>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId.ToString());
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.UserIdNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var lessonSkillScoreQuery = from lr in _lessonResultRepository.Queryable
                                        join hr in _homeWorkResultRepository.Queryable on lr.Id equals hr.LessonResultId
                                        join h in _homeWorkRepository.Queryable on hr.HomeWorkId equals h.Id
                                        join ha in _homeWorkAnswerRepository.Queryable on hr.Id equals ha.HomeWorkResultId
                                        join hq in _homeWorkQuestionRepository.Queryable on ha.HomeWorkQuestionId equals hq.Id
                                        join q in _questionRepository.Queryable on hq.QuestionId equals q.Id
                                        where lr.StudentId == studentId && lr.Id == request.LessonResultId
                                        group new { q, ha, h } by h.CourseSkill into g
                                        select new LessonHomeworkSearchModel
                                        {
                                            TotalCount = g.Select(x => x.q).Sum(x => x.CorrectTotal),
                                            CompletedCount = g.Select(x => x.ha).Sum(x => x.CorrectCount),
                                            HomeWork = _mapper.Map<HomeWorkModel>(g.Select(x => x.h).FirstOrDefault())
                                        };

            methodResult.Result = await lessonSkillScoreQuery.ToListAsync(cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
