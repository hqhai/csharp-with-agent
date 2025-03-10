// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.QueryModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class GetAccessTimeByUserAndFeatureQueryModel
    {
        public IList<Guid>? UserIds { get; set; }

        public IList<EnumFeature>? Features { get; set; }
    }
}
