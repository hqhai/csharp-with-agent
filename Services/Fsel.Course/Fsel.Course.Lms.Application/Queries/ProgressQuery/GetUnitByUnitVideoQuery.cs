// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByUnitVideoQuery : IRequest<MethodResult<IList<UnitModel>>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetUnitByUnitVideoQueryHandler : IRequestHandler<GetUnitByUnitVideoQuery, MethodResult<IList<UnitModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;

        public GetUnitByUnitVideoQueryHandler(AuthContext authContext
            , IMapper mapper
            , IUnitRepository unitRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _mapper = mapper;
            _unitRepository = unitRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<UnitModel>>> Handle(GetUnitByUnitVideoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UnitModel>> methodResult = new MethodResult<IList<UnitModel>>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

            var units = await _unitRepository.Queryable.Include(x => x.UnitLessons).ThenInclude(x => x.Lesson)
                                .Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId))
                                .Include(x => x.CourseUnitMockTests)
                                .Include(x => x.LessonResults.Where(x => x.StudentId == studentId))
                                .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                                .AsNoTracking()
                                .ToListAsync(cancellationToken);
            if (units == null || units.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(units));
                return methodResult;
            }
            var unitModels = units.Select(x =>
            {
                var countDone = x.LessonResults.Where(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done).Count();
                var unitModel = new UnitModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    CourseLevel = x.CourseLevel,
                    CreatedDate = x.CreatedDate,
                    CreatedFullName = x.CreatedFullName,
                    CreatedUserId = x.CreatedUserId,
                    IsActive = true,
                    Name = x.Name,
                    DisplayOrder = x.CourseUnitMockTests.FirstOrDefault(y => y.CourseId == request.CourseId && y.UnitId == x.Id)?.DisplayOrder ?? default,
                    UpdatedDate = x.UpdatedDate,
                    UpdatedFullName = x.UpdatedFullName,
                    UpdatedUserId = x.UpdatedUserId,
                    UnitResult = _mapper.Map<UnitResultModel>(x.UnitResults.FirstOrDefault()),
                    Percent = (double)countDone / x.UnitLessons.Select(x => x.Lesson).Count() * 100,
                };
                return unitModel;
            }).OrderBy(x => x.DisplayOrder).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = unitModels;
            return methodResult;
        }
    }
}
