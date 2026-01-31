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
}

public enum BlockState
{
    Active = 0,
    Inactive = 1,
    Deadly = 2,
    Life = 3,
}

public enum MaskType
{
    None = 0,
    Trap = 1,
    Obstacle = 2,
}