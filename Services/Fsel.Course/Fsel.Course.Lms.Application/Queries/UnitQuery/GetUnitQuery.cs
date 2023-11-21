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
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult?.Content?.Result?.Id;
            var unit = await _unitRepository.Queryable
                            .Include(x => x.CourseUnitMockTests)
                            .Include(x => x.UnitResults.Where(x => x.CourseId == request.CourseId && x.StudentId == studentId))
                            .Where(x => x.CourseUnitMockTests.Select(x => x.CourseId).Contains(request.CourseId))
                            .FirstOrDefaultAsync(cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }
            var unitDto = _mapper.Map<UnitModel>(unit);
            unitDto.UnitResult = _mapper.Map<UnitResultModel>(unit.UnitResults.FirstOrDefault());
            methodResult.Result = unitDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
