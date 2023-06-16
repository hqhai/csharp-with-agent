// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using Fsel.Core.Entities;

    public class PostTag : Entity
    {
        /// <summary>
        /// Post
        /// </summary>
        public Guid PostId { get; set; }

        public Post? Post { get; set; }

        /// <summary>
        /// TopicTag
        /// </summary>
        public Guid TopicTagId { get; set; }

        public TopicTag? TopicTag { get; set; }
    }
}
