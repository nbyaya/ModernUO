using Server;
using Server.Mobiles;
using ModernUO.Serialization;
using System;
using Server.Items;

namespace Server.Items
{
    public class BagOfGMPlate : Bag
    {
        private const string CrafterName = "Zyx";
        private const int Hue = 1150;
        private const int MaxHitPoints = 999999;
        private const bool PlayerConstructed = true;
        private const ArmorProtectionLevel ProtectionLevel = ArmorProtectionLevel.Invulnerability;
        private const int BaseArmorRating = 999;
        private const int DexBonus = 20;
        private const ArmorQuality Quality = ArmorQuality.Exceptional;
		private const WeaponQuality WQuality = WeaponQuality.Exceptional;
        private const int StrRequirement = 101;
        private const string ChestName = "a mysterious platemail tunic";
        private const string ArmsName = "mysterious platemail arms";
        private const string LegsName = "mysterious platemail legs";
        private const string GlovesName = "mysterious platemail gloves";
        private const string GorgetName = "mysterious platemail gorget";
        private const string ShieldName = "a mysterious kite shield";
        private const string KatanaName = "a mysterious katana";
		
		[Constructible]
        public BagOfGMPlate()
        {
			PlateChest plateChest = new PlateChest();
            plateChest.Quality = Quality;
			plateChest.MaxHitPoints = MaxHitPoints;
            plateChest.PlayerConstructed = PlayerConstructed;
            plateChest.ProtectionLevel = ProtectionLevel;
            plateChest.BaseArmorRating = BaseArmorRating;
            plateChest.Attributes.BonusDex = DexBonus;
            plateChest.StrRequirement = StrRequirement;
            plateChest.Name = ChestName;
            plateChest.Crafter = CrafterName;
            plateChest.Hue = Hue;

			PlateArms plateArms = new PlateArms();
            plateArms.Quality = Quality;
			plateArms.MaxHitPoints = MaxHitPoints;
            plateArms.PlayerConstructed = PlayerConstructed;
            plateArms.ProtectionLevel = ProtectionLevel;
            plateArms.BaseArmorRating = BaseArmorRating;
            plateArms.Attributes.BonusDex = DexBonus;
            plateArms.StrRequirement = StrRequirement;
            plateArms.Name = ArmsName;
            plateArms.Crafter = CrafterName;
            plateArms.Hue = Hue;

			PlateLegs plateLegs = new PlateLegs();
            plateLegs.Quality = Quality;
			plateLegs.MaxHitPoints = MaxHitPoints;
            plateLegs.PlayerConstructed = PlayerConstructed;
            plateLegs.ProtectionLevel = ProtectionLevel;
            plateLegs.BaseArmorRating = BaseArmorRating;
            plateLegs.Attributes.BonusDex = DexBonus;
            plateLegs.StrRequirement = StrRequirement;
            plateLegs.Name = LegsName;
            plateLegs.Crafter = CrafterName;
            plateLegs.Hue = Hue;

			PlateGloves plateGloves = new PlateGloves();
            plateGloves.Quality = Quality;
			plateGloves.MaxHitPoints = MaxHitPoints;
            plateGloves.PlayerConstructed = PlayerConstructed;
            plateGloves.ProtectionLevel = ProtectionLevel;
            plateGloves.BaseArmorRating = BaseArmorRating;
            plateGloves.Attributes.BonusDex = DexBonus;
            plateGloves.StrRequirement = StrRequirement;
            plateGloves.Name = GlovesName;
            plateGloves.Crafter = CrafterName;
            plateGloves.Hue = Hue;
            
			PlateGorget plateGorget = new PlateGorget();
            plateGorget.Quality = Quality;
			plateGorget.MaxHitPoints = MaxHitPoints;
            plateGorget.PlayerConstructed = PlayerConstructed;
            plateGorget.ProtectionLevel = ProtectionLevel;
            plateGorget.BaseArmorRating = BaseArmorRating;
            plateGorget.Attributes.BonusDex = DexBonus;
            plateGorget.StrRequirement = StrRequirement;
            plateGorget.Name = GorgetName;
            plateGorget.Crafter = CrafterName;
            plateGorget.Hue = Hue;

			DropItem(plateChest);
            DropItem(plateArms);
            DropItem(plateLegs);
            DropItem(plateGloves);
            DropItem(plateGorget);   
        }
		
		public BagOfGMPlate(Serial serial) : base(serial)
        {
        }

        public override string DefaultName => "a GM Plate Bag";

        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}

namespace Server.Items
{
    public class GMPlateChest : PlateChest
    {
        [Constructible]
        public GMPlateChest()
        {
            Name = "Mysterious Plate Chest";
            Quality = ArmorQuality.Exceptional;
            MaxHitPoints = 999999;
            HitPoints = 999999;
            PlayerConstructed = true;
            Crafter = "Zyx";
			ProtectionLevel = ArmorProtectionLevel.Invulnerability;
            BaseArmorRating = 999;  //This value might need to be adjusted
            StrRequirement = 999;
            Hue = 1150;
        }

        public GMPlateChest(Serial serial) : base(serial)
        {
        }
		

        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	
	public class GMPlateArms : PlateArms
    {
        [Constructible]
        public GMPlateArms()
        {
            Name = "Mysterious Plate Arms";
            Quality = ArmorQuality.Exceptional;
            MaxHitPoints = 999999;
            HitPoints = 999999;
            PlayerConstructed = true;
            Crafter = "Zyx";
			ProtectionLevel = ArmorProtectionLevel.Invulnerability;
            BaseArmorRating = 999;  //This value might need to be adjusted
            StrRequirement = 999;
            Hue = 1150;

        }

        public GMPlateArms(Serial serial) : base(serial)
        {
        }
		

        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	
	public class GMPlateLegs : PlateLegs
    {
        [Constructible]
        public GMPlateLegs()
        {
            Name = "Mysterious Plate Legs";
            Quality = ArmorQuality.Exceptional;
            MaxHitPoints = 999999;
            HitPoints = 999999;
            PlayerConstructed = true;
            Crafter = "Zyx";
			ProtectionLevel = ArmorProtectionLevel.Invulnerability;
            BaseArmorRating = 999;  //This value might need to be adjusted
            StrRequirement = 999;
            Hue = 1150;

        }
    
        public GMPlateLegs(Serial serial) : base(serial)
        {
        }
    
        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }
    
        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	
	public class GMPlateGorget : PlateGorget
    {
        [Constructible]
        public GMPlateGorget()
        {
            Name = "Mysterious Plate Gorget";
            Quality = ArmorQuality.Exceptional;
            MaxHitPoints = 999999;
            HitPoints = 999999;
            PlayerConstructed = true;
            Crafter = "Zyx";
			ProtectionLevel = ArmorProtectionLevel.Invulnerability;
            BaseArmorRating = 999;  //This value might need to be adjusted
            StrRequirement = 999;
            Hue = 1150;
        }
    
        public GMPlateGorget(Serial serial) : base(serial)
        {
        }
    
        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }
    
        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	
	public class GMPlateGloves : PlateGloves
    {
        [Constructible]
        public GMPlateGloves()
        {
            Name = "Mysterious Plate Gloves";
            Quality = ArmorQuality.Exceptional;
            MaxHitPoints = 999999;
            HitPoints = 999999;
            PlayerConstructed = true;
            Crafter = "Zyx";
			ProtectionLevel = ArmorProtectionLevel.Invulnerability;
            BaseArmorRating = 999; // This value might need to be adjusted
            StrRequirement = 999;
            Hue = 1150;
        }
    
        public GMPlateGloves(Serial serial) : base(serial)
        {
        }
    
        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }
    
        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	
	public class GMHelm : CloseHelm
    {
        [Constructible]
        public GMHelm()
        {
            Name = "Mysterious Close Helm";
            Quality = ArmorQuality.Exceptional;
            MaxHitPoints = 999999;
            HitPoints = 999999;
            PlayerConstructed = true;
            Crafter = "Zyx";
			ProtectionLevel = ArmorProtectionLevel.Invulnerability;
            BaseArmorRating = 999; // This value might need to be adjusted
            StrRequirement = 999;
            Hue = 1150;
        }
    
        public GMHelm(Serial serial) : base(serial)
        {
        }
    
        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }
    
        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	
	public class GMCloak : Cloak
    {
        [Constructible]
        public GMCloak()
        {
            Name = "Mysterious Cloak";
            MaxHitPoints = 999999;
            HitPoints = 999999;
            PlayerConstructed = true;
            Crafter = "Zyx";
            StrRequirement = 999;
            Hue = 1150;
        }
    
        public GMCloak(Serial serial) : base(serial)
        {
        }
    
        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }
    
        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	
	public class OwnerRobe : Robe
    {
        [Constructible]
        public OwnerRobe()
        {
            Name = "Mysterious Robe";
            MaxHitPoints = 999999;
            HitPoints = 999999;
            PlayerConstructed = true;
            Crafter = "Zyx";
            StrRequirement = 999;
            Hue = 1150;
			ItemID = 0x204F;
        }
    
        public OwnerRobe(Serial serial) : base(serial)
        {
        }
    
        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }
    
        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
	
	public class OwnerStaff : GlacialStaff
    {
        [Constructible]
        public OwnerStaff()
        {
            Name = "Mysterious Staff";
            PlayerConstructed = true;
            StrRequirement = 999;
            Hue = 1152;
        }
    
        public OwnerStaff(Serial serial) : base(serial)
        {
        }
    
        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }
    
        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
