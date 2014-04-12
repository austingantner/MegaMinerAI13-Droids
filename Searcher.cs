﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Node
{
    public Node(Node parent, Point point)
    {
        this.parent = parent;
        this.point = point;
    }

    public Node parent;
    public Point point;
}

class Searcher
{
    static public int mapWidth;
    static public int mapHeight;

    static public IEnumerable<Point> findPath(Droid start, Func<Point, bool> isGoal, Func<Point, bool> isWalkable)
    {
        Queue<Node> frontier = new Queue<Node>();
        List<Point> explored = new List<Point>();
        frontier.Enqueue(new Node(null, new Point(start.X, start.Y)));
        explored.Add(new Point(start.X, start.Y));
        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();

            for (int i = -1; i < 2; i += 2)
            {
                Point pX = new Point(current.point.X + i, current.point.Y);
                if(pX.X >= 0 && pX.X < mapWidth)
                {
                    if (isGoal(pX))
                    {
                        List<Point> path = new List<Point>();
                        while (current.parent != null)
                        {
                            path.Add(current.point);
                            current = current.parent;
                        }
                        path.Reverse();
                        return path;
                    }
                    if (isWalkable(pX) && !containsPoint(explored, pX))
                    {
                        explored.Add(pX);
                        frontier.Enqueue(new Node(current, pX));
                    }
                }
                
                
                Point pY = new Point(current.point.X, current.point.Y + i);
                if (pY.Y >= 0 && pY.Y < mapHeight)
                {
                    if (isGoal(pY))
                    {
                        List<Point> path = new List<Point>();
                        while (current.parent != null)
                        {
                            path.Add(current.point);
                            current = current.parent;
                        }
                        path.Reverse();
                        return path;
                    }
                    if (isWalkable(pY) && !containsPoint(explored,pY))
                    {
                        explored.Add(pY);
                        frontier.Enqueue(new Node(current, pY));
                    }
                }
            }
        }

        return new List<Point>();  // No valid path was found
    }

    public static bool containsPoint(List<Point> l, Point p)
    {
        foreach (Point i in l)
        {
            if (i.X == p.X && i.Y == p.Y)
                return true;
        }
        return false;
    }
}
