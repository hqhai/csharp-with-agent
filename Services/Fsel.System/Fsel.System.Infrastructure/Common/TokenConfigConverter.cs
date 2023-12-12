// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Common
{
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Entities.Configs;

    public class TokenConfigConverter
    {
        public int GetTotalCorrectByAnswerType(object? config, Guid? objectId, int? level)
        {
            if (objectId.HasValue)
            {
                return GetToken(config, objectId.Value);
            }
            else if (level.HasValue)
            {
                return GetToken(config, level.Value);
            }
            else
            {
                return GetToken(config);
            }
        }

        private static int GetToken(object? config, Guid objectId)
        {
            var data = config.Deserialize<TokenForcusTime>();
            return data?.FocusTimes?.FirstOrDefault(x => x.FocusTimeId == objectId)?.Number ?? default;
        }

        private static int GetToken(object? config, int level)
        {
            var data = config.Deserialize<TokenDailyCheckin>();
            return data?.DailyCheckins?.FirstOrDefault(x => x.Level == level)?.Number ?? default;
        }

        private static int GetToken(object? config)
        {
            var data = config.Deserialize<TokenNumber>();
            return data?.Number ?? default;
        }
    }
}
