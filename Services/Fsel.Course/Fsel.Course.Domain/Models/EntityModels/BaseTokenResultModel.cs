// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class BaseTokenResultModel : BaseLearnResultModel
    {
        public int TokenDone { get; set; }
        public int TokenHighestStreak { get; set; }
        public int TokenSuperFire { get; set; }
    }
}
