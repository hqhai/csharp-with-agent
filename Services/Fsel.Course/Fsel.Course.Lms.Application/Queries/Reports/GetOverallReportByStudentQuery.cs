// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;

    public class GetOverallReportByStudentQuery : IRequest<MethodResult<OverallReportModel>>
    {
        public Guid CourseId { get; set; }
        public Guid? StudentId { get; set; }
    }

    public class GetOverallReportByStudentQueryHandler : IRequestHandler<GetOverallReportByStudentQuery, MethodResult<OverallReportModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly IInteractionService _interactionService;
        private readonly IUserService _userService;

        public GetOverallReportByStudentQueryHandler(ICourseRepository courseRepository, ICourseResultRepository courseResultRepository, AuthContext authContext, ISystemService systemService, IInteractionService interactionService, IUserService userService)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _systemService = systemService;
            _interactionService = interactionService;
            _userService = userService;
        }

        public async Task<MethodResult<OverallReportModel>> Handle(GetOverallReportByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallReportModel>();

            Guid? studentId;
            if (_authContext.CurrentUserId != default)
            {
                var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResult.Error);
                    return methodResult;
                }
                studentId = studentResult.Content?.Result?.Id;
            }
            else
            {
                studentId = request.StudentId;
            }
        }
    }
}
