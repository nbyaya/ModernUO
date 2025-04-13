using ModernUO.Serialization;
using Server.Mobiles;
using Server.Gumps;

namespace Server.Items;

[SerializationGenerator(0, false)]
public partial class SoulPhylactery : Item
{
    [Constructible]
    public SoulPhylactery() : base(0x1F1C)
	{
        Hue = 0xABB;
        Light = LightType.Circle150;
		Weight = 1.0;
    }

    public override string DefaultName => "a soul phylactery";

    public void HandlePlayerDeath(PlayerMobile m)
    {
        if (IsChildOf(m.Backpack) && !m.Alive)

        {
            m.SendGump(new ResurrectGump(m));
        }
                
        
    }
}
