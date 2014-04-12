using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class CIA
{
    void runMission(Mission m)
    {
        if (m.missionType == MissionTypes.goTo)
        {
            goTo(m);
        }
    }

    bool goTo(Mission m)
    {
        
        return true;
    }
}