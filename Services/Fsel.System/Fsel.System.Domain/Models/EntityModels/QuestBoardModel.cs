// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class QuestBoardModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        private string? _imagePath;
        public string? ImagePath
        {
            set { _imagePath = value; }
            get { return _imagePath.AddS3BaseUrl(); }
        }
        public EnumQuestBoardType Type { get; set; }
        public EnumQuestBoardCategory Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfStars { get; set; }
        public bool IsLifeTime { get; set; }
        public EnumRepeatType? RepeatType { get; set; }
        public IList<Guid>? PackageIds { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public Guid? DependentId { get; set; }
    }
}
