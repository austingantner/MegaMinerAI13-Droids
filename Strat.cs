using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Strat
{
    public static Dictionary<int, List<Mission>> retainedMissions = new Dictionary<int,List<Mission>>();

    public static List<Mission> AssignMissions(Droid[] droids)
    {
        List<Mission> missions = new List<Mission>();
        foreach (Droid d in droids)
        {
            if (retainedMissions.ContainsKey(d.Id))
            {
                //todo: mission complete
                missions.AddRange(retainedMissions[d.Id]);
            }
            
        }
        return missions;
    }
}