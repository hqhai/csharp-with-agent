// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitScoreQuery : IRequest<MethodResult<IList<UnitResultModel>>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetUnitScoreQueryHandler : IRequestHandler<GetUnitScoreQuery, MethodResult<IList<UnitResultModel>>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IUnitResultRepository _unitResultRepository;

        public GetUnitScoreQueryHandler(IUserService userService
            , AuthContext authContext
            , IUnitResultRepository unitResultRepository)
        {
            _userService = userService;
            _authContext = authContext;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<IList<UnitResultModel>>> Handle(GetUnitScoreQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<UnitResultModel>> methodResult = new MethodResult<IList<UnitResultModel>>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var unitQuery = await _unitResultRepository
                                .Queryable.Where(x => x.CourseId == request.CourseId)
                                .Select(x => new UnitResultModel
                                {
                                    Id = x.Id,
                                    Percent = x.Percent,
                                    Status = x.Status,
                                    SkillScores = x.SkillScores,
                                    CorrectCount = x.CorrectCount,
                                    CorrectTotal = x.CorrectTotal,
                                    StudentId = studentId,
                                    UnitId = x.UnitId,
                                    CourseId = request.CourseId,
                                    CreatedDate = x.CreatedDate,
                                }).ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = unitQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
