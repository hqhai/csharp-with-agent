// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers
{
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.Shared.Enums;

    public class ExamPracticeCommon
    {
        private int _countReadingQuestion;
        private int _countListenningQuestion;
        private EnumCourseSkill? _positionCourseSkill;
        private int _countQuestion;

        public ExamPracticeCommon()
        {
        }

        public ExamPracticeCommon Create()
        {
            return new ExamPracticeCommon();
        }

        public void HanderQuestionIndexSection(ICollection<ExamPracticeSection> examPracticeSections)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSections);

            foreach (var examPracticeSection in examPracticeSections.Where(x => !x.ParentExamPracticeSectionId.HasValue))
            {
                _countQuestion = 0;
                CountQuestion(examPracticeSection.CourseSkill, examPracticeSection.Questions);
                if (examPracticeSection.ExamPracticeSections != null && examPracticeSection.ExamPracticeSections.Any())
                {
                    HanderSubQuestionIndexSection(examPracticeSection.ExamPracticeSections.OrderBy(x => x.DisplayOrder).ToList());
                }
            }

            SetTotalQuestion(examPracticeSections);
        }

        public void HanderSubQuestionIndexSection(ICollection<ExamPracticeSection> examPracticeSections)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSections);

            foreach (var examPracticeSection in examPracticeSections)
            {
                var numberQuestion = _countQuestion;
                numberQuestion++;

                CountQuestion(examPracticeSection.CourseSkill, examPracticeSection.Questions);
                if (examPracticeSection.ExamPracticeSections != null && examPracticeSection.ExamPracticeSections.Any())
                {
                    HanderSubQuestionIndexSection(examPracticeSection.ExamPracticeSections.OrderBy(x => x.DisplayOrder).ToList());
                }
                if (examPracticeSection.Questions != null && examPracticeSection.Questions.Any())
                {
                    HanderSubQuestionIndexQuestion(examPracticeSection.Questions.OrderBy(x => x.DisplayOrder).ToList());
                    examPracticeSection.SubQuestionIndexs = RangeInclusive(numberQuestion, _countQuestion);
                }
            }
        }

        private void CountQuestion(EnumCourseSkill? courseSkill, ICollection<Question>? questions)
        {
            if (courseSkill.HasValue)
            {
                switch (courseSkill)
                {
                    case EnumCourseSkill.Reading:
                        _positionCourseSkill = EnumCourseSkill.Reading;
                        break;

                    case EnumCourseSkill.Listening:
                        _positionCourseSkill = EnumCourseSkill.Listening;
                        break;

                    default:
                        _positionCourseSkill = null;
                        break;
                }
            }

            if (_positionCourseSkill.HasValue && questions != null && questions.Any())
            {
                switch (_positionCourseSkill)
                {
                    case EnumCourseSkill.Reading:
                        _countReadingQuestion += questions.Sum(x => x.CorrectTotal);
                        break;

                    case EnumCourseSkill.Listening:
                        _countListenningQuestion += questions.Sum(x => x.CorrectTotal);
                        break;
                }
            }
        }

        private void SetTotalQuestion(ICollection<ExamPracticeSection> examPracticeSections)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSections);

            foreach (var examPracticeSection in examPracticeSections)
            {
                if (examPracticeSection.CourseSkill.HasValue && examPracticeSection.Config != null)
                {
                    switch (examPracticeSection.CourseSkill)
                    {
                        case EnumCourseSkill.Reading:
                            var configReading = examPracticeSection.Config;
                            configReading.TotalQuestion = _countReadingQuestion;
                            examPracticeSection.Config = configReading;
                            break;

                        case EnumCourseSkill.Listening:
                            var configListening = examPracticeSection.Config;
                            configListening.TotalQuestion = _countListenningQuestion;
                            examPracticeSection.Config = configListening;
                            break;
                    }
                }
            }
        }

        private void HanderSubQuestionIndexQuestion(ICollection<Question> questions)
        {
            foreach (var question in questions)
            {
                var numberQuestion = _countQuestion;
                numberQuestion++;
                _countQuestion += question.CorrectTotal;
                question.SubQuestionIndexs = RangeInclusive(numberQuestion, _countQuestion);
            }
        }

        private static List<int> RangeInclusive(int start, int end)
        {
            return Enumerable.Range(start, end - start + 1).ToList();
        }
    }
}
