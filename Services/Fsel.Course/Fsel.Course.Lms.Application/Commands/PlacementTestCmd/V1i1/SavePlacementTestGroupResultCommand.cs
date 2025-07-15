// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SavePlacementTestGroupResultCommand : IRequest<MethodResult<bool>>
    {
        public EnumCourseLevel ChooseCourseLevel { get; set; }
    }

    public class SavePlacementTestGroupResultCommandHandler : IRequestHandler<SavePlacementTestGroupResultCommand, MethodResult<bool>>
    {
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SavePlacementTestGroupResultCommandHandler(IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            AuthContext authContext,
            IUserService userService)
        {
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(SavePlacementTestGroupResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }
            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id, cancellationToken);
            if (placementTestGroupResult == null)
            {
                return methodResult;
            }
            placementTestGroupResult.ChooseLevel = request.ChooseCourseLevel;
            await _placementTestGroupResultRepository.BulkUpdateList(new List<PlacementTestGroupResult> { placementTestGroupResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.StudentId };
            });
            methodResult.Result = true;
            return methodResult;
        }
    }
}
