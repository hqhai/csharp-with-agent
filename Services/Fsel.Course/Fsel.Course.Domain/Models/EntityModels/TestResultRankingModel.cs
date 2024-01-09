// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;

    public class TestResultRankingModel : BaseScoreResultModel
    {
        public bool IsCurrentStudent { get; set; }
        public int CorrectQuestion { get; set; }
        public double? WorkingTime { get; set; }

        private string? _avatarPath;
        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public string? FullName { get; set; }

        public double? Score { get; set; }
    }
}
