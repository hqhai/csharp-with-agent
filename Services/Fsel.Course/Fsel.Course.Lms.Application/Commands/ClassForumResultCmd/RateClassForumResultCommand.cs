// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class RateClassForumResultCommand : RateClassForumResultCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class RateClassForumResultCommandHandler : IRequestHandler<RateClassForumResultCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public RateClassForumResultCommandHandler(IMapper mapper, AuthContext authContext, IUserService userService, IClassForumResultRepository classForumResultRepository)
        {
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(RateClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (!EnumFeedBackHelper.IsCheckFeedBack(request.FeedBackNegatives, request.FeedBackPositives))
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.FeedbackPositiveOrFeedBackBothHaveValue));
                return methodResult;
            }
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var classForumResult = await _classForumResultRepository.GetByIdAsync(request.ClassForumResultId);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            if (studentId != classForumResult.StudentId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ResultNotFromStudent));
                return methodResult;
            }
            if (classForumResult.FeedBackStars > 5)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.FeedBackStarOnlyCanHane5));
                return methodResult;
            }
            #region temporary delete
            /* if (classForumResult.Status != EnumClassForumResultStatus.Graded)
             {
                 methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotGraded));
                 return methodResult;
             }*/
            #endregion
            _mapper.Map(request, classForumResult);
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                classForumResult = _classForumResultRepository.Update(classForumResult);
                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
