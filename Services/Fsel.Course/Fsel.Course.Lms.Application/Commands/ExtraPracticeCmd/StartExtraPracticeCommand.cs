// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartExtraPracticeCommand : IRequest<MethodResult<ExtraPracticeModel>>
    {
        public Guid ExtraPracticeId { get; set; }
    }

    public class StartExtraPracticeCommandHandler : IRequestHandler<StartExtraPracticeCommand, MethodResult<ExtraPracticeModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ExtraPracticeConverter _extraPracticeConverter;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public StartExtraPracticeCommandHandler(IExtraPracticeRepository extraPracticeRepository
            , AuthContext authContext
            , IUserService userService
            , ExtraPracticeConverter extraPracticeConverter
            , IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _authContext = authContext;
            _userService = userService;
            _extraPracticeConverter = extraPracticeConverter;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(StartExtraPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExtraPracticeModel>();

            var extraPractice = await _extraPracticeRepository.GetByIdAsync(request.ExtraPracticeId);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeNotExist));
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == request.ExtraPracticeId, cancellationToken);
            if (extraPracticeResult == null)
            {
                extraPractice.ExtraPracticeResults.Add(new ExtraPracticeResult
                {
                    StudentId = studentId ?? default
                });
                _extraPracticeRepository.Update(extraPractice);
                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await _extraPracticeConverter.SwitchExtraPractice(extraPractice, studentId ?? default);
            return methodResult;
        }
    }
}
