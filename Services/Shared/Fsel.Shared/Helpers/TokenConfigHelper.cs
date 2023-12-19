// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Models.ShareModels;

    public static class TokenConfigHelper
    {
        public static T? GetTokenNumber<T>(this TokenConfigModel? tokenConfigModel, bool isSuperMode = false) where T : class
        {
            if (tokenConfigModel == null)
            {
                return default;
            }

            var config = isSuperMode ? tokenConfigModel.SuperConfig : tokenConfigModel.Config;
            return config?.Deserialize<T>();
        }
    }
}
