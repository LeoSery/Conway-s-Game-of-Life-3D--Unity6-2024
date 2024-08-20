using Unity.Mathematics;

public class Cell
{
    public int3 Position;
    public byte State;

    public Cell(int3 position, byte state)
    {
        Position = position;
        State = state;
    }
}

public static class CellState
{
    public const byte Dead = 0;
    public const byte Alive = 1;
    public const byte ActiveZone = 2;
}
