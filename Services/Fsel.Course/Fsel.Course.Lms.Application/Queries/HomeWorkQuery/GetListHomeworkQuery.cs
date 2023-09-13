// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
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
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetListHomeworkQueryHandler(AuthContext authContext,
            IUserService userService,
            IHomeWorkRepository homeWorkRepository,
            ILessonResultRepository lessonResultRepository,
            IMapper mapper
            )
        {
            _authContext = authContext;
            _userService = userService;
            _homeWorkRepository = homeWorkRepository;
            _lessonResultRepository = lessonResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonHomeWorkResultModel>>> Handle(GetListHomeworkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonHomeWorkResultModel>>();
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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
            if (lessonResult.Status == Domain.Enums.EnumResultStatus.Unfinished || lessonResult.Status == Domain.Enums.EnumResultStatus.New)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var homeWorks = await _homeWorkRepository.Queryable
                                        .Include(x => x!.LessonHomeWorks)
                                        .Include(x => x!.HomeWorkQuestions)
                                        .ThenInclude(x => x.Question)
                                        .Include(x => x.HomeWorkResults)
                                        .ThenInclude(x => x.HomeWorkAnswers)
                                        .Where(x => x.LessonHomeWorks.Any(x => x.LessonId == lessonResult.LessonId))
                                        .AsNoTracking()
                                        .Select(h => new LessonHomeWorkResultModel
                                        {
                                            Id = h.Id,
                                            CreatedDate = h.LessonHomeWorks.FirstOrDefault(x => x.HomeWorkId == h.Id && x.LessonId == lessonResult.LessonId)!.CreatedDate,
                                            Code = h.Code,
                                            Name = h.Name,
                                            CourseSkill = h.CourseSkill,
                                            CourseLevel = h.CourseLevel,
                                            QuestionTotal = h.HomeWorkQuestions.Select(x => x.Question).Count(),
                                            QuestionCompleted = h.HomeWorkResults.FirstOrDefault(x => x.HomeWorkId == h.Id && x.LessonResultId == request.LessonResultId)!.HomeWorkAnswers.Count,
                                            HomeWorkResult = _mapper.Map<HomeWorkResultModel>(h.HomeWorkResults.FirstOrDefault(x => x.HomeWorkId == h.Id && x.LessonResultId == request.LessonResultId))
                                        })
                                        .ToListAsync(cancellationToken);
            methodResult.Result = homeWorks.OrderBy(x => x.CreatedDate).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
