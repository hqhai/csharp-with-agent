// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GameVocabularies
{
    public class MassUploadVocabularyCommandModel
    {
        public IList<CreateGameVocabularyCommandModel>? GameVocabularies { get; set; }
    }
}
