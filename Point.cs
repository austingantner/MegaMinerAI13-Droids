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

    public override bool Equals(Object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Point p = (Point)obj;
        return (X == p.X) && (X == p.Y);
    }

    public override int GetHashCode()
    {
        return X ^ Y;
    }

    public static bool operator ==(Point p1, Point p2)
    {
        return p1.X == p2.X && p1.Y == p2.Y;
    }

    public static bool operator !=(Point p1, Point p2)
    {
        return !(p1 == p2);
    }

}
