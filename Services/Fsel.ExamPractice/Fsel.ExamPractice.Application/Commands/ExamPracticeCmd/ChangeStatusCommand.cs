// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Commands.ExamPracticeCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Infrastructure.Common;
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

        public ChangeStatusCommandHandler(
            IExamPracticeRepository examPracticeRepository,
            IMapper mapper,
            ExamPracticeHelper examPracticeHelper)
        {
            _examPracticeRepository = examPracticeRepository;
            _mapper = mapper;
            _examPracticeHelper = examPracticeHelper;
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
            if (examPractice.Status == EnumExamPracticeStatus.Cloned)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.LockedClonedStatus), nameof(examPractice.Status), examPractice.Status);
                return methodResult;
            }

            if (examPractice.Type == EnumExamPracticeType.ExamPractice)
            {
                if (examPractice.Status != EnumExamPracticeStatus.Active && request.IsActive)
                {
                    if (!await _examPracticeHelper.IsValidateActiveStatus(examPractice))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.MissingRequiredData));
                        return methodResult;
                    }

                    examPractice.Status = EnumExamPracticeStatus.Active;
                    if (!examPractice.ActivatedAt.HasValue)
                    {
                        examPractice.ActivatedAt = DateTime.UtcNow;
                    }
                }
                else if (examPractice.Status == EnumExamPracticeStatus.Active)
                {
                    examPractice.Status = request.IsActive ? EnumExamPracticeStatus.Active : EnumExamPracticeStatus.Inactive;
                }
            }
            else
            {
                if (examPractice.Status != EnumExamPracticeStatus.Active && request.IsActive)
                {
                    examPractice.Status = EnumExamPracticeStatus.Active;
                    if (!examPractice.ActivatedAt.HasValue)
                    {
                        examPractice.ActivatedAt = DateTime.UtcNow;
                    }
                }
                else if (examPractice.Status == EnumExamPracticeStatus.Active)
                {
                    examPractice.Status = request.IsActive ? EnumExamPracticeStatus.Active : EnumExamPracticeStatus.Inactive;
                }
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
