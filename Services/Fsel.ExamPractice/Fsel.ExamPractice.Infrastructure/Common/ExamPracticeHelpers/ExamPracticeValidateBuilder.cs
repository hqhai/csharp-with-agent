// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class ExamPracticeValidateBuilder
    {
        private readonly UpdateExamPracticeCommandModel _request;
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly VoidMethodResult _errorResult;
        private int _totalReadingQuestionVstep = 40;
        private int _totalListenningQuestionVstep = 35;

        public ExamPracticeValidateBuilder(UpdateExamPracticeCommandModel request,
                                           IExamPracticeRepository examPracticeRepository)
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

        public async Task<ExamPracticeValidateBuilder> ValidateChangeStatus(EnumExamPracticeType type, IExamPracticeSectionRepository examPracticeSectionRepository, Guid examPracticeId)
        {
            switch (type)
            {
                case EnumExamPracticeType.Vstep:
                    await ValidateTotalQuestionVstepAsync(examPracticeSectionRepository, examPracticeId);
                    break;
            }
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

        public async Task<ExamPracticeValidateBuilder> ValidateTotalQuestionVstepAsync(IExamPracticeSectionRepository examPracticeSectionRepository, Guid examPracticeId)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSectionRepository);

            var examPracticeSections = await examPracticeSectionRepository.Queryable
                                                                          .Where(x => x.ExamPracticeId == examPracticeId)
                                                                          .AsQueryable()
                                                                          .ToListAsync();

            if (examPracticeSections == null || !examPracticeSections.Any())
            {
                return this;
            }

            foreach (var item in examPracticeSections)
            {
                if (item.Config != null && item.Config.TotalQuestion < _totalReadingQuestionVstep && item.CourseSkill == EnumCourseSkill.Reading)
                {
                    _errorResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.InvalidReadingQuestionCount), nameof(item.Config.TotalQuestion));
                }

                if (item.Config != null && item.Config.TotalQuestion < _totalListenningQuestionVstep && item.CourseSkill == EnumCourseSkill.Listening)
                {
                    _errorResult.AddErrorBadRequest(nameof(EnumExamPracticeErrorCode.InvalidListenningQuestionCount), nameof(item.Config.TotalQuestion));
                }
            }

            return this;
        }

        public VoidMethodResult GetResult()
        {
            return _errorResult;
        }
    }
}
