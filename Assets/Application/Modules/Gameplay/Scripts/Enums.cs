public enum BlockAnimationState
{
    Spawning, Unaffected, Affected, Animating 
}

public enum LevelType
{
    Level1, Level2
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
    NatureBridge = 9
}

public enum MaskType
{
    None = 0,
    Trap = 1,
    Obstacle = 2,
    Nature = 3,
    Lava = 4,
    // add more as needed
}