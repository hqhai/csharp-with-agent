// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitQuery : IRequest<MethodResult<UnitModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetUnitQueryHandler : IRequestHandler<GetUnitQuery, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetUnitQueryHandler(IUnitRepository unitRepository, IUserService userService, AuthContext authContext, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _userService = userService;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(GetUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UnitModel>();
            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult?.Content?.Result?.Id;
            var unitModel = await _unitRepository.Queryable
                            .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId && x.UnitId == request.UnitId))
                            .Select(x => new UnitModel
                            {
                                Id = x.Id,
                                Code = x.Code,
                                Name = x.Name,
                                CourseLevel = x.CourseLevel,
                                DisplayOrder = x.CourseUnitMockTests.Max(x => x.DisplayOrder),
                                CreatedDate = x.CreatedDate,
                                UnitResult = _mapper.Map<UnitResultModel>(x.UnitResults.AsQueryable().Include(x => x.Unit).ThenInclude(x => x!.LessonResults.Where(x => x.CourseId == request.CourseId && x.StudentId == studentId))
                                                                 .Include(x => x.Unit).ThenInclude(x => x!.UnitLessons)
                                                                 .Where(y => y.StudentId == studentId && y.CourseId == request.CourseId)
                                                                 .AsNoTracking().FirstOrDefault()),
                            })
                            .FirstOrDefaultAsync(cancellationToken);
            if (unitModel == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitModel));
                return methodResult;
            }
            methodResult.Result = unitModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
