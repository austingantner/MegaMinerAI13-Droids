using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class CIA
{
    public static bool runMission(Mission m)
    {
        if (m.missionType == MissionTypes.goTo)
        {
            return goTo(m);
        }
        else if (m.missionType == MissionTypes.attackInRange)
        {
            return attackInRange(m);
        }
        return false;
    }

    public static bool goTo(Mission m)
    {
        IEnumerable<Point> path = Searcher.findPath(m.agent, m.target, m.isWalkable);
        foreach (Point p in path)
        {
            if (m.agent.MovementLeft <= 0)
                break;
            m.agent.move(p.X, p.Y);
        }
        return true;
    }

    public static bool attackInRange(Mission m)
    {
        Droid attacker = m.agent;
        for (int i = 0; i < 40 && attacker.AttacksLeft > 0; i++)
        {
            for (int j = 0; j < 20 && attacker.AttacksLeft > 0; j++)
            {
                if (m.target(new Point(i, j)))
                {
                    if (Math.Abs(i - attacker.X) + Math.Abs(j - attacker.Y) <= attacker.Range)
                    {
                        attacker.operate(i, j);
                    }
                }
            }
        }
        return true;
    }
}