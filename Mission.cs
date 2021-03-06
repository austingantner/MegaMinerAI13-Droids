﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum MissionTypes { goTo, goToAttack, attackInRange };

class Mission
{
  
    public MissionTypes missionType;
    public Droid agent;
    public Func<Point, bool> target;
    public Func<Point, bool> isWalkable;
    public Func<Point, bool> attackAlongTheWay;

    public Mission(MissionTypes missionType, Droid agent, Func<Point, bool> target, Func<Point, bool> isWalkable, Func<Point, bool> attackAlongTheWay)
    {
        this.missionType = missionType;
        this.agent = agent;
        this.target = target;
        this.isWalkable = isWalkable;
        this.attackAlongTheWay = attackAlongTheWay;
    }
}

