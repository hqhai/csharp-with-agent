// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class TokenConfigsConverter
    {
        private readonly IMapper _mapper;

        public TokenConfigsConverter(IMapper mapper)
        {
            _mapper = mapper;
        }

        public (object?, bool) GetTokenConfigs(object? config, object? configOld, EnumTokenFeature feature)
        {
            bool validateData = false;
            switch (feature)
            {
                case EnumTokenFeature.FocusMode:
                    var configFocusModes = config.Deserialize<IList<TokenCoinConfigs>>();
                    validateData = Validate(configFocusModes);

                    var configFocusModeOlds = configOld.Deserialize<IList<TokenConfigFocusModes>>();
                    configFocusModeOlds = configFocusModeOlds?.OrderBy(x => x.DisplayOrder).ToList();
                    if (configFocusModes != null && configFocusModeOlds != null)
                    {
                        int index = 0;
                        foreach (var item in configFocusModes)
                        {
                            var configFocusMode = configFocusModeOlds[index];
                            if (configFocusMode != null)
                            {
                                _mapper.Map(item, configFocusMode);
                            }
                            index++;
                        }
                    }
                    config = configFocusModeOlds;

                    break;

                case EnumTokenFeature.DailyCheckin:
                    var configDailyCheckIns = config.Deserialize<IList<TokenCoinConfigs>>();
                    validateData = Validate(configDailyCheckIns);

                    var configDailyCheckInOlds = configOld.Deserialize<IList<TokenConfigDailyCheckIns>>();
                    configDailyCheckInOlds = configDailyCheckInOlds?.OrderBy(x => x.Level).ToList();
                    if (configDailyCheckIns != null && configDailyCheckInOlds != null)
                    {
                        int index = 0;
                        foreach (var item in configDailyCheckIns)
                        {
                            var configDailyCheckIn = configDailyCheckInOlds[index];
                            if (configDailyCheckIn != null)
                            {
                                _mapper.Map(item, configDailyCheckIn);
                            }
                            index++;
                        }
                    }

                    config = configDailyCheckInOlds;

                    break;

                case EnumTokenFeature.Learn:
                case EnumTokenFeature.Test:
                case EnumTokenFeature.Achievement:
                    var configs = config.Deserialize<TokenCoinConfigs>();
                    validateData = Validate(configs);

                    var configOlds = configOld.Deserialize<TokenConfigs>();
                    _mapper.Map(configs, configOlds);
                    config = configOlds;
                    break;
            }
            return (config, validateData);
        }

        private static bool Validate(TokenCoinConfigs? configs)
        {
            if (configs == null || configs.TotalActions < 0 || configs.BaseValue < 0)
            {
                return false;
            }
            return true;
        }

        private static bool Validate(IList<TokenCoinConfigs>? configs)
        {
            var boolValidates = configs?.Select(x => Validate(x)).ToList();
            return boolValidates != null && boolValidates.All(x => x);
        }
    }
}
