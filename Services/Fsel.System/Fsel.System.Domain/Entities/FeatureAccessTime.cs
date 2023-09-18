// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class FeatureAccessTime : Entity
    {
        /// <summary>
        /// Chức năng
        /// </summary>
        public EnumFeature EnumFeature { get; set; }

        /// <summary>
        /// Số lần đăng nhập
        /// </summary>
        public int Visit { get; set; }

        /// <summary>
        /// Số giây học sinh thực hiện
        /// </summary>
        public long AccessTime { get; set; }

        /// <summary>
        /// Lần cuối học sinh thực hiện
        /// </summary>
        public DateTime? LastVisited { get; set; }

        public Guid ObjectId { get; set; }
    }
}
