using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class CIA
{
    public static void runMissions(List<Mission> missions)
    {
        foreach (Mission m in missions)
        {
            runMission(m);
        }
    }

    public static bool runMission(Mission m)
    {
        switch (m.missionType)
        {
            case MissionTypes.goTo:
                return goTo(m);
            case MissionTypes.attackInRange:
                return attackInRange(m);
            case MissionTypes.goToAttack:
                return goToAttack(m);
            default:
                return false;
        };
    }

    public static bool goTo(Mission m)
    {
        if (m.attackAlongTheWay)
        {
            attackInRange(m);//todo: targets are wrong here
            attackInRange(m);
        }
        IEnumerable<Point> path = Searcher.findPath(m.agent, m.target, m.isWalkable);
        bool pathExists = false;
        foreach (Point p in path)
        {
            pathExists = true;
            if (m.agent.MovementLeft <= 0)
                break;
            m.agent.move(p.X, p.Y);
            if (m.attackAlongTheWay)
            {
                attackInRange(m);//todo: targets are wrong here
                attackInRange(m);
            }
        }
        return pathExists;
    }

    //goes to closest target and attacks
    //stops moving on successful attack
    public static bool goToAttack(Mission m)
    {
        IEnumerable<Point> path = Searcher.findPath(m.agent, m.target, m.isWalkable);
        foreach (Point p in path)
        {
            if (attackInRange(m))
            {
                attackInRange(m);//for second attack
                return true;
            }
            if (m.agent.MovementLeft <= 0)
                break;
            m.agent.move(p.X, p.Y);
            if (m.attackAlongTheWay)
            {
                attackInRange(m);//todo: targets are wrong here
                attackInRange(m);
            }
        }
        return true;
    }

    public static bool attackInRange(Mission m)
    {
        if (m.agent.AttacksLeft <= 0)
        {
            return false;
        }
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
                        return true;
                    }
                }
            }
        }
        return false;
    }
}