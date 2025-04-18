using System.Collections.Generic;

namespace Server;

public static class SkillGainRatios
{
    static SkillGainRatios()
    {
        ratios.Add(SkillName.AnimalTaming, 3.0);
        ratios.Add(SkillName.Stealing, 3.0);
        ratios.Add(SkillName.Lockpicking, 3.0);
        ratios.Add(SkillName.Meditation, 0.5);
        ratios.Add(SkillName.Focus, 0.5);
    }
    public static double GetRatioForSkill(SkillName skill)
    {
        if (ratios.ContainsKey(skill))
        {
            return ratios[skill];
        }
        return 1.0;
    }
    private static readonly Dictionary<SkillName, double> ratios = new Dictionary<SkillName, double>();
}
