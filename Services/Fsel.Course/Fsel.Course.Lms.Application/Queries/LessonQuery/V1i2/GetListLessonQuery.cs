// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums; // EnumVersionStatus
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLessonQuery : IRequest<MethodResult<IList<LessonModel>>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetListLessonQueryHandler : IRequestHandler<GetListLessonQuery, MethodResult<IList<LessonModel>>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitModuleRepository _unitModuleRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetListLessonQueryHandler(
            ILessonRepository lessonRepository,
            IUnitModuleRepository unitModuleRepository,
            ILessonResultRepository lessonResultRepository,
            AuthContext authContext,
            IUserService userService,
            IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _unitModuleRepository = unitModuleRepository;
            _lessonResultRepository = lessonResultRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonModel>>> Handle(GetListLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<LessonModel>>();

            // 1) Student
            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode || student.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "Student");
                return methodResult;
            }

            var studentId = student.Content.Result.Id;

            // 2) UnitModules -> lấy group OriginalId + DisplayOrder (đúng thứ tự)
            var unitModules = await _unitModuleRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => x.UnitId == request.UnitId)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new
                {
                    x.OriginalId,
                    x.DisplayOrder
                })
                .ToListAsync(cancellationToken);

            if (unitModules.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "UnitModule");
                return methodResult;
            }

            var lessonOriginalIds = unitModules
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            // 3) Lessons: theo OriginalId + LastVersion
            var lessons = await _lessonRepository.ReadQueryable
                .AsNoTracking()
                .Where(l => lessonOriginalIds.Contains(l.OriginalId)
                            && l.VersionStatus == EnumVersionStatus.LastVersion)
                .Select(l => new
                {
                    l.Id,
                    l.OriginalId,
                    l.Name,
                    l.InstructionContent
                })
                .ToListAsync(cancellationToken);

            if (lessons.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "Lesson");
                return methodResult;
            }

            // Map lesson theo OriginalId để join lại theo UnitModule order
            var lessonByOriginalId = lessons
                .GroupBy(x => x.OriginalId)
                .ToDictionary(g => g.Key, g => g.First()); // LastVersion nên kỳ vọng 1 record/group

            var lessonIds = lessons.Select(x => x.Id).ToList();

            var lessonResultDict = await _lessonResultRepository.ReadQueryable
                .AsNoTracking()
                .Where(r => r.StudentId == studentId
                            && r.CourseId == request.CourseId
                            && r.UnitId == request.UnitId
                            && lessonIds.Contains(r.LessonId))
                .ToDictionaryAsync(x => x.LessonId, cancellationToken);

            var result = new List<LessonModel>(unitModules.Count);

            foreach (var um in unitModules)
            {
                if (!lessonByOriginalId.TryGetValue(um.OriginalId, out var lesson))
                {
                    continue;
                }

                lessonResultDict.TryGetValue(lesson.Id, out var lr);

                result.Add(new LessonModel
                {
                    Id = lesson.Id,
                    Name = lesson.Name,
                    InstructionContent = lesson.InstructionContent,
                    IsActive = true,
                    DisplayOrder = um.DisplayOrder,
                    LessonResult = lr != null ? _mapper.Map<LessonResultModel>(lr) : null
                });
            }

            if (result.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "Lesson");
                return methodResult;
            }

            // đảm bảo order
            methodResult.Result = result.OrderBy(x => x.DisplayOrder).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
