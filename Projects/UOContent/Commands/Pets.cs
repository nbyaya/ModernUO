using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Commands
{
    public static class Pets
    {
        public static void Configure()
        {
            CommandSystem.Register("Pets", AccessLevel.Player, Pets_OnCommand);
        }
        [Usage("Pets"), Description("Displays your currently-active pets.")]
        public static void Pets_OnCommand(CommandEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;
            if (pm == null) return;
            var pmAllFollowers = pm.AllFollowers;
            if (pmAllFollowers is not null)
                foreach (var pet in pmAllFollowers)
                {
                    if (pet is BaseCreature bc)
                    {
                        try
                        {
                            if (bc == pm.Mount)
                            {
                                e.Mobile.SendMessage($"Mount: {bc.Name} - {bc.ControlSlots} slots {pm.X},{pm.Y},{pm.Z},{pm.Map.Name}");
                            }
                            else
                            {
                                e.Mobile.SendMessage($"Pet: {bc.Name} - {bc.ControlSlots} slots {bc.X},{bc.Y},{bc.Z},{bc.Map.Name}");
                            }
                        }
                        catch { }
                    }
                }
            //e.Mobile.SendGump(new PetsGump(e.Mobile));
        }
    }

    //internal class PetsGump : BaseGump
    //{
    //    private Mobile mobile;

    //    public PetsGump(Mobile mobile)
    //    {
    //        this.mobile = mobile;
    //    }
    //}
}
