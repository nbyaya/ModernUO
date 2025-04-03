/****************************************
 * Original Author Unknown              *
 * Updated for MUO by Delphi            *
 * Updated again by AKAWilbur           *
 * Date: April 1, 2025                  *
 ****************************************/

using Server.Network;

namespace Server.Gumps
{
    public class GeorgeGump : Gump
    {
        public static void Initialize()
        {
            CommandSystem.Register("GeorgeGump", AccessLevel.GameMaster, GeorgeGump_OnCommand);
        }

        private static void GeorgeGump_OnCommand( CommandEventArgs e )
        {
            e.Mobile.SendGump( new GeorgeGump() );
        }

        public GeorgeGump() : base( 50,50 )
        {
            //----------------------------------------------------------------------------------------------------

            AddPage( 0 );
            AddImageTiled(  54, 33, 369, 400, 2624 );
            AddAlphaRegion( 54, 33, 369, 400 );

            AddImageTiled( 416, 39, 44, 389, 203 );
            //--------------------------------------Window size bar--------------------------------------------

            AddImage( 97, 49, 9005 );
            AddImageTiled( 58, 39, 29, 390, 10460 );
            AddImageTiled( 412, 37, 31, 389, 10460 );
            AddLabel( 140, 60, 0x34, "Message" );


            AddHtml( 107, 140, 300, 230, "<BODY>" +
                                         //----------------------/----------------------------------------------/
                                         "<BASEFONT COLOR=YELLOW>Greetings and salutations young adventurer.<BR><BR>I have need of a strong back such as yours for a task.<BR><BR>" +
                                         "<BASEFONT COLOR=YELLOW>For you see, I am not but a poor rancher.  However, I have been able to breed a special type of stygian bull that has a magical hide.<BR><BR>Unfortunately, when trying to ship them from Skara Brae, the ship carrying my magical bulls quickly wrecked and my bulls got loose and now roam on an island outside of Skara Brae.<BR>" +
                                         "<BASEFONT COLOR=YELLOW><BR>I would really like to get some more of those magical hides.  If you could find it in yourself to acquire 20 pieces of stygian hides from these bulls, I will gladly craft you a bag of great value. I can only give you one bag, so guard it well.<BR>" +
                                         "<BASEFONT COLOR=YELLOW><BR>Be careful, for they can be quite a handful!  You'll have to carve them up to get the special hides, but I'm sure you're up to it!<BR><BR>Return to me once you have acquired 20 pieces of the stygian bull hides!" +
                                         "</BODY>", false, true);

            //			<BASEFONT COLOR=#7B6D20>

            AddImage( 430, 9, 10441);
            AddImageTiled( 40, 38, 17, 391, 9263 );
            AddImage( 6, 25, 10421 );
            AddImage( 34, 12, 10420 );
            AddImageTiled( 94, 25, 342, 15, 10304 );
            AddImageTiled( 40, 427, 415, 16, 10304 );
            AddImage( -10, 314, 10402 );
            AddImage( 56, 150, 10411 );
            AddImage( 155, 120, 2103 );
            AddImage( 136, 84, 96 );

            AddButton( 225, 390, 0xF7, 0xF8, 0 );

            //--------------------------------------------------------------------------------------------------------------
        }

        public override void OnResponse( NetState state, in RelayInfo info ) //Function for GumpButtonType.Reply Buttons
        {
            Mobile from = state.Mobile;

            switch ( info.ButtonID )
            {
                case 0: //Case uses the ActionIDs defenied above. Case 0 defenies the actions for the button with the action id 0
                    {
                        //Cancel
                        from.SendMessage( "Bring me some of my stygian hides!" );
                        break;
                    }

            }
        }
    }
}
