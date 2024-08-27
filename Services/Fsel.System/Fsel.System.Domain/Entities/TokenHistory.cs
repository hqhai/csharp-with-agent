// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class TokenHistory : Entity
    {
        private double _remainToken;
        public Guid? TokenConfigId { get; set; }

        /// <summary>
        /// Token Ban Đầu
        /// </summary>
        public double InitialToken { get; set; }

        ///
        /// <summary>
        /// Token biến động
        /// </summary>
        public double VolatileToken { get; set; }

        /// <summary>
        /// Token đạt được
        /// </summary>
        public double RemainToken
        {
            get
            {
                _remainToken = InitialToken + (Type == EnumTokenHistoryType.Recevived ? VolatileToken : -VolatileToken);
                return _remainToken;
            }
            set { _remainToken = value; }
        }

        public EnumTokenFeature Feature { get; set; }

        public EnumTokenMission? Mission { get; set; }

        public Guid UserId { get; set; }

        public Guid? ObjectId { get; set; }
        public Guid? CourseResultId { get; set; }
        public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public string? EventCode { get; set; }
        public EnumTokenHistoryType Type { get; set; }
        public TokenConfig? TokenConfig { get; set; }
    }
}
