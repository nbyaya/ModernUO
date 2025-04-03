/****************************************
 * Original Author Unknown              *
 * Updated for MUO by Delphi            *
 * Updated again by AKAWilbur           *
 * Date: April 1, 2025                  *
 ****************************************/

using System;
using Server;
using Server.Accounting;
using Server.Collections;
using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Mobiles
{
    [CorpseName("Georgia's corpse")]
    public class Georgia : Mobile
    {
        public virtual bool IsInvulnerable { get { return true; } }
        [Constructible]
        public Georgia()
        {
            Name = "Georgia";
            Title = "the Rancher's Sister";
            Body = 0x191;
            Hue = Race.Human.RandomSkinHue();
            HairItemID = 0x203C; // LongHair
            HairHue = 1929;
            Blessed = true;

            Boots b = new Boots();
            b.Hue = 1;
            AddItem(b);

            FancyDress fd = new FancyDress();
            fd.Hue = 1172;
            AddItem(fd);

            Pitchfork pf = new Pitchfork();
            AddItem(pf);

        }

        public Georgia(Serial serial) : base(serial)
        {
        }

        public override void GetContextMenuEntries(Mobile from, ref PooledRefList<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, ref list);
            list.Add(new GeorgiaEntry(from, this));
        }

        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }

        public class GeorgiaEntry : ContextMenuEntry
        {
            private Mobile m_Mobile;
            private Mobile m_Giver;

            public GeorgiaEntry(Mobile from, Mobile giver) : base(6146, 3)
            {
                m_Mobile = from;
                m_Giver = giver;
            }

            public override void OnClick(Mobile from, IEntity target)
            {
                if (!(m_Mobile is PlayerMobile))
                    return;

                PlayerMobile mobile = (PlayerMobile)m_Mobile;

                {
                    if (!mobile.HasGump<GeorgiaGump>())
                    {
                        mobile.SendGump(new GeorgiaGump());

                    }
                }
            }
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            Mobile m = from;
            PlayerMobile mobile = m as PlayerMobile;
            Account acct = (Account)from.Account;
            bool BackpackOfReductionReceived = Convert.ToBoolean(acct.GetTag("BackpackOfReductionReceived"));

            if (mobile != null)
            {
                if (dropped is StygianBullHides)
                {
                    if (dropped.Amount != 30)
                    {
                        this.PrivateOverheadMessage(MessageType.Regular, 1153, false, "I need 30 of them!", mobile.NetState);
                        return false;
                    }

                    if (!BackpackOfReductionReceived) //added account tag check
                    {
                        dropped.Delete();
                        mobile.AddToBackpack(new BackpackOfReduction());
                        this.PrivateOverheadMessage(MessageType.Regular, 1153, false, "Thank you so much!  Here is your backpack as promised!", mobile.NetState);
                        acct.SetTag("BackpackOfReductionReceived", "true");


                    }
                    else //what to do if account has already been tagged
                    {
                        this.PrivateOverheadMessage(MessageType.Regular, 1153, false, "Thank you for bringing me even more hides!", mobile.NetState);
                        mobile.AddToBackpack(new Gold(3000));
                        dropped.Delete();
                    }
                }
                else
                {
                    this.PrivateOverheadMessage(MessageType.Regular, 1153, false, "I have no need for this item.", mobile.NetState);
                }
            }
            return false;
        }
    }
}
