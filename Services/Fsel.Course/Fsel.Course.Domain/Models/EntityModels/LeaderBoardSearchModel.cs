// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LeaderBoardSearchModel
    {
        public IList<LeaderBoardModel>? LeaderBoards { get; set; }
        public LeaderBoardModel? LeaderBoard { get; set; }
    }
}
