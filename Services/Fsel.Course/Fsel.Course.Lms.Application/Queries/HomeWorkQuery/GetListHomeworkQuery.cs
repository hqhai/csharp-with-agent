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

    public class GetListHomeworkQuery : IRequest<MethodResult<IList<LessonHomeWorkResultModel>>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetListHomeworkQueryHandler : IRequestHandler<GetListHomeworkQuery, MethodResult<IList<LessonHomeWorkResultModel>>>
    {
        private readonly IUserService _userService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetListHomeworkQueryHandler(AuthContext authContext,
            IUserService userService,
            ILessonResultRepository lessonResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IMapper mapper
            )
        {
            _authContext = authContext;
            _userService = userService;
            _lessonResultRepository = lessonResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonHomeWorkResultModel>>> Handle(GetListHomeworkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonHomeWorkResultModel>>();
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonResultErrorCode.LessonResultNotExist));
                return methodResult;
            }
            var lessonSkillScoreQuery = await _homeWorkResultRepository.Queryable
                                        .Include(x => x.HomeWork)
                                        .ThenInclude(x => x!.LessonHomeWorks)
                                        .Include(x => x.HomeWork)
                                        .ThenInclude(x => x!.HomeWorkQuestions.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedDate))
                                        .ThenInclude(x => x.Question)
                                        .Include(x => x.HomeWorkAnswers.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedDate))
                                        .Where(x => x.HomeWork != null && x.LessonResultId == request.LessonResultId)
                                        .AsNoTracking()
                                        .Select(h => new LessonHomeWorkResultModel
                                        {
                                            Id = h.HomeWork!.Id,
                                            CreatedDate = h.HomeWork.LessonHomeWorks.FirstOrDefault(x => x.HomeWorkId == h.HomeWorkId && x.LessonId == lessonResult.LessonId)!.CreatedDate,
                                            Code = h.HomeWork.Code,
                                            Name = h.HomeWork.Name,
                                            CourseSkill = h.HomeWork.CourseSkill,
                                            CourseLevel = h.HomeWork.CourseLevel,
                                            QuestionTotal = h.HomeWork.HomeWorkQuestions.Select(x => x.Question).Count(),
                                            QuestionCompleted = h.HomeWorkAnswers.Count(),
                                            HomeWorkResult = _mapper.Map<HomeWorkResultModel>(h)
                                        }).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);

            methodResult.Result = lessonSkillScoreQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
