namespace lemonSpire2.PlayerStateEx.RemoteFlash;

public sealed record UiFlashOptions(
    float PeakStrength = 0.7f,
    float HalfDuration = 0.1f,
    int Pulses = 2);
