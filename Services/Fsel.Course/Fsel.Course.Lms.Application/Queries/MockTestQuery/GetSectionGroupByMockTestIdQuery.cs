// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSectionGroupByMockTestIdQuery : IRequest<MethodResult<IList<SectionGroupModel>>>
    {
        public Guid MockTestId { get; set; }
    }

    public class GetSectionGroupByMockTestIdQueryHandler : IRequestHandler<GetSectionGroupByMockTestIdQuery, MethodResult<IList<SectionGroupModel>>>
    {
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetSectionGroupByMockTestIdQueryHandler(ISectionGroupRepository sectionGroupRepository, AuthContext authContext, IUserService userService)
        {
            _sectionGroupRepository = sectionGroupRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<IList<SectionGroupModel>>> Handle(GetSectionGroupByMockTestIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SectionGroupModel>>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id ?? default;
            var sectionGroups = await _sectionGroupRepository.Queryable.Include(x => x.MockTestSections).Include(x => x.SectionGroupResults.Where(x => x.StudentId == studentId)).Where(x => x.MockTestSections.Any(x => x.MockTestId == request.MockTestId)).ToListAsync(cancellationToken);
            var
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
