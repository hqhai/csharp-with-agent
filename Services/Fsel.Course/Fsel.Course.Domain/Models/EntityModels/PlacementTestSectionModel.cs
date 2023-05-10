// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;

    public class PlacementTestSectionModel
    {
        public Guid PlacementTestId { get; set; }
        public Guid SectionGroupId { get; set; }
    }
}
