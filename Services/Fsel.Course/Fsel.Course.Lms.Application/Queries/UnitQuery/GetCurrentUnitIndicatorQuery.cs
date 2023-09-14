// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentUnitIndicatorQuery : IRequest<MethodResult<IList<SkillScores>>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetCurrentUnitIndicatorQueryHandler : IRequestHandler<GetCurrentUnitIndicatorQuery, MethodResult<IList<SkillScores>>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IUnitResultRepository _unitResultRepository;

        public GetCurrentUnitIndicatorQueryHandler(IUserService userService
            , AuthContext authContext
            , IUnitResultRepository unitResultRepository)
        {
            _userService = userService;
            _authContext = authContext;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<IList<SkillScores>>> Handle(GetCurrentUnitIndicatorQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<SkillScores>> methodResult = new MethodResult<IList<SkillScores>>();
            #region Validate
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            #endregion

            #region Handler
            var studentId = studentsResult.Content!.Result!.Id;

            var currentUnitIndicator = await _unitResultRepository
                              .Queryable
                              .Where(x => x.CourseId == request.CourseId && x.Status == EnumResultStatus.Done)
                              .Select(x => new
                              {
                                  UnitResult = x,
                                  MaxDisplayOrder = x.Course!.CourseUnitMockTests
                                                    .Where(cumt => cumt.UnitId == x.UnitId && cumt.CourseId == x.CourseId)
                                                    .Max(cumt => cumt.DisplayOrder)
                              })
                              .OrderByDescending(x => x.MaxDisplayOrder)
                              .FirstOrDefaultAsync(cancellationToken);

            if (currentUnitIndicator == null)
            {
                return methodResult;
            }
            #endregion

            methodResult.Result = currentUnitIndicator!.UnitResult.SkillScores;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
