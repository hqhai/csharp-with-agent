// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateSectionGroupResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid SectionGroupId { get; set; }
        public Guid MockTestResultId { get; set; }
    }

    public class CreateSectionGroupResultCommandHandler : IRequestHandler<CreateSectionGroupResultCommand, MethodResult<bool>>
    {
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public CreateSectionGroupResultCommandHandler(ISectionGroupResultRepository sectionGroupResultRepository, IUserService userService, AuthContext authContext)
        {
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CreateSectionGroupResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id ?? default;
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.SectionGroupId == request.SectionGroupId && x.MockTestResultId == request.MockTestResultId && x.StudentId == studentId)
                                                                                  .FirstOrDefaultAsync(cancellationToken);
            if (sectionGroupResult == null)
            {
                sectionGroupResult = new SectionGroupResult
                {
                    MockTestResultId = request.MockTestResultId,
                    SectionGroupId = request.SectionGroupId,
                    StudentId = studentId
                };
                _sectionGroupResultRepository.Add(sectionGroupResult);
                await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
