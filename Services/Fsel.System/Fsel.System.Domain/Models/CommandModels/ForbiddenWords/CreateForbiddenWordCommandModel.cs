// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ForbiddenWords
{
    public class CreateForbiddenWordCommandModel
    {
        public string? Word { get; set; }

        public string? Description { get; set; }
    }
}
