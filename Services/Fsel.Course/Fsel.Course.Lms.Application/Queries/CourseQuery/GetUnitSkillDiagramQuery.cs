// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitSkillDiagramQuery : IRequest<MethodResult<IList<SkillScores>>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetUnitSkillDiagramQueryHandler : IRequestHandler<GetUnitSkillDiagramQuery, MethodResult<IList<SkillScores>>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetUnitSkillDiagramQueryHandler(ICourseResultRepository courseResultRepository, AuthContext authContext, IUserService userService)
        {
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<IList<SkillScores>>> Handle(GetUnitSkillDiagramQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SkillScores>> methodResult = new MethodResult<IList<SkillScores>>();

            #region Validate

            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }

            #endregion Validate

            #region Handler

            var studentId = studentsResult.Content!.Result!.Id;

            var courseResult = await _courseResultRepository.Queryable
                          .Where(p => p.CourseId == request.CourseId && p.StudentId == studentId)
                          .Select(x => new
                          {
                              CourseResult = x
                          }).FirstOrDefaultAsync(cancellationToken);

            if (courseResult == null)
            {
                return methodResult;
            }

            methodResult.Result = courseResult.CourseResult.SkillScores;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;

            #endregion Handler
        }
    }
}
