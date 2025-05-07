using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles
{
    public partial class PlayerMobile
    {
        private const int ShoveStaminaLoss = 10;

        private bool DoShoveCheck(Mobile shoved)
        {
            if(this.Map == Map.Felucca)
            {
                this.SendMessage(MessageHues.BlueNoticeHue, "Felucca shove!");
                return true;
            }

            if (!shoved.Alive || !Alive || shoved.IsDeadBondedPet || IsDeadBondedPet)
            {
                return true;
            }
            if (shoved is BaseCreature bc && bc.GetMaster() == this)
            {
                this.SendMessage(MessageHues.BlueNoticeHue, "Your pet steps aside deftly.");
                return true;
            }

            if (shoved.Hidden && shoved.AccessLevel > AccessLevel.Player)
            {
                return true;
            }

            if (!Pushing)
            {
                Pushing = true;

                int number;

                if (AccessLevel > AccessLevel.Player)
                {
                    number = shoved.Hidden ? 1019041 : 1019040;
                    SendLocalizedMessage(number);
                }
                else
                {
                    if (Stam >= ShoveStaminaLoss)
                    {
                        //number = shoved.Hidden ? 1019043 : 1019042;
                        Stam -= ShoveStaminaLoss;
                        int huecolor = MessageHues.BlueNoticeHue;
                        if (Stam <= ShoveStaminaLoss * 2)
                        {
                            huecolor = MessageHues.RedErrorHue;
                        }
                        else if (Stam <= StamMax / 2)
                        {
                            huecolor = MessageHues.YellowNoticeHue;
                        }
                        this.SendMessage(huecolor, $"You have {Stam} stamina left.");
                        this.PublicOverheadMessage(MessageType.Emote, MessageHues.BlueNoticeHue, true, "*shove*");
                        RevealingAction();
                    }
                    else
                    {
                        return false;
                    }
                }

            }

            return true;

        }
    }
}
