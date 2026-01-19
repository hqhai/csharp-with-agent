// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels
{
    using Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;

    public class NavigateUserAggregate
    {
        public IList<CourseChangingHistory>? ChangeHistories { get; set; }

        private readonly IList<TestGroupResult>? _ptResults;

        public CurrentStateInfo CurrentStateInfo { get; set; }

        public NavigateUserAggregate(CurrentStateInfo currentStateInfo, IList<CourseChangingHistory>? changeHistories, IList<TestGroupResult>? ptResults)
        {
            CurrentStateInfo = currentStateInfo;
            ChangeHistories = changeHistories;
            _ptResults = ptResults;
        }

        public NavigateAction GetNavigateAction()
        {
            if (NotDoingYetAnything())
            {
                return new NavigateAction
                {
                    PtResultId = null,
                    Status = EnumNavigateActionStatus.NotDoingYetAnything
                };
            }

            if (IsResultBeforeHistoryAvailable())
            {
                var latestPtResult = _ptResults?.OrderByDescending(pt => pt.CreatedDate).FirstOrDefault();

                if (latestPtResult?.Status is EnumResultStatus.Done or EnumResultStatus.ByPass)
                {
                    if (CurrentStateInfo.LevelId == null || CurrentStateInfo.ProgramId == null)
                    {
                        return new NavigateAction
                        {
                            PtResultId = latestPtResult.Id,
                            LevelOfPt = latestPtResult.CurrentLevelId,
                            Status = EnumNavigateActionStatus.ChooseLevel
                        };
                    }

                    return new NavigateAction
                    {
                        PtResultId = CurrentStateInfo.PtResultId ?? latestPtResult.Id,
                        LevelOfPt = latestPtResult.CurrentLevelId,
                        Status = EnumNavigateActionStatus.ContinueLearning
                    };
                }

                return new NavigateAction
                {
                    PtResultId = latestPtResult?.Id,
                    Status = EnumNavigateActionStatus.ContinuePt
                };
            }
            else
            {
                var latestHistory = ChangeHistories.OrderByDescending(ch => ch.CreatedDate).First();
                var relatedPtResult = _ptResults?.FirstOrDefault(x => x.Id == latestHistory.PtResultId);
                if (latestHistory?.Status == EnumChangingStatus.InProgressSelectCourse)
                {
                    return new NavigateAction
                    {
                        PtResultId = latestHistory.PtResultId,
                        RelatedHistoryId = latestHistory.Id,
                        LevelOfPt = relatedPtResult?.CurrentLevelId,
                        Status = EnumNavigateActionStatus.ChooseLevel
                    };
                }
                else if (latestHistory?.Status == EnumChangingStatus.InprogressSelectProgram)
                {
                    return new NavigateAction
                    {
                        RelatedHistoryId = latestHistory.Id,
                        Status = EnumNavigateActionStatus.ChooseProgram
                    };
                }
                else if (latestHistory?.Status == EnumChangingStatus.Completed)
                {
                    return new NavigateAction
                    {
                        PtResultId = relatedPtResult?.Id,
                        LevelOfPt = relatedPtResult?.CurrentLevelId,
                        Status = EnumNavigateActionStatus.ContinueLearning
                    };
                }

                return new NavigateAction
                {
                    PtResultId = relatedPtResult?.Id,
                    LevelOfPt = relatedPtResult?.CurrentLevelId,
                    Status = EnumNavigateActionStatus.ContinuePt
                };
            }
        }

        private bool NotDoingYetAnything()
        {
            return _ptResults == null || !_ptResults.Any();
        }

        private bool IsResultBeforeHistoryAvailable()
        {
            return ChangeHistories == null || !ChangeHistories.Any();
        }
    }

    public record NavigateAction
    {
        public Guid? PtResultId { get; set; }
        public Guid? LevelOfPt { get; set; }
        public Guid? RelatedHistoryId { get; set; }
        public EnumNavigateActionStatus Status { get; set; } = EnumNavigateActionStatus.NotDoingYetAnything;
    }

    public enum EnumNavigateActionStatus
    {
        NotDoingYetAnything,
        ChooseProgram,
        ChooseLevel,
        ContinuePt,
        ContinueLearning
    }
}
