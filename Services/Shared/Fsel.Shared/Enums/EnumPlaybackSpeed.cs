// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    public enum EnumPlaybackSpeed
    {
        Slow = 75,     // 0.75x
        Normal = 100,  // 1x
        Fast = 125,    // 1.25x
        Faster = 150   // 1.5x
    }

    public static class PlaybackSpeedExtensions
    {
        public static double ToDouble(this EnumPlaybackSpeed speed)
        {
            return (int)speed / 100.0;
        }
    }
}
