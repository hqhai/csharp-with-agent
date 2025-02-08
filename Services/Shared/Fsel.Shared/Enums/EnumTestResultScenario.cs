// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    public enum EnumTestResultScenario
    {
        BelowSuggestedLevelAndAge = 1, // Kết quả PT < Level đề xuất theo tuổi và Độ tuổi của User > Độ tuổi đề xuất
        EqualToSuggestedLevel = 2,    // Kết quả PT = Level đề xuất theo tuổi
        AboveSuggestedLevel = 3       // Kết quả PT > Level đề xuất theo tuổi
    }
}
