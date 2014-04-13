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
    public Bb walkable;
    public Bb notAttackedByEnemy;
    public Bb attackTargets;
    public Bb hackTargets;
    public Bb ourHalf;
    public Bb theirHalf;
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
        notAttackedByEnemy = new Bb(mapWidth, mapHeight);
        walkable = new Bb(mapWidth, mapHeight);
        attackTargets = new Bb(mapWidth, mapHeight);
        hackTargets = new Bb(mapWidth, mapHeight);
        ourHalf = new Bb(mapWidth, mapHeight);
        theirHalf = new Bb(mapWidth, mapHeight);
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (i < 20)
                {
                    ourHalf.setValueAtSpot(i, j);
                }
                else
                {
                    theirHalf.setValueAtSpot(i, j);
                }
            }
        }
        if (ourID == 1)
        {
            ourHalf.board = ourHalf.board.Not();
            theirHalf.board = theirHalf.board.Not();
        }
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
                    if (current.HealthLeft > 0)
                    {
                        if (current.Owner == ourID)
                        {
                            ourMovables.setValueAtSpot(current.X, current.Y);
                            if (current.HackedTurnsLeft > 0)
                            {
                                attackTargets.setValueAtSpot(current.X, current.Y);
                                hackTargets.setValueAtSpot(current.X, current.Y);
                            }
                        }
                        else
                        {
                            theirMovables.setValueAtSpot(current.X, current.Y);
                            if (current.HackedTurnsLeft <= 0)
                            {
                                attackTargets.setValueAtSpot(current.X, current.Y);
                                hackTargets.setValueAtSpot(current.X, current.Y);
                            }
                        }
                    }
                    break;
                case (int)Unit.TURRET:
                case (int)Unit.WALL:
                    if (current.HealthLeft > 0)
                    {
                        if (current.Owner == ourID)
                            ourImmovables.setValueAtSpot(current.X, current.Y);
                        else
                        {
                            theirImmovables.setValueAtSpot(current.X, current.Y);
                            attackTargets.setValueAtSpot(current.X, current.Y);
                        }
                    }
                    break;
                case (int)Unit.HANGAR:
                    if (current.HealthLeft > 0)
                    {
                        if (current.Owner == ourID)
                            ourHangers.setValueAtSpot(current.X, current.Y);
                        else
                        {
                            theirHangers.setValueAtSpot(current.X, current.Y);
                            attackTargets.setValueAtSpot(current.X, current.Y);
                        }
                    }
                    break;
            }
            if (current.Owner != ourID)
            {
                Queue<Point> frontier = new Queue<Point>();
                HashSet<Point> explored = new HashSet<Point>();
                frontier.Enqueue(new Point(current.X, current.Y));
                explored.Add(new Point(current.X, current.Y));
                int depth = 0;
                while (frontier.Count > 0 && depth < current.MaxMovement + current.Range)
                {
                    depth++;
                    Point point = frontier.Dequeue();

                    for (int j = -1; j < 2; j += 2)
                    {
                        Point pX = new Point(point.X + j, point.Y);
                        if (pX.X >= 0 && pX.X < mapWidth)
                        {
                            if (!explored.Contains(pX))
                            {
                                explored.Add(pX);
                                frontier.Enqueue(new Point(pX.X, pX.Y));
                                notAttackedByEnemy.setValueAtSpot(pX.X, pX.Y);
                            }
                        }

                        Point pY = new Point(point.X, point.Y + j);
                        if (pY.Y >= 0 && pY.Y < mapHeight)
                        {
                            if (!explored.Contains(pY))
                            {
                                explored.Add(pY);
                                frontier.Enqueue(new Point(pY.X, pY.Y));
                                notAttackedByEnemy.setValueAtSpot(pY.X, pY.Y);
                            }
                        }
                    }
                }
            }
        }

        notAttackedByEnemy.board.Not();
        walkable.board.Or(ourHangers.board).Or(theirHangers.board).Or(ourMovables.board).Or(theirMovables.board).Or(ourImmovables.board).Or(theirImmovables.board).Not();
    }
}
