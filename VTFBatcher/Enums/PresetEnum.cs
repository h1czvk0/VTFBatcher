using System;

namespace VTFBatcher.Enums;

[Flags]
public enum PresetEnum
{
    None = 0,
    Nick = 1,
    Ellis = 1 << 1,
    Rochelle = 1 << 2,
    Coach = 1 << 3,
    Bill = 1 << 4,
    Louis = 1 << 5,
    Zoey = 1 << 6,
    Francis = 1 << 7,
}