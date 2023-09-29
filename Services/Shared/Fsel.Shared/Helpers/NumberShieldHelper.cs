// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;

    public class NumberShieldConfig
    {
        public NumberShieldConfig(int numberOfShield, EnumPackageCode packageCode)
        {
            NumberOfShield = numberOfShield;
            PackageCode = packageCode;
        }

        public int NumberOfShield { get; set; }
        public EnumPackageCode PackageCode { get; set; }
    }

    public static class NumberShieldHelper
    {
        private static IList<NumberShieldConfig> s_numberShieldConfigs = new List<NumberShieldConfig>
        {
            new NumberShieldConfig(1, EnumPackageCode.BASIC),
            new NumberShieldConfig(2, EnumPackageCode.STANDARD),
            new NumberShieldConfig(3, EnumPackageCode.PREMIUM),
        };

        public static int GetNumberToken(this EnumPackageCode packageCode)
        {
            var config = s_numberShieldConfigs.FirstOrDefault(x => x.PackageCode == packageCode);
            if (config != null)
            {
                return config.NumberOfShield;
            }
            return default;
        }
    }
}
