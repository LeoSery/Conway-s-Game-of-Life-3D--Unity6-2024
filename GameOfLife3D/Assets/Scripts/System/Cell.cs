using Unity.Mathematics;

public class Cell
{
    #region Properties
    public int3 Position;
    public byte State;
    #endregion

    #region Constructors
    public Cell(int3 position, byte state)
    {
        Position = position;
        State = state;
    }
    #endregion
}

public static class CellState
{
    #region Constants
    public const byte Dead = 0;
    public const byte Alive = 1;
    public const byte ActiveZone = 2;
    #endregion
}
