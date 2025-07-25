// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CustomerSurveyCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
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

        public SaveUserSurveyAssignmentCommandHandler(IUserSurveyAssignmentRepository userSurveyAssignmentRepository, AuthContext authContext, IMapper mapper)
        {
            _userSurveyAssignmentRepository = userSurveyAssignmentRepository;
            _authContext = authContext;
            _mapper = mapper;
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

            if (!userSurveyAssignment.IsValid())
            {
                return methodResult;
            }

            if (userSurveyAssignment != null)
            {
                var dailyQuizWinners = new List<UserSurveyAssignment>() { userSurveyAssignment };

                await _userSurveyAssignmentRepository.ExecuteTransactionAsync(async () =>
                {
                    await _userSurveyAssignmentRepository.BulkMergeAsync(dailyQuizWinners, x =>
                    {
                        x.ColumnPrimaryKeyExpression = c => new { c.CreatedUserId, c.CourseLevel, c.CourseType, c.ProgressRequirement, c.IsSurveyQuestBoard };
                    });
                    await _userSurveyAssignmentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status201Created;
                    methodResult.Result = true;
                    return methodResult;
                });
            }

            return methodResult;
        }
    }
}
