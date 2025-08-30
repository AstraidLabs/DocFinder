using System;

namespace DocFinder.Domain.Settings;

[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8
}

public sealed class Hotkey
{
    public HotkeyModifiers Modifiers { get; set; } = HotkeyModifiers.Control;
    public uint Key { get; set; } = 0x20; // VK_SPACE default
    public override string ToString() => $"{Modifiers}+{Key}";
}
