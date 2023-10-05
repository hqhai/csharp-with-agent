// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.InteractionService.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class CreateCustomerSurveyCommandModel
    {
        public IList<CreateSurveyCommandModel>? Answers { get; set; }

        public Guid? UserId { get; set; }

        public bool? IsPilot { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }

    public class CreateSurveyCommandModel
    {
        public Guid Id { get; set; }
        public object? Answer { get; set; }
    }
}
