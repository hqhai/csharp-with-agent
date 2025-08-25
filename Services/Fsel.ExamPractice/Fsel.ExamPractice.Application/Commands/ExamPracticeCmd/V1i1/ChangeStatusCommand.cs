// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Commands.ExamPracticeCmd.V1i1
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common;
    using Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers;
    using MediatR;

    public class ChangeStatusCommand : IRequest<MethodResult<ExamPracticeModel>>
    {
        public Guid Id { get; set; }

        public bool IsActive { get; set; }
    }

    public class ChangeStatusCommandHandler : IRequestHandler<ChangeStatusCommand, MethodResult<ExamPracticeModel>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IMapper _mapper;
        private readonly ExamPracticeHelper _examPracticeHelper;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;

        public ChangeStatusCommandHandler(IExamPracticeRepository examPracticeRepository,
                                          IMapper mapper,
                                          ExamPracticeHelper examPracticeHelper,
                                          IExamPracticeSectionRepository examPracticeSectionRepository)
        {
            _examPracticeRepository = examPracticeRepository;
            _mapper = mapper;
            _examPracticeHelper = examPracticeHelper;
            _examPracticeSectionRepository = examPracticeSectionRepository;
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(ChangeStatusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeModel>();

            var examPractice = await _examPracticeRepository.GetByIdAsync(request.Id);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPractice), request.Id);
                return methodResult;
            }

            if (request.IsActive)
            {
                var validate = await ExamPracticeValidateBuilder.Create(new UpdateExamPracticeCommandModel(), _examPracticeRepository).ValidateChangeStatus(examPractice.Type, _examPracticeSectionRepository, examPractice.Id);
                var validateResult = validate.GetResult();
                if (!validateResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(validateResult.ErrorMessages);
                    return methodResult;
                }
            }

            if (request.IsActive && !examPractice.ActivatedAt.HasValue)
            {
                examPractice.Status = EnumExamPracticeStatus.Active;
                examPractice.ActivatedAt = DateTime.UtcNow;
            }
            else
            {
                examPractice.Status = request.IsActive ? EnumExamPracticeStatus.Active : EnumExamPracticeStatus.Inactive;
            }

            await _examPracticeRepository.ExecuteTransactionAsync(async () =>
            {
                _examPracticeRepository.Update(examPractice);
                await _examPracticeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<ExamPracticeModel>(examPractice);
                return methodResult;
            });

            await _examPracticeHelper.UpdateExamPracticeSectionScoreAsync(examPractice).ConfigureAwait(false);
            return methodResult;
        }
    }
}
