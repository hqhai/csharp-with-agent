// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SurveyConfigCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ChangeStatusViewSurveyCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ChangeStatusViewSurveyCommandHandler : IRequestHandler<ChangeStatusViewSurveyCommand, MethodResult<bool>>
    {
        private readonly IUserSurveyAssignmentRepository _userSurveyAssignmentRepository;

        public ChangeStatusViewSurveyCommandHandler(IUserSurveyAssignmentRepository userSurveyAssignmentRepository)
        {
            _userSurveyAssignmentRepository = userSurveyAssignmentRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusViewSurveyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var userSurveyAssignment = await _userSurveyAssignmentRepository.GetByIdAsync(request.Id);
            if (userSurveyAssignment == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userSurveyAssignment));
                return methodResult;
            }
            await _userSurveyAssignmentRepository.ExecuteTransactionAsync(async () =>
            {
                userSurveyAssignment.IsView = true;
                userSurveyAssignment = _userSurveyAssignmentRepository.Update(userSurveyAssignment);
                await _userSurveyAssignmentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
