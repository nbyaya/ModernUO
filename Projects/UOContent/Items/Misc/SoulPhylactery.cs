using ModernUO.Serialization;
using Server.Mobiles;
using Server.Gumps;
using System;

namespace Server.Items;

[SerializationGenerator(0, false)]
public partial class SoulPhylactery : Item
{
    [Constructible]
    public SoulPhylactery() : base(0x1F1C)
    {
        Name = "a soul phylactery";
        Hue = 0xABB;
        LootType = LootType.Blessed;
        Weight = 1.0;
        
    }
    public override void GetProperties(IPropertyList list)
    {
        base.GetProperties(list);
        list.Add("Keep in your backpack to be resurrected on death...");
        list.Add("Once");
    }
    public static void CheckSoulPhylactery(PlayerMobile pm)
    {
        if (pm == null || pm.Deleted || pm.Alive)
            return;
        SoulPhylactery phylactery = pm.Backpack?.FindItemByType<SoulPhylactery>();
        if (phylactery is not null)
        {
            pm.SendGump(new ResurrectGump(pm, phylactery));
        }
    }

}
