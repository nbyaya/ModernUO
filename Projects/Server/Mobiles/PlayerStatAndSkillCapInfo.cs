using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles
{
    public static class PlayerStatAndSkillCapInfo
    {
        //All new values should point here; this should be the definitive source of this value
        public const int StatCap = 325;

        public const int OldStatCap = 225;

        //All new values should point here; this should be the definitive source of this value
        public const int SkillsCap =  10000;

        public const int OldSkillsCap = 7000; //value is scaled down by 0.1. 7000=700.0

        public const int IndividualStatCap = 150;
        public const int OldIndividualStatCap = 125;

        public const int IndividualSkillCap = 100;
        public const int OldIndividualSkillCap = 100;
    }
}
