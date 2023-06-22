// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.InterationActions
{
    public class CheckIsLikedByObjectIdsQueryModel
    {
        public IList<Guid>? ObjectIds { get; set; }

        public Guid? CurrentUserId { get; set; }
    }
}
