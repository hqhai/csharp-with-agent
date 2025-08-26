// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers
{
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections;
    using Fsel.ExamPractice.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Enums;

    public class ExamPracticeCommon
    {
        private int _countReadingQuestion;
        private int _countListenningQuestion;
        private EnumCourseSkill? _positionCourseSkill;

        public ExamPracticeCommon()
        {

        }

        public ExamPracticeCommon Create()
        {
            return new ExamPracticeCommon();
        }

        public void HandlerTotalQuestion(IList<UpdateExamPracticeSectionCommandModel> examPracticeSections)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSections);

            foreach (var examPracticeSection in examPracticeSections)
            {
                CountQuestion(examPracticeSection.CourseSkill, (examPracticeSection.ChildrenExamPracticeSections != null && examPracticeSection.ChildrenExamPracticeSections.Any()), examPracticeSection.Questions);

                if (examPracticeSection.ChildrenExamPracticeSections != null && examPracticeSection.ChildrenExamPracticeSections.Any())
                {
                    HandlerTotalQuestion(examPracticeSection.ChildrenExamPracticeSections);
                }
            }
        }

        public void SetTotalQuestion(IList<UpdateExamPracticeSectionCommandModel> examPracticeSections)
        {
            ArgumentNullException.ThrowIfNull(examPracticeSections);

            foreach (var examPracticeSection in examPracticeSections)
            {
                if (examPracticeSection.CourseSkill.HasValue && examPracticeSection.Config != null)
                {
                    switch (examPracticeSection.CourseSkill)
                    {
                        case EnumCourseSkill.Reading:
                            examPracticeSection.Config.TotalQuestion = _countReadingQuestion;
                            break;
                        case EnumCourseSkill.Listening:
                            examPracticeSection.Config.TotalQuestion = _countListenningQuestion;
                            break;
                    }
                }
            }
        }

        private void CountQuestion(EnumCourseSkill? courseSkill, bool isHadChildent, IList<UpdateQuestionCommandModel>? questions)
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
                }
            }

            if (_positionCourseSkill.HasValue && questions != null && questions.Any())
            {
                switch (_positionCourseSkill)
                {
                    case EnumCourseSkill.Reading:
                        _countReadingQuestion += questions.Count;
                        break;
                    case EnumCourseSkill.Listening:
                        _countListenningQuestion += questions.Count;
                        break;
                }
            }

            if (!isHadChildent)
            {
                _positionCourseSkill = null;
            }
        }
    }
}
