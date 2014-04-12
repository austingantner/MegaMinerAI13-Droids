using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum MissionTypes { goTo, attackInRange };

class Mission
{
  
    public MissionTypes missionType;
    public Droid agent;
    public Func<Point, bool> target;

    public Mission(MissionTypes missionType, Droid agent, Func<Point, bool> target)
    {
        this.missionType = missionType;
        this.agent = agent;
        this.target = target;
    }
}

