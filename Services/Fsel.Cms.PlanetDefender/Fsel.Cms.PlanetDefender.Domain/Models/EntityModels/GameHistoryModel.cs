// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class GameHistoryModel : BaseModel
    {
        public int RoundNumber { get; set; }

        public long Score { get; set; }

        public Guid SpaceShipId { get; set; }
        public Guid StudentGameInfoId { get; set; }

        public string? SpaceShipCode { get; set; }
        public int NumberOfToken { get; set; }
        public int ImpactNumber { get; set; }
        public int DestroyNumber { get; set; }
        public int ComboNumber { get; set; }
        public int ZPlanetNumber { get; set; }
        public long TotalScore { get; set; }
        public IList<StudentAnswerModel>? StudentAnswers { get; set; }
    }

    public class StudentAnswerModel
    {
        public EnumGameVocabType? Type { get; set; }
        public string? QuestionContent { get; set; }
        public string? Key { get; set; }
        public string? Answer { get; set; }
        public bool IsCorrect { get; set; }
    }
}
