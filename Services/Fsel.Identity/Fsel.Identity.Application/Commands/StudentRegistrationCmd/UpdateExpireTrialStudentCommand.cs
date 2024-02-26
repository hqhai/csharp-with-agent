// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRegistrationCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateExpireTrialStudentCommand : IRequest<MethodResult<bool>>
    {
    }

    public class UpdateExpireTrialStudentCommandHandler : IRequestHandler<UpdateExpireTrialStudentCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentTrialRegistrationRepository _studentTrialRegistrationRepository;
        private const int CompareDate = -14;

        public UpdateExpireTrialStudentCommandHandler(IMapper mapper, IStudentTrialRegistrationRepository studentTrialRegistrationRepository)
        {
            _mapper = mapper;
            _studentTrialRegistrationRepository = studentTrialRegistrationRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateExpireTrialStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            DateTime compareWithCreateDate = DateTime.UtcNow.AddDays(CompareDate);
            var studentTrialRegistrationResults = _studentTrialRegistrationRepository.Queryable.Where(x => x.Status == EnumTrialRegistrationStatus.Trial && x.CreatedDate.Date <= compareWithCreateDate.Date && x.CreatedDate.Month <= compareWithCreateDate.Month && x.CreatedDate.Year <= compareWithCreateDate.Year);

            if (studentTrialRegistrationResults == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            foreach (var item in studentTrialRegistrationResults)
            {
                item.Status = EnumTrialRegistrationStatus.Expired;
            }

            await _studentTrialRegistrationRepository.ExecuteTransactionAsync(async () =>
            {
                _studentTrialRegistrationRepository.UpdateList(studentTrialRegistrationResults);
                await _studentTrialRegistrationRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
