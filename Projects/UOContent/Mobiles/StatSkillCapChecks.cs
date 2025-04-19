using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles
{
    public static class StatSkillCapChecks
    {
        public static void CheckSkillAndStatCaps(PlayerMobile m)
        {
            if (m is null) return;
            try
            {
                if (m.StatCap < PlayerStatAndSkillCapInfo.StatCap)
                {
                    int delta =  m.StatCap - PlayerStatAndSkillCapInfo.OldStatCap;
                    int newCap = PlayerStatAndSkillCapInfo.StatCap + delta;
                    m.SendMessage(MessageHues.BlueNoticeHue, $"Your stats cap is set to the old value ({m.StatCap}). It is being updated to {newCap}");
                    m.StatCap = newCap;
                }
            }
            finally { }
            try
            {
                if(m.SkillsCap < PlayerStatAndSkillCapInfo.SkillsCap)
                {
                    int delta = m.SkillsCap - PlayerStatAndSkillCapInfo.OldSkillsCap;
                    int newCap = PlayerStatAndSkillCapInfo.SkillsCap + delta;
                    m.SendMessage(MessageHues.BlueNoticeHue, $"Your skills cap is set to the old value ({m.SkillsCap}). It is being updated to {newCap}");
                    m.SkillsCap = newCap;
                }
            }
            finally { }
        }
    }
}
