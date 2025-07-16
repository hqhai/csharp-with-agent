// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoResultCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateShowTokenVideoResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class UpdateShowTokenVideoResultCommandHandler : IRequestHandler<UpdateShowTokenVideoResultCommand, MethodResult<bool>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public UpdateShowTokenVideoResultCommandHandler(IVideoResultRepository videoResultRepository, AuthContext authContext, IUserService userService)
        {
            _videoResultRepository = videoResultRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(UpdateShowTokenVideoResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId && x.StudentId == student.Id, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            if (videoResult.IsShowToken)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.CoinDisplayed), nameof(videoResult.IsShowToken), videoResult.IsShowToken);
                return methodResult;
            }
            videoResult.IsShowToken = true;
            await _videoResultRepository.ExecuteTransactionAsync(async () =>
            {
                await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.VideoId, c.StudentId, c.LessonResultId };
                });

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
