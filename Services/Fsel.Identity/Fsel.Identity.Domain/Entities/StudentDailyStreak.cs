// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class StudentDailyStreak : Entity
    {
        /// <summary>
        /// Gift received
        /// </summary>
        public int? LevelOfGift { get; set; }

        /// <summary>
        /// Used Shield
        /// </summary>
        public bool IsUseShield { get; set; }

        /// <summary>
        /// Gift Receive
        /// </summary>
        public bool IsGiftReceive { get; set; }

        /// <summary>
        /// Armorial Receive
        /// </summary>
        public bool IsArmorialReceive { get; set; }

        /// <summary>
        /// Daily
        /// </summary>
        public DateTime DailyDate { get; set; }

        public Student? Student { get; set; }
        public Guid StudentId { get; set; }
    }
}
