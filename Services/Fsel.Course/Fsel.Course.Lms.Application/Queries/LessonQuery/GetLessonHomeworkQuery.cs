// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
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

    public class GetLessonHomeworkQuery : IRequest<MethodResult<IList<LessonHomeworkSearchModel>>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetLessonHomeworkQueryHandler : IRequestHandler<GetLessonHomeworkQuery, MethodResult<IList<LessonHomeworkSearchModel>>>
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

        public GetLessonHomeworkQueryHandler(AuthContext authContext,
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

        public async Task<MethodResult<IList<LessonHomeworkSearchModel>>> Handle(GetLessonHomeworkQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<LessonHomeworkSearchModel>> methodResult = new MethodResult<IList<LessonHomeworkSearchModel>>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId.ToString());
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.UserIdNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            //var lessonSkillScoreQuery = from lr in _lessonResultRepository.Queryable
            //                            join hr in _homeWorkResultRepository.Queryable on lr.Id equals hr.LessonResultId
            //                            join h in _homeWorkRepository.Queryable on hr.HomeWorkId equals h.Id
            //                            join ha in _homeWorkAnswerRepository.Queryable on hr.Id equals ha.HomeWorkResultId
            //                            join hq in _homeWorkQuestionRepository.Queryable on ha.HomeWorkQuestionId equals hq.Id
            //                            join q in _questionRepository.Queryable on hq.QuestionId equals q.Id
            //                            where lr.StudentId == studentId
            //                            group new { q, ha, h } by h.CourseSkill into g
            //                            select new LessonHomeworkSearchModel
            //                            {
            //                                TotalCount = g.Select(x => x.q).Count(),
            //                                CompletedCount = g.Select(x => x.ha).Count(),
            //                                Percent = g.Select(x => x.q).Count() > 0 ? (g.Select(x => x.ha).Count() / g.Select(x => x.q).Count()) * 100 : 0,
            //                                HomeWork = _mapper.Map<HomeWorkModel>(g.Select(x => x.h).FirstOrDefault())
            //                            };

            var lessonSkillScoreQuery = _lessonResultRepository.Queryable
                            .Join(_homeWorkResultRepository.Queryable, lr => lr.Id, hr => hr.LessonResultId, (lr, hr) => new { lr, hr })
                            .Join(_homeWorkRepository.Queryable, lr_hr => lr_hr.hr.HomeWorkId, h => h.Id, (lr_hr, h) => new { lr_hr.lr, lr_hr.hr, h })
                            .Join(_homeWorkAnswerRepository.Queryable, lr_hr_h => lr_hr_h.hr.Id, ha => ha.HomeWorkResultId, (lr_hr_h, ha) => new { lr_hr_h.lr, lr_hr_h.hr, lr_hr_h.h, ha })
                            .Join(_homeWorkQuestionRepository.Queryable, lr_hr_h_ha => lr_hr_h_ha.ha.HomeWorkQuestionId, hq => hq.Id, (lr_hr_h_ha, hq) => new { lr_hr_h_ha.lr, lr_hr_h_ha.hr, lr_hr_h_ha.h, lr_hr_h_ha.ha, hq })
                            .Join(_questionRepository.Queryable, lr_hr_h_ha_hq => lr_hr_h_ha_hq.hq.QuestionId, q => q.Id, (lr_hr_h_ha_hq, q) => new { lr_hr_h_ha_hq.lr, lr_hr_h_ha_hq.hr, lr_hr_h_ha_hq.h, lr_hr_h_ha_hq.ha, lr_hr_h_ha_hq.hq, q })
                            .Where(x => x.lr.StudentId == studentId)
                            .GroupBy(x => x.h.CourseSkill)
                            .AsNoTracking()
                            .Select(g => new LessonHomeworkSearchModel
                            {
                                TotalCount = g.Select(x => x.q).Count(),
                                CompletedCount = g.Select(x => x.ha).Count(),
                                Percent = (g.Select(x => x.ha).Count() / g.Select(x => x.q).Count()) * 100,
                                HomeWork = _mapper.Map<HomeWorkModel>(g.Select(x => x.h).FirstOrDefault())
                            });

            methodResult.Result = await lessonSkillScoreQuery.ToListAsync(cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
