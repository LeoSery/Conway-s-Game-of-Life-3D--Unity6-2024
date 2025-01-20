using Unity.Mathematics;

public class Cell
{
    #region Properties
    /// <summary>
    /// Gets or sets the position of the cell.
    /// </summary>
    public int3 Position { get; set; }

    /// <summary>
    /// Gets or sets the state of the cell.
    /// </summary>
    public byte State { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Cell"/> class.
    /// </summary>
    /// <param name="_position">The position of the cell.</param>
    /// <param name="_state">The state of the cell.</param>
    public Cell(int3 _position, byte _state)
    {
        Position = _position;
        State = _state;
    }
    #endregion
}

public static class CellState
{
    #region Constants
    /// <summary>
    /// Represents the dead state of a cell.
    /// </summary>
    public const byte Dead = 0;

    /// <summary>
    /// Represents the alive state of a cell.
    /// </summary>
    public const byte Alive = 1;

    /// <summary>
    /// Represents the active zone state of a cell.
    /// </summary>
    public const byte ActiveZone = 2;
    #endregion
}
