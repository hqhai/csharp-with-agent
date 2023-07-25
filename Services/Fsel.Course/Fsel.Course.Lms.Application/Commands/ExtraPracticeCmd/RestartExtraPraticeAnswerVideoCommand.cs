// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RestartExtraPraticeAnswerVideoCommand : IRequest<MethodResult<ExtraPracticeResultModel>>
    {
        public Guid ExtraPracticeResultId { get; set; }
    }

    public class RestartExtraPraticeAnswerVideoCommandHandler : IRequestHandler<RestartExtraPraticeAnswerVideoCommand, MethodResult<ExtraPracticeResultModel>>
    {
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IExtraPracticeAnswerRepository _extraPracticeAnswerRepository;

        public RestartExtraPraticeAnswerVideoCommandHandler(IExtraPracticeResultRepository extraPracticeResultRepository
            , AuthContext authContext
            , IMapper mapper
            , IUserService userService
            , IExtraPracticeAnswerRepository extraPracticeAnswerRepository)
        {
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _authContext = authContext;
            _mapper = mapper;
            _userService = userService;
            _extraPracticeAnswerRepository = extraPracticeAnswerRepository;
        }

        public async Task<MethodResult<ExtraPracticeResultModel>> Handle(RestartExtraPraticeAnswerVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeResultModel> methodResult = new MethodResult<ExtraPracticeResultModel>();

            #region Validate

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.Include(x => x.ExtraPracticeAnswers).FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeResultId && x.StudentId == studentId, cancellationToken);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeResultNotExist));
                return methodResult;
            }

            #endregion Validate

            #region xoa cau tra loi

            if (extraPracticeResult.Status == EnumResultStatus.Done)
            {
                foreach (var item in extraPracticeResult.ExtraPracticeAnswers)
                {
                    await _extraPracticeAnswerRepository.DeleteAsync(item);
                    await _extraPracticeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            #endregion xoa cau tra loi

            await _extraPracticeResultRepository.ExecuteTransactionAsync(async () =>
            {
                extraPracticeResult.CurrentVideoTimeCodeId = null;
                _extraPracticeResultRepository.Update(extraPracticeResult);
                await _extraPracticeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<ExtraPracticeResultModel>(extraPracticeResult);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });
            return methodResult;
        }
    }
}
