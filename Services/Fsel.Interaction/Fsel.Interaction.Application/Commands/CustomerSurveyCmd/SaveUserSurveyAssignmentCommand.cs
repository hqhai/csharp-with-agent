// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CustomerSurveyCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Caching;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveUserSurveyAssignmentCommand : SaveUserSurveyAssignmentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SaveUserSurveyAssignmentCommandHandler : IRequestHandler<SaveUserSurveyAssignmentCommand, MethodResult<bool>>
    {
        private readonly IUserSurveyAssignmentRepository _userSurveyAssignmentRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ICacheService<UserSurveyAssignment> _cacheService;
        private readonly SendNotifyUserHasSurveyPublisher _sendNotifyUserHasSurveyPublisher;

        public SaveUserSurveyAssignmentCommandHandler(IUserSurveyAssignmentRepository userSurveyAssignmentRepository, AuthContext authContext, IMapper mapper, ICacheService<UserSurveyAssignment> cacheService, SendNotifyUserHasSurveyPublisher sendNotifyUserHasSurveyPublisher)
        {
            _userSurveyAssignmentRepository = userSurveyAssignmentRepository;
            _authContext = authContext;
            _mapper = mapper;
            _cacheService = cacheService;
            _sendNotifyUserHasSurveyPublisher = sendNotifyUserHasSurveyPublisher;
        }

        public async Task<MethodResult<bool>> Handle(SaveUserSurveyAssignmentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            UserSurveyAssignment? userSurveyAssignment = null;

            if (!request.IsSurveyQuestBoard)
            {
                if (await _userSurveyAssignmentRepository.Queryable.AnyAsync(p => p.CreatedUserId == _authContext.CurrentUserId && p.CourseLevel == request.CourseLevel && p.CourseType == request.CourseType && p.ProgressRequirement == request.ProgressRequirement, cancellationToken))
                {
                    return methodResult;
                }

                userSurveyAssignment = _mapper.Map<UserSurveyAssignment>(request);
                userSurveyAssignment.IsSurveyQuestBoard = false;
            }
            else
            {
                if (await _userSurveyAssignmentRepository.Queryable.AnyAsync(p => p.CreatedUserId == _authContext.CurrentUserId && p.IsSurveyQuestBoard == request.IsSurveyQuestBoard, cancellationToken))
                {
                    return methodResult;
                }

                userSurveyAssignment = new UserSurveyAssignment() { IsSurveyQuestBoard = true };
            }

            if (userSurveyAssignment != null)
            {
                if (!userSurveyAssignment.IsValid())
                {
                    return methodResult;
                }

                var key = $"SaveUserSurveyAssignment_{_authContext.CurrentUserId}";

                var studentDoSurveyCache = await _cacheService.GetAsync(key);
                if (studentDoSurveyCache != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                    return methodResult;
                }
                await _cacheService.SetAsync(key, userSurveyAssignment, TimeSpan.FromSeconds(5));

                await _userSurveyAssignmentRepository.ExecuteTransactionAsync(async () =>
                {
                    userSurveyAssignment = _userSurveyAssignmentRepository.Add(userSurveyAssignment);
                    await _userSurveyAssignmentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status201Created;

                    if (!request.IsSurveyQuestBoard && request.CourseType.HasValue && request.CourseLevel.HasValue && request.ProgressRequirement.HasValue)
                    {
                        await _sendNotifyUserHasSurveyPublisher.Publish(new SaveUserSurveyAssignmentCommandModel()
                        {
                            CourseLevel = request.CourseLevel.Value,
                            CourseType = request.CourseType.Value,
                            ProgressRequirement = request.ProgressRequirement.Value
                        }, cancellationToken);
                    }

                    methodResult.Result = true;
                    return methodResult;
                });
            }

            return methodResult;
        }
    }
}
