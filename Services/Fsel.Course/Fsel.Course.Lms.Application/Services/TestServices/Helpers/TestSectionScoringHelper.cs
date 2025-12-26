// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Lms.Application.Services.TestServices.Interface;
    using Fsel.Shared.Enums;

    public static class TestSectionScoringHelper
    {
        /// <summary>
        /// Map raw value (correctCount/rawScore) theo ScoringFormulaConfigs.
        /// - Chọn rule có From lớn nhất nhưng <= rawValue.
        /// - Output theo formulaType:
        ///   + BandScore  : trả Equal (int) dưới dạng double
        ///   + Percent    : convert Equal (band) -> percent
        /// </summary>
        public static double MapScore(
            EnumScoringFormulaType formulaType,
            IReadOnlyList<ScoringFormulaConfig>? rules,
            double rawValue,
            IBandPercentConverter? converter = null,
            double? defaultWhenNoRule = 0d)
        {
            if (rules == null || rules.Count == 0)
                return defaultWhenNoRule ?? 0d;

            // Lọc rule hợp lệ
            var validRules = rules
                .Where(r => r != null)
                .Where(r => r.Equal.HasValue)
                .OrderByDescending(r => r.From)
                .ToList();

            if (validRules.Count == 0)
                return defaultWhenNoRule ?? 0d;

            // Chọn rule phù hợp
            var matched = validRules.FirstOrDefault(r => rawValue >= r.From)
                          ?? validRules.Last(); // nếu raw nhỏ hơn From nhỏ nhất

            var equal = (double)matched.Equal!.Value;

            return formulaType switch
            {
                EnumScoringFormulaType.BandScore => equal,
                EnumScoringFormulaType.Percent => (converter ?? new LinearBandPercentConverter()).BandToPercent(equal),
                _ => equal
            };
        }

        /// <summary>
        /// Validate input + rules trước khi dùng (để tránh cấu hình sai làm lệch điểm).
        /// </summary>
        public static void ValidateRules(
            IReadOnlyList<ScoringFormulaConfig>? rules,
            EnumScoringFormulaType formulaType,
            double? maxBand = 9d)
        {
            if (rules == null || rules.Count == 0)
                throw new InvalidOperationException("ScoringFormulaConfigs is empty.");

            // From nên không âm và không trùng nhau (khuyên)
            var froms = rules.Select(r => r.From).ToList();
            if (froms.Any(f => f < 0))
                throw new InvalidOperationException("Scoring rules: From must be >= 0.");

            if (froms.Distinct().Count() != froms.Count)
                throw new InvalidOperationException("Scoring rules: From must be unique.");

            // Equal null là invalid rule
            if (rules.Any(r => r.Equal is null))
                throw new InvalidOperationException("Scoring rules: Equal must not be null.");

            // Nếu output là band: Equal phải nằm trong range (tuỳ hệ)
            if (formulaType == EnumScoringFormulaType.BandScore && maxBand.HasValue)
            {
                var mb = maxBand.Value;
                if (rules.Any(r => r.Equal!.Value < 0 || r.Equal!.Value > mb))
                    throw new InvalidOperationException($"Scoring rules: Equal (band) must be in range [0..{mb}].");
            }

            // Nếu output là percent: Equal vẫn là band (vì model hiện tại), nên vẫn validate band
            if (formulaType == EnumScoringFormulaType.Percent && maxBand.HasValue)
            {
                var mb = maxBand.Value;
                if (rules.Any(r => r.Equal!.Value < 0 || r.Equal!.Value > mb))
                    throw new InvalidOperationException($"Scoring rules: Equal (band) must be in range [0..{mb}] to convert to percent.");
            }
        }
    }
}
