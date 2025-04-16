using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles
{
    public static class StatCapCheck
    {
        public static void CheckStatCap(PlayerMobile m)
        {
            if (m is null) return;
            try
            {
                if (m.StatCap < PlayerStatCap.StatCap)
                {
                    int delta =  m.StatCap - PlayerStatCap.OldStatCap;
                    int newCap = PlayerStatCap.StatCap + delta;
                    m.SendMessage(MessageHues.BlueNoticeHue, $"Your stat cap is set to the old value ({m.StatCap}). It is being updated to {newCap}");
                    m.StatCap = newCap;
                }
            }
            finally { }
        }
    }
}
