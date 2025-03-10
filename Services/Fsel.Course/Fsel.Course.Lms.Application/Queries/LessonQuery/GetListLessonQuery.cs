// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using System;
    using System.Collections.Generic;
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
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetListLessonQueryHandler(ILessonRepository lessonRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonModel>>> Handle(GetListLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LessonModel>> methodResult = new MethodResult<IList<LessonModel>>();
            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var lessonQuery = await _lessonRepository.Queryable
                                .Include(x => x.UnitLessons)
                                .Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId && x.UnitId == request.UnitId))
                                .Where(x => x.UnitLessons.Select(x => x.UnitId).Contains(request.UnitId))
                                .AsNoTracking()
                                .Select(x => new LessonModel
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    InstructionContent = x.InstructionContent,
                                    IsActive = x.UnitLessons.Any(),
                                    DisplayOrder = x.UnitLessons.Where(n => n.UnitId == request.UnitId).Select(x => x.DisplayOrder).FirstOrDefault(),
                                    LessonResult = _mapper.Map<LessonResultModel>(x.LessonResults.FirstOrDefault(x => x.StudentId == studentId && x.CourseId == request.CourseId && x.UnitId == request.UnitId))
                                }).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken: cancellationToken);

            if (lessonQuery.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonQuery));
                return methodResult;
            }
            methodResult.Result = lessonQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
