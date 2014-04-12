using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class BoardState
{
    public Bb ourHangers;
    public Bb theirHangers;
    public Bb ourMovables;
    public Bb theirMovables;
    public Bb ourImmovables;
    public Bb theirImmovables;
    public int mapWidth;
    public int mapHeight;
    public int ourID;

    /// <summary>
    /// Creates a board state for the current turn
    /// </summary>
    /// <param name="droids"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="playerID"></param>
    public BoardState(Droid[] droids, int width, int height, int playerID)
    {
        mapWidth = width;
        mapHeight = height;
        ourID = playerID;
        update(droids);
    }

    /// <summary>
    /// Update the position of all droids
    /// </summary>
    /// <param name="droids"></param>
    public void update(Droid[] droids)
    {
        ourHangers = new Bb(mapWidth, mapHeight);
        theirHangers = new Bb(mapWidth, mapHeight);
        ourMovables = new Bb(mapWidth, mapHeight);
        theirMovables = new Bb(mapWidth, mapHeight);
        ourImmovables = new Bb(mapWidth, mapHeight);
        theirImmovables = new Bb(mapWidth, mapHeight);
        for (int i = 0; i < droids.Length; i++)
        {
            Droid current = droids[i];
            switch (current.Variant)
            {
                case (int)Unit.CLAW:
                case (int)Unit.ARCHER:
                case (int)Unit.REPAIRER:
                case (int)Unit.HACKER:
                case (int)Unit.TERMINATOR:
                    if (current.Owner == ourID)
                        ourMovables.setValueAtSpot(current.X, current.Y);
                    else
                        theirMovables.setValueAtSpot(current.X, current.Y);
                    break;
                case (int)Unit.TURRET:
                case (int)Unit.WALL:
                    if (current.Owner == ourID)
                        ourImmovables.setValueAtSpot(current.X, current.Y);
                    else
                        theirImmovables.setValueAtSpot(current.X, current.Y);
                    break;
                case (int)Unit.HANGAR:
                    if (current.Owner == ourID)
                        ourHangers.setValueAtSpot(current.X, current.Y);
                    else
                        theirHangers.setValueAtSpot(current.X, current.Y);
                    break;
            }
        }
    }
}
