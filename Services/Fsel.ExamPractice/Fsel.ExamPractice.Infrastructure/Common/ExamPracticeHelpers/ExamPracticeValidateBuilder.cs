// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Microsoft.EntityFrameworkCore;

    public class ExamPracticeValidateBuilder
    {
        private readonly UpdateExamPracticeCommandModel _request;
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly VoidMethodResult _errorResult;

        public ExamPracticeValidateBuilder(UpdateExamPracticeCommandModel request, IExamPracticeRepository examPracticeRepository)
        {
            _request = request;
            _examPracticeRepository = examPracticeRepository;
            _errorResult = new VoidMethodResult();
        }

        public static ExamPracticeValidateBuilder Create(UpdateExamPracticeCommandModel request, IExamPracticeRepository examPracticeRepository)
        {
            return new ExamPracticeValidateBuilder(request, examPracticeRepository);
        }

        public ExamPracticeValidateBuilder ValidateRequestData()
        {
            return this;
        }

        public async Task<ExamPracticeValidateBuilder> ValidateDuplicateTestAsync(Guid originalId)
        {
            var isDuplicatedExamPractice = await _examPracticeRepository.Queryable.AsQueryable().AnyAsync(u => u.Code == _request.Code && u.OriginalId != originalId).ConfigureAwait(false);
            if (isDuplicatedExamPractice)
            {
                _errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), $"{nameof(_request.Code)} and {nameof(originalId)}");
            }

            return this;
        }

        public VoidMethodResult GetResult()
        {
            return _errorResult;
        }
    }
}
