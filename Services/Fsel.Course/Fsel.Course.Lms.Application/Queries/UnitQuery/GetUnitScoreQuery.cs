// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.LessonQuery;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;

    public class GetUnitScoreQuery : IRequest<MethodResult<UnitScoreModel>>
    {
        public Guid CourseId { get; set; }

        public Guid UnitId { get; set; }
    }
    public class GetUnitScoreQueryHandler : IRequestHandler<GetUnitScoreQuery, MethodResult<UnitScoreModel>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IUnitResultRepository _unitResultRepository;

        public GetUnitScoreQueryHandler(IUserService userService, AuthContext authContext, IUnitResultRepository unitResultRepository)
        {
            _userService = userService;
            _authContext = authContext;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<UnitScoreModel>> Handle(GetUnitScoreQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<UnitScoreModel>();
            var unitScore = new UnitScoreModel();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
        }
    }
}
