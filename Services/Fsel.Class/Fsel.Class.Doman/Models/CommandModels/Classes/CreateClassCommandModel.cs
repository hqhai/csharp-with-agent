// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Doman.Models.CommandModels.Classes
{
    using Fsel.Class.Doman.Enums;

    public class CreateClassCommandModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public EnumClassType Status { get; set; }
        public Guid StudentId { get; set; }
    }
}
