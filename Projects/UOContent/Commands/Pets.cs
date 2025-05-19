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
                            e.Mobile.SendMessage($"Follower: {bc.Name} - {GetDistanceAndCompassDirectionToPet(pm, bc)}");
                        }
                        catch { }
                    }
                }
            //e.Mobile.SendGump(new PetsGump(e.Mobile));
        }

        private static string GetDistanceAndCompassDirectionToPet(PlayerMobile pm, BaseCreature pet)
        {
            if (pet == null) return string.Empty;
            if (pet == pm.Mount)
            {
                return "(mounted)";
            }
            else if (pm.Map == pet.Map)
            {
                var distance = (int)(pm.GetDistanceToSqrt(pet));
                var direction = pm.GetDirectionTo(pet);
                var dirString = direction.ToString();
                if (direction == Direction.Up)
                {
                    dirString = "Northwest";
                }
                else if (direction == Direction.Right)
                {
                    dirString = "Northeast";
                }
                else if (direction == Direction.Left)
                {
                    dirString = "Southwest";
                }
                else if (direction == Direction.Down)
                {
                    dirString = "Southeast";
                }
                return $"{distance} tiles, {dirString} ({pet.Region.Name})";
            }
            else
            {
                return $"in {pet.Map.Name}";
            }
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
