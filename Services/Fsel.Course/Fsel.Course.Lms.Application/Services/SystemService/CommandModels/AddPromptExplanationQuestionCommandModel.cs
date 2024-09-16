// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.CommandModels
{
    using Fsel.Shared.Models.ShareModels;

    public class AddPromptExplanationQuestionCommandModel
    {
        public IList<AddQuestionExplanationPromptModel>? AddPromptExplanationQuestions { get; set; }
    }
}
