using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum MissionTypes { goTo, attack };

class Mission
{
  
    public MissionTypes missionType;
    public Droid agent;
    public Bb target;

}

