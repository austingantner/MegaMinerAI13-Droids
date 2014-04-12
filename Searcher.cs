using System;
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
    public Searcher()
    {
    }

    public IEnumerable<Point> findPath(Droid start, Func<Point, bool> isGoal)
    {
        Queue<Node> frontier = new Queue<Node>();
        HashSet<Point> explored = new HashSet<Point>();
        frontier.Enqueue(new Node(null, new Point(start.X, start.Y)));
        explored.Add(new Point(start.X, start.Y));
        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();

            if (isGoal(current.point))
            {
                IList<Point> path = new List<Point>();
                while (current.parent != null)
                {
                    path.Insert(0, current.point);
                    current = current.parent;
                }
                return path;
            }

            for (int i = -1; i < 2; i += 2)
            {
                Point pX = new Point(current.point.X + i, current.point.Y);
                Point pY = new Point(current.point.X, current.point.Y + i);
                if (!explored.Contains(pX))
                {
                    explored.Add(pX);
                    frontier.Enqueue(new Node(current, pX));
                }
                if (!explored.Contains(pY))
                {
                    explored.Add(pY);
                    frontier.Enqueue(new Node(current, pY));
                }
            }
        }

        return new List<Point>();  // No valid path was found
    }
}
