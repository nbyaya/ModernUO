using System;
 
namespace Server.Items
{
    public class GMSash : BodySash
    {
        [Constructible]
        public GMSash() : base( 5441 )
        {
            this.Name = "Staff Vestment";
            this.Hue = 1281;
			Layer = Layer.Earrings;
			/* Attributes.NightSight = 1;
			Attributes.AttackChance = 20;
			Attributes.LowerRegCost = 100;
			Attributes.LowerManaCost = 100;
			Attributes.RegenHits = 12;
			Attributes.RegenStam = 24;
			Attributes.RegenMana = 18;
			Attributes.SpellDamage = 30;
			Attributes.CastRecovery = 6;
			Attributes.CastSpeed = 4; */
			LootType = LootType.Blessed;
        }
 
        public GMSash( Serial serial ) : base( serial )
        {
        }
 
        public override void Serialize( IGenericWriter writer )
        {
            base.Serialize( writer );
 
            writer.Write( (int) 0 ); // version
        }
 
        public override void Deserialize( IGenericReader reader )
        {
            base.Deserialize( reader );
 
            int version = reader.ReadInt();
        }
    }
}
