using System;

namespace _1;

public static class AudioSettings
{
    public static float MusicVolume { get; private set; } = 0.5f;
    public static float EffectsVolume { get; private set; } = 1f;

    public static void AdjustMusicVolume(float change)
    {
        MusicVolume = Math.Clamp(MusicVolume + change, 0f, 1f);
    }

    public static void AdjustEffectsVolume(float change)
    {
        EffectsVolume = Math.Clamp(EffectsVolume + change, 0f, 1f);
    }
}
