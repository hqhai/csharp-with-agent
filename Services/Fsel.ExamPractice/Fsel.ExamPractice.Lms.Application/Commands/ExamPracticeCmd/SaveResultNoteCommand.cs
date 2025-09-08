// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Commands.ExamPracticeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using MediatR;

    public class SaveResultNoteCommand : IRequest<MethodResult<ExamPracticeSectionResultModel>>
    {
        public string? Note { get; set; }
        public Guid ExamPracticeSectionResultId { get; set; }
    }

    public class SaveResultNoteCommandHandler : IRequestHandler<SaveResultNoteCommand, MethodResult<ExamPracticeSectionResultModel>>
    {
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;
        private readonly IMapper _mapper;

        public SaveResultNoteCommandHandler(IExamPracticeSectionResultRepository examPracticeSectionResultRepository,
            IMapper mapper)
        {
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExamPracticeSectionResultModel>> Handle(SaveResultNoteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeSectionResultModel>();

            var examPracticeSectionResult = await _examPracticeSectionResultRepository.GetByIdAsync(request.ExamPracticeSectionResultId);
            if (examPracticeSectionResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSectionResult), request.ExamPracticeSectionResultId);
                return methodResult;
            }
            //examPracticeSectionResult.Note = request.Note;
            if (!examPracticeSectionResult.IsValid())
            {
                methodResult.AddErrorBadRequest(examPracticeSectionResult.ErrorMessages);
                return methodResult;
            }

            await _examPracticeSectionResultRepository.ExecuteTransactionAsync(async () =>
            {
                _examPracticeSectionResultRepository.Update(examPracticeSectionResult);
                await _examPracticeSectionResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<ExamPracticeSectionResultModel>(examPracticeSectionResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
