// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumResultQuery : IRequest<MethodResult<ClassForumResultModel>>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class GetClassForumResultQueryHandler : IRequestHandler<GetClassForumResultQuery, MethodResult<ClassForumResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetClassForumResultQueryHandler(AuthContext authContext, IClassForumResultRepository classForumResultRepository, IMapper mapper, IUserService userService)
        {
            _authContext = authContext;
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(GetClassForumResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();
            var classForumResult = await _classForumResultRepository.Queryable
                .Include(x => x.LessonResult)
                .ThenInclude(x => x!.Lesson)
                .ThenInclude(x => x!.UnitLessons)
                .ThenInclude(x => x.Unit)
                .ThenInclude(x => x!.CourseUnitMockTests)
                .Include(x => x.ClassForum)
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .Where(x => x.Id == request.ClassForumResultId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            if (_authContext.Roles!.Contains(EnumRole.CSO.ToString()))
            {
                var csoResults = await _userService.GetCSOByUserId(_authContext.CurrentUserId);
                var csoId = csoResults.Content?.Result?.Id;
                if (classForumResult.CheckStartDate.HasValue && classForumResult.CheckStartDate.Value.AddMinutes(30) < DateTime.Now)
                {
                    classForumResult.CheckCsoId = null;
                    classForumResult.CheckStartDate = null;
                }
                else
                {
                    classForumResult.CheckCsoId = csoId;
                    classForumResult.CheckStartDate = DateTime.Now;
                }
            }

            if (_authContext.Roles!.Contains(EnumRole.Teacher.ToString()))
            {
                var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
                var teacherId = teacherResult.Content?.Result?.Id;
                if (classForumResult.GradingStartDate.HasValue && classForumResult.GradingStartDate.Value.AddMinutes(30) < DateTime.Now)
                {
                    classForumResult.GradingTeacherId = null;
                    classForumResult.GradingStartDate = null;
                }
                else
                {
                    classForumResult.GradingTeacherId = teacherId;
                    classForumResult.GradingStartDate = DateTime.Now;
                }
            }

            var lesson = classForumResult.LessonResult?.Lesson?.UnitLessons.FirstOrDefault(y => y.UnitId == classForumResult.LessonResult.UnitId)?.DisplayOrder;
            var unit = classForumResult.LessonResult?.Unit?.CourseUnitMockTests.FirstOrDefault(y => y.CourseId == classForumResult.LessonResult.CourseId)?.DisplayOrder;

            var classForumResultModel = new ClassForumResultModel
            {
                Id = classForumResult.Id,
                Content = classForumResult.Content,
                Status = classForumResult.Status,
                ClassForumId = classForumResult.ClassForumId,
                ClassForum = _mapper.Map<ClassForumModel>(classForumResult.ClassForum),
                LessonDisplayOrder = lesson ?? default,
                UnitDisplayOrder = unit ?? default,
                CreatedDate = classForumResult.CreatedDate,
                CheckStartDate = classForumResult.CheckStartDate,
                GradingStartDate = classForumResult.GradingStartDate,
                CheckCsoId = classForumResult.CheckCsoId,
                GradingTeacherId = classForumResult.GradingTeacherId ?? default,
                FilePaths = classForumResult.ClassForumResultFiles == null ? null : classForumResult.ClassForumResultFiles.Select(x => x.FilePath ?? string.Empty).ToList(),
                ClassForumScores = classForumResult.ClassForumScores == null ? null : classForumResult.ClassForumScores.Select(x => new ClassForumScoreModel
                {
                    Id = x.Id,
                    Feedback = x.Feedback,
                    Criteria = x.Criteria,
                    Score = x.Score
                }).ToList(),
            };

            classForumResult = _classForumResultRepository.Update(classForumResult);
            await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = classForumResultModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
