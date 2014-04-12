using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Client
{
    class Bb
    {
        BitArray board;
        int width;
        int height;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapWidth">width of the map</param>
        /// <param name="mapHeight">height of the map</param>
        Bb(int mapWidth, int mapHeight)
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
        bool getValueFromSpot(int x, int y)
        {
            return board[y * width + x];
        }

        /// <summary>
        /// Sets the value at the passed point to a 1
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        void setValueAtSpot(int x, int y)
        {
            board[y * width + x] = true;
        }

        /// <summary>
        /// Sets all spots to false
        /// </summary>
        void clear()
        {
            board.SetAll(false);
        }
    }
}
