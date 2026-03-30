using System;

[Flags]
public enum TerritoryTileFlags
{
    None = 0,
    PlayerStart = 1 << 0,
    EnemyStart = 1 << 1
}
