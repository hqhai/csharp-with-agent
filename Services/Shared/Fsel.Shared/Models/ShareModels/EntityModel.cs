// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class EntityModel
    {
        public Guid Id { get; set; }

        public Guid CreatedUserId { get; set; }

        public Guid? UpdatedUserId { get; set; }

        public Guid? DeletedUserId { get; set; }

        public string? CreatedFullName { get; set; }

        public string? UpdatedFullName { get; set; }

        public string? DeletedFullName { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public DateTime? DeletedDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
