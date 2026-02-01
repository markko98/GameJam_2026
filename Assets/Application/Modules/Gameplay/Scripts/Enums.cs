public enum BlockAnimationState
{
    Spawning, Unaffected, Affected, Animating 
}

public enum LevelType
{
    Level1, Level2, Level3, Level4
}

public enum PlayerSide
{
    Left = 0,
    Right = 1
}

public enum BlockType
{
    Empty = 0,
    Floor = 1,
    Trap = 2,
    Obstacle = 3,
    Goal = 4,
    Start = 5,
    Floor1 = 6,
    Bridge = 7,
    ArrowDispenser = 8,
    NatureBridge = 9,
    LavaBlock = 10
}

public enum MaskType
{
    None = 0,
    Kane = 1,
    Lono = 2,
    Ku = 3,
    Kanaloa = 4,
}

public enum BlockRotation
{
    Up = 0,
    Right = 90,
    Down = 180,
    Left = 270
}