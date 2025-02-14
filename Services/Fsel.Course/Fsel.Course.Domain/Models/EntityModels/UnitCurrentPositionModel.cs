// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class UnitCurrentPositionModel
    {
        public Guid StudentId { get; set; }
        public int DisplayUnit { get; set; } = 1;
        public int DisplayLesson { get; set; } = 1;
    }
}
