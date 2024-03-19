// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Models.ShareModels;

    public static class TokenConfigHelper
    {
        public static T? GetTokenConfig<T>(this TokenConfigModel? tokenConfigModel) where T : class
        {
            if (tokenConfigModel == null)
            {
                return default;
            }

            return tokenConfigModel.Config?.Deserialize<T>();
        }
    }
}
