using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class CIA
{
    bool runMission(Mission m)
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

    bool goTo(Mission m)
    {
        IEnumerable<Point> path = Searcher.findPath(m.agent, m.target,m.isWalkable);
        foreach (Point p in path)
        {
            if (m.agent.MovementLeft <= 0)
                break;
            m.agent.move(p.X, p.Y);
        }
        return true;
    }

    bool attackInRange(Mission m)
    {
        return true;
    }
}