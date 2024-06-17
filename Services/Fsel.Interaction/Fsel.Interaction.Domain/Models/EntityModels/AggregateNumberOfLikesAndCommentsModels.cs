// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    public class AggregateNumberOfLikesAndCommentsModels
    {
        public AggregateNumberOfLikesAndCommentsModel? Receive { get; set; }
        public AggregateNumberOfLikesAndCommentsModel? Give { get; set; }
    }

    public class AggregateNumberOfLikesAndCommentsModel
    {
        public int NumberLike { get; set; }
        public int NumberComment { get; set; }
    }
}
