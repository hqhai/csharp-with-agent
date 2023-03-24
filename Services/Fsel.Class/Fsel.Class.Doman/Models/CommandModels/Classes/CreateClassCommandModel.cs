// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Doman.Models.CommandModels.Classes
{
    public class CreateClassCommandModel
    {
        public string? Code { get; set; }
        public Guid ClassId { get; set; }
        public Guid UserId { get; set; }
    }
}
