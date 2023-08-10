// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgessQuery
{
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

    public class GetUnitByUnitQuery : IRequest<MethodResult<IList<UnitModel>>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetUnitByUnitQueryHandler : IRequestHandler<GetUnitByUnitQuery, MethodResult<IList<UnitModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;

        public GetUnitByUnitQueryHandler(AuthContext authContext
            , IMapper mapper
            , IUnitRepository unitRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _mapper = mapper;
            _unitRepository = unitRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<UnitModel>>> Handle(GetUnitByUnitQuery request, CancellationToken cancellationToken)
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

            var units = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId))
                                .Include(x => x.CourseUnitMockTests)
                                .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                                .ToListAsync(cancellationToken);
            if (units == null || units.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(units));
                return methodResult;
            }
            var unitModels = units.Select(x => new UnitModel
            {
                Id = x.Id,
                Code = x.Code,
                CourseLevel = x.CourseLevel,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                IsActive = true,
                Name = x.Name,
                UpdatedDate = x.UpdatedDate,
                UpdatedFullName = x.UpdatedFullName,
                UpdatedUserId = x.UpdatedUserId,
                UnitResult = _mapper.Map<UnitResultModel>(x.UnitResults.FirstOrDefault())
            }).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = unitModels;
            return methodResult;
        }
    }
}
