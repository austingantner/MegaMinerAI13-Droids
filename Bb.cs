using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

class Bb
{
    public BitArray board;
    public int width;
    public int height;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mapWidth">width of the map</param>
    /// <param name="mapHeight">height of the map</param>
    public Bb(int mapWidth, int mapHeight)
    {
        board = new BitArray(mapWidth * mapHeight);
        width = mapWidth;
        height = mapHeight;
    }

    /// <summary>
    /// Returns the value at the specified coordinate
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <returns>True if set</returns>
    public bool getValueFromSpot(int x, int y)
    {
        return board[y * width + x];
    }

    /// <summary>
    /// Sets the value at the passed point to a 1
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    public void setValueAtSpot(int x, int y)
    {
        board[y * width + x] = true;
    }

    /// <summary>
    /// Sets all spots to false
    /// </summary>
    public void clear()
    {
        board.SetAll(false);
    }

    /// <summary>
    /// Returns a bitboard with the specified unit of the specified player set to 1
    /// everything else is set to 0
    /// </summary>
    /// <param name="droids"></param>
    /// <param name="playerID"></param>
    /// <param name="droidType"></param>
    public void setAllDroidsForPlayer(Droid[] droids, int playerID, int droidType)
    {
        clear();
        for (int i = 0; i < droids.Length; i++)
        {
            if (droids[i].Owner == playerID)
            {
                setValueAtSpot(droids[i].X, droids[i].Y);
            }
        }
    }
}