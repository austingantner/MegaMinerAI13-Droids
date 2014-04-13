using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Strat
{
    public static Dictionary<int, List<Mission>> retainedMissions = new Dictionary<int,List<Mission>>();

    public static List<Mission> AssignMissions(Droid[] droids, int playerID, BoardState boardState)
    {
        List<Mission> missions = new List<Mission>();
        foreach (Droid d in droids)
        {            
            if (retainedMissions.ContainsKey(d.Id))
            {
                //todo: mission complete
                if ((d.Owner == playerID && d.HackedTurnsLeft > 0) || (d.Owner != playerID && d.HackedTurnsLeft == 0))
                {
                    retainedMissions.Remove(d.Id);
                }
                missions.AddRange(retainedMissions[d.Id]);
                continue;
            }
            else
            {
                // make a mission
                Func<Point, bool> isWalkable = delegate(Point p)
                {
                    return boardState.walkable.getValueFromSpot(p.X, p.Y);
                };
                Func<Point, bool> isEnemyHangar = delegate(Point p)
                {
                    return boardState.theirHangers.getValueFromSpot(p.X, p.Y);
                };
                Func<Point, bool> isNotAttacked = delegate(Point p)
                {
                    return boardState.notAttackedByEnemy.getValueFromSpot(p.X, p.Y);
                };
                Func<Point, bool> isGoalHacker = delegate(Point p)
                {
                    return boardState.theirMovables.getValueFromSpot(p.X, p.Y);
                };

                for (int i = 0; i < droids.Length; i++)
                {
                    if (droids[i].MovementLeft > 0 && (droids[i].Owner == playerID || (droids[i].Owner != playerID && droids[i].HackedTurnsLeft > 0)))
                    {
                        if (!(droids[i].Variant == (int)Unit.HACKER))
                        {
                            missions.Add(new Mission(MissionTypes.goTo, droids[i], isEnemyHangar, isWalkable, true));
                        }
                        else
                        {
                            missions.Add(new Mission(MissionTypes.goTo, droids[i], isGoalHacker, isWalkable, true));
                        }
                    }
                }

                //loop through all of the droids
                for (int i = 0; i < droids.Length; i++)
                {
                    //if you have control of the droid
                    if ((droids[i].Owner == playerID && droids[i].HackedTurnsLeft <= 0) ||
                        (droids[i].Owner != playerID && droids[i].HackedTurnsLeft > 0))
                    {
                        //if there are any attacks left
                        if (droids[i].AttacksLeft > 0)
                        {
                            if (droids[i].Variant == (int)Unit.REPAIRER)
                            {
                                Bb targets = new Bb(boardState.ourHangers.width, boardState.ourHangers.height);
                                targets.board = targets.board.Or(boardState.ourHangers.board);
                                targets.board = targets.board.Or(boardState.ourMovables.board);
                                targets.board = targets.board.Or(boardState.ourImmovables.board);
                                Func<Point, bool> target = spot =>
                                {
                                    return targets.getValueFromSpot(spot.X, spot.Y);
                                };
                                Func<Point, bool> walkable = spot =>
                                {
                                    return boardState.walkable.getValueFromSpot(spot.X, spot.Y);
                                };
                                missions.Add(new Mission(MissionTypes.attackInRange, droids[i], target, walkable, true));
                            }
                            else
                            {
                                Bb targets = new Bb(boardState.ourHangers.width, boardState.ourHangers.height);
                                targets.board = targets.board.Or(boardState.theirHangers.board);
                                targets.board = targets.board.Or(boardState.theirMovables.board);
                                targets.board = targets.board.Or(boardState.theirImmovables.board);
                                Func<Point, bool> hackerTarget = spot =>
                                {
                                    return boardState.theirMovables.getValueFromSpot(spot.X, spot.Y);
                                };
                                Func<Point, bool> target = spot =>
                                {
                                    return targets.getValueFromSpot(spot.X, spot.Y);
                                };
                                Func<Point, bool> walkable = spot =>
                                {
                                    return boardState.walkable.getValueFromSpot(spot.X, spot.Y);
                                };
                                if(droids[i].Variant != (int)Unit.HACKER)
                                {
                                    missions.Add(new Mission(MissionTypes.attackInRange, droids[i], target, walkable, true));
                                }
                                else
                                {
                                    missions.Add(new Mission(MissionTypes.attackInRange, droids[i], hackerTarget, walkable, true));
                                }
                            }
                        }
                    }
                }
            }
        }
        return missions;
    }
}