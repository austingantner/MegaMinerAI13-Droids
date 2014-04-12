using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Point
{
    public int X;
    public int Y;

    public Point() : this(0, 0)
    {
    }

    public Point(int X, int Y)
    {
        this.X = X;
        this.Y = Y;
    }

    public override bool Equals(object obj)
    {
        Point p = (Point)obj;
        return (X == p.X) && (Y == p.Y);
    }

    public override int GetHashCode()
    {
        return X ^ Y;
    }
}
