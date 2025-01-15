// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.LeaderBoardRewards
{
    public class LeaderBoardCollectiveAwardModel
    {
        public string? SchoolName { get; set; }

        public int AmountFinishPT { get; set; }
        public int AmountFinishUnitOne { get; set; }
        public int AmountFinishUnitTwo { get; set; }
        public int AmountFinishUnitThree { get; set; }
        public int AmountFinishUnitFour { get; set; }

    }
}
