using System;
using ModernUO.Serialization;
using Server.Engines.Craft;

namespace Server.Items
{
[Forge]
[SerializationGenerator(0, false)]
public partial class PortableForge : Item
	{
    [Constructible]
    public PortableForge() : base(0xFB1)

		{
			Name = "a Portable Forge";
			Hue = 0;
			Movable = true;
			LootType = LootType.Blessed;
			Weight = 100.0;
		}
	}	
}	
