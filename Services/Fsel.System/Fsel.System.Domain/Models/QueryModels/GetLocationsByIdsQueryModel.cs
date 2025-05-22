// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Shared.Helpers;
    using global::System;
    using global::System.Collections.Generic;

    public class GetLocationsByIdsQueryModel
    {
        public IList<Guid>? Ids { get; set; }

        private string? _idsStr;

        public string? IdsStr
        {
            get
            {
                return _idsStr;
            }
            set
            {
                _idsStr = value;
                if (!string.IsNullOrEmpty(_idsStr) && (Ids == null || !Ids.Any()))
                {
                    Ids = _idsStr.ToList<Guid>();
                }
            }
        }
    }
}
