// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using System.Text.Json.Serialization;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using static Fsel.Shared.Constants.ValueSettings;

    public class ToolTestOverallScoreCommand : IRequest<MethodResult<double>>
    {
        public string? Type { get; set; }
        public EnumCourseType CourseType { get; set; }
        public IList<SkillScoreTest>? SkillScoreVideos { get; set; }
        public IList<SkillScoreTest>? SkillScoreHomeWorks { get; set; }
        public IList<SkillScoreTest>? SkillScoreClassForums { get; set; }
        public IList<SkillScoreTest>? SkillScoreUnitTests { get; set; }
        public IList<SkillScoreTest>? SkillScoreSkillTests { get; set; }
        public IList<double>? OverallUnitTestPercents { get; set; }
        public IList<double>? OverallSkillTestPercents { get; set; }
        public int PercentFinalTest { get; set; }
    }

    public class SkillScoreTest
    {
        [JsonRequired]
        public EnumCourseSkill Skill { get; set; }

        [JsonRequired]
        public double Percent { get; set; }
    }

    public class ToolTestOverallScoreCommandHandler : IRequestHandler<ToolTestOverallScoreCommand, MethodResult<double>>
    {
        public ToolTestOverallScoreCommandHandler()
        {
        }

        public async Task<MethodResult<double>> Handle(ToolTestOverallScoreCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<double>();
            double percent = 0;
            if (request.Type == nameof(Course))
            {
                percent = GetCourseResult(request);
            }
            else
            {
                percent = GetUnitResult(request);
            }
            methodResult.Result = percent;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private double GetCourseResult(ToolTestOverallScoreCommand request)
        {
            var percentVideo = GetVideoSkillScores(request);
            var percentHomeWork = GetHomeWordsSkillScores(request);
            var percentClassForum = GetClassForumSkillScores(request);

            var percents = new List<double> { percentClassForum, percentHomeWork, percentVideo };
            if (request.CourseType == EnumCourseType.Academic)
            {
                var percentUnitSkill = GetSkillScoreByCourses(request.OverallUnitTestPercents, OverallPercentCourse.OverallAcaPercentUnitTest);
                var percentSkillTest = GetSkillScoreByCourses(request.OverallSkillTestPercents, OverallPercentCourse.OverallAcaPercentSkillTest);
                var percentFinalTest = GetFinalTestPercent(request);
                percents.AddRange(new List<double> { percentUnitSkill, percentSkillTest, percentFinalTest });
            }
            if (request.CourseType == EnumCourseType.EnglishFoundation)
            {
                var percentUnitSkill = GetSkillScoreByCourses(request.OverallUnitTestPercents, OverallPercentCourse.OverallRFIPercentUnitTest);
                var percentFinalTest = GetFinalTestPercent(request);
                percents.AddRange(new List<double> { percentUnitSkill, percentFinalTest });
            }
            return NumberHelper.ConvertRound(percents.Sum());
        }

        private double GetUnitResult(ToolTestOverallScoreCommand request)
        {
            var percents = new List<double>();
            if (request.CourseType == EnumCourseType.Academic)
            {
                var percentVideo = GetVideoSkillScores(request, percentSkill: OverallPercentUnit.OverallAcaPercentVideo);
                var percentUnitTest = GetVideoSkillScores(request, EnumTimeCodeType.UnitTest, OverallPercentUnit.OverallAcaPercentUnitTest);
                var percentSkillTest = GetVideoSkillScores(request, EnumTimeCodeType.SkillTest, OverallPercentUnit.OverallAcaPercentSkillTest);
                var percentHomeWork = GetHomeWordsSkillScores(request, OverallPercentUnit.OverallAcaPercentHomeWork);
                var percentClassForum = GetClassForumSkillScores(request, OverallPercentUnit.OverallAcaPercentClassForum);
                percents = new List<double> { percentClassForum, percentHomeWork, percentSkillTest, percentUnitTest, percentVideo };
            }
            else if (request.CourseType == EnumCourseType.Ielts)
            {
                var percentVideo = GetVideoSkillScores(request);
                var percentHomeWork = GetHomeWordsSkillScores(request);
                var percentClassForum = GetClassForumSkillScores(request);
                percents = new List<double> { percentClassForum, percentHomeWork, percentVideo };
            }
            else if (request.CourseType == EnumCourseType.EnglishFoundation)
            {
                var percentVideo = GetVideoSkillScores(request, percentSkill: OverallPercentUnit.OverallRFIPercentVideo);
                var percentUnitTest = GetVideoSkillScores(request, EnumTimeCodeType.UnitTest, OverallPercentUnit.OverallRFIPercentUnitTest);
                var percentHomeWork = GetHomeWordsSkillScores(request, OverallPercentUnit.OverallRFIPercentHomeWork);
                var percentClassForum = GetClassForumSkillScores(request, OverallPercentUnit.OverallRFIPercentClassForum);
                percents = new List<double> { percentClassForum, percentHomeWork, percentUnitTest, percentVideo };
            }

            return NumberHelper.ConvertRound(percents.Sum());
        }

        public double GetVideoSkillScores(ToolTestOverallScoreCommand request, EnumTimeCodeType? timeCodeType = null, int percentSkill = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            List<SkillScoreTest> skillScores = (request.SkillScoreVideos ?? new List<SkillScoreTest>()).ToList();
            if (timeCodeType.HasValue)
            {
                switch (timeCodeType.Value)
                {
                    case EnumTimeCodeType.UnitTest:
                        skillScores = (request.SkillScoreUnitTests ?? new List<SkillScoreTest>()).ToList();
                        break;

                    case EnumTimeCodeType.SkillTest:
                        skillScores = (request.SkillScoreSkillTests ?? new List<SkillScoreTest>()).ToList();
                        break;
                }
            }
            if (request.Type == nameof(Course))
            {
                if (request.CourseType == EnumCourseType.Ielts)
                {
                    return skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Sum(x =>
                    {
                        if (x.Skill == EnumCourseSkill.Writing || x.Skill == EnumCourseSkill.Speaking)
                        {
                            return NumberHelper.ConvertRound(x.Percent * OverallPercentCourse.SkillSWIELTSPercentVideo);
                        }
                        return NumberHelper.ConvertRound(x.Percent * OverallPercentCourse.SkillIELTSPercentVideo);
                    })) : default;
                }
                switch (request.CourseType)
                {
                    case EnumCourseType.Academic:
                        percentSkill = OverallPercentCourse.OverallAcaPercentVideo;
                        break;

                    case EnumCourseType.EnglishFoundation:
                        percentSkill = OverallPercentCourse.OverallRFIPercentVideo;
                        break;
                }
            }

            return GetDoublePercent(skillScores, percentSkill);
        }

        public double GetClassForumSkillScores(ToolTestOverallScoreCommand request, int percentSkill = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            List<SkillScoreTest> skillScores = (request.SkillScoreClassForums ?? new List<SkillScoreTest>()).ToList();
            if (request.Type == nameof(Course))
            {
                switch (request.CourseType)
                {
                    case EnumCourseType.Academic:
                        percentSkill = OverallPercentCourse.OverallAcaPercentClassForum;
                        break;

                    case EnumCourseType.Ielts:
                        percentSkill = OverallPercentCourse.OverallIELTSPercentClassForum;
                        break;

                    case EnumCourseType.EnglishFoundation:
                        percentSkill = OverallPercentCourse.OverallRFIPercentClassForum;
                        break;
                }
            }
            return GetDoublePercent(skillScores, percentSkill);
        }

        public double GetHomeWordsSkillScores(ToolTestOverallScoreCommand request, int percentSkill = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            List<SkillScoreTest> skillScores = (request.SkillScoreHomeWorks ?? new List<SkillScoreTest>()).ToList();
            if (request.Type == nameof(Course))
            {
                switch (request.CourseType)
                {
                    case EnumCourseType.Academic:
                        percentSkill = OverallPercentCourse.OverallAcaPercentHomeWork;
                        break;

                    case EnumCourseType.Ielts:
                        percentSkill = OverallPercentCourse.OverallIELTSPercentHomeWork;
                        break;

                    case EnumCourseType.EnglishFoundation:
                        percentSkill = OverallPercentCourse.OverallRFIPercentHomeWork;
                        break;
                }
            }
            return GetDoublePercent(skillScores, percentSkill);
        }

        private static double GetDoublePercent(IList<SkillScoreTest>? skillScores, int percentOccupy, int numberOfElements = default)
        {
            if (skillScores == null || !skillScores.Any())
            {
                return default;
            }
            if (numberOfElements != default)
            {
                return NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent * percentOccupy / numberOfElements));
            }
            else
            {
                return NumberHelper.ConvertDoublePercent(skillScores.Sum(x => x.Percent * percentOccupy / skillScores.Count));
            }
        }

        private static double GetSkillScoreByCourses(IList<double>? overallUnitTestPercent, int percentSkill = default)
        {
            if (overallUnitTestPercent == null || !overallUnitTestPercent.Any())
            {
                return percentSkill;
            }
            var percents = overallUnitTestPercent.Select(x => x * percentSkill / overallUnitTestPercent.Count).ToList();
            return NumberHelper.ConvertDoublePercent(percents.Sum());
        }

        private static double GetFinalTestPercent(ToolTestOverallScoreCommand request)
        {
            var percentSkill = 0;
            if (request.CourseType == EnumCourseType.Academic)
            {
                percentSkill = OverallPercentCourse.OverallAcaPercentFinalTest;
            }
            else if (request.CourseType == EnumCourseType.EnglishFoundation)
            {
                percentSkill = OverallPercentCourse.OverallRFIPercentFinalTest;
            }
            return NumberHelper.ConvertDoublePercent(request.PercentFinalTest * percentSkill);
        }
    }
}
