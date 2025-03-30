/****************************************
 * Original Author Unknown              *
 * Updated for MUO by Delphi            *
 * Updated again by AKAWilbur           *
 * Date: March 30, 2025                 *
 ****************************************/

namespace Server.Items
{
    public class StygianBullHides : Item
    {
        [Constructible]
        public StygianBullHides() : this( 1 )
        {
        }

        [Constructible]
        public StygianBullHides( int amount ) : base( 0x1079 )
        {
            Name = "Stygian Bull Hides";
            Stackable = true;
            Hue = 1929;
            Weight = 5.0;
            Amount = amount;
        }

        public StygianBullHides( Serial serial ) : base( serial )
        {
        }

        public override void Serialize( IGenericWriter writer )
        {
            base.Serialize( writer );

            writer.Write( 0 ); // version
        }

        public override void Deserialize( IGenericReader reader )
        {
            base.Deserialize( reader );

            int version = reader.ReadInt();
        }
    }
}
