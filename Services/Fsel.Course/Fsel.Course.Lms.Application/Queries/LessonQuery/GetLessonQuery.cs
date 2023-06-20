// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonQuery : IRequest<MethodResult<LessonsMockTestModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid? LessonId { get; set; }
    }

    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, MethodResult<LessonsMockTestModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetLessonQueryHandler(ILessonRepository lessonRepository,
            AuthContext authContext,
            IMapper mapper,
            IUserService userService,
            IUnitRepository unitRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<LessonsMockTestModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonsMockTestModel> methodResult = new MethodResult<LessonsMockTestModel>();
            methodResult.Result = new LessonsMockTestModel();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var lessons = await _lessonRepository.Queryable
                                .Include(x => x.LessonInstructions.Where(x => !x.IsDeleted))
                                .Include(x => x.LessonResults.Where(y => !y.IsDeleted))
                                .Include(x => x.UnitLessons.Where(x => !x.IsDeleted))
                                .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                                .ThenInclude(x => x.Video)
                                .Where(x => x.UnitLessons.Select(x => x.UnitId).Contains(request.UnitId))
                                .Where(x => !request.LessonId.HasValue || x.Id == request.LessonId)
                                .AsNoTracking()
                                .Select(x => new LessonModel
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    CourseLevel = x.CourseLevel,
                                    InstructionContent = x.InstructionContent,
                                    IsActive = x.UnitLessons.Any(),
                                    VideoId = x.LessonVideos.Where(x => x.Video != null).Select(x => x.Video).FirstOrDefault()!.Id,
                                    DisplayOrder = x.UnitLessons.Where(n => n.UnitId == request.UnitId).Select(x => x.DisplayOrder).FirstOrDefault(),
                                    LessonInstructions = _mapper.Map<IList<LessonInstructionModel>>(x.LessonInstructions),
                                    LessonResult = _mapper.Map<LessonResultModel>(x.LessonResults.FirstOrDefault(y => y.UnitId == request.UnitId && y.CourseId == request.CourseId && y.StudentId == studentId)),
                                }).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken: cancellationToken);

            if (lessons.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotExist), nameof(request.LessonId), request.LessonId);
                return methodResult;
            }

            var mocktest = await _unitRepository.Queryable
                                .Include(x => x.UnitSkillMockTests)
                                .ThenInclude(x => x.MockTest)
                                .ThenInclude(x => x!.MockTestSections)
                                .ThenInclude(x => x.SectionGroup)
                                .Where(x => x.Id == request.UnitId)
                                .SelectMany(x => x.UnitSkillMockTests)
                                .Select(x => x.MockTest)
                                .Select(x => new MockTestModel
                                {
                                    CourseType = x!.CourseType,
                                    Id = x.Id,
                                    Name = x.Name,
                                    TotalQuestion = x.MockTestSections.Select(x => x.SectionGroup).SelectMany(x => x!.Sections).SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Select(x => x!.CorrectTotal).Sum(),
                                    SectionGroups = x.MockTestSections.Select(x => x.SectionGroup).Select(x => new SectionGroupModel
                                    {
                                        Id = x!.Id,
                                        CourseSkill = x.CourseSkill,
                                        ExecutionTime = x!.ExecutionTime,
                                    }).ToList(),
                                })
                                .FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = new LessonsMockTestModel
            {
                Lessons = lessons,
                MockTest = mocktest
            };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
