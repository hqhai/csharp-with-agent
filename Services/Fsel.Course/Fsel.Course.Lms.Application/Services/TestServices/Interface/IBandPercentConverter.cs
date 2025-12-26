// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices.Interface
{
    using System;

    public interface IBandPercentConverter
    {
        double BandToPercent(double band);

        double PercentToBand(double percent);
    }

    public sealed class LinearBandPercentConverter : IBandPercentConverter
    {
        private readonly double _maxBand;

        public LinearBandPercentConverter(double maxBand = 9d)
        {
            if (maxBand <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBand));
            _maxBand = maxBand;
        }

        public double BandToPercent(double band)
            => Clamp((band / _maxBand) * 100d, 0d, 100d);

        public double PercentToBand(double percent)
            => Clamp((percent / 100d) * _maxBand, 0d, _maxBand);

        private static double Clamp(double v, double min, double max)
            => v < min ? min : (v > max ? max : v);
    }
}
