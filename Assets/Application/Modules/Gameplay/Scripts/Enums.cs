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
    Wall = 2,
    Trap = 3,
    Obstacle = 4,
    Goal = 5
}

public enum BlockState
{
    // Keep it simple; expand as needed
    Active = 0,
    Inactive = 1,
    Deadly = 2
}

public enum MaskType
{
    None = 0,
    Trap = 1,
    Obstacle = 2,
    // add more as needed
}