using ModernUO.Serialization;
using Server.Mobiles;
using Server.Gumps;
using System;

namespace Server.Items;

[SerializationGenerator(0, false)]
public partial class SoulPhylactery : Item
{
    private Serial m_ownerSerial = (Serial)0;
    private const int EmptyHue = 0xABB;
    private const int FullHue = 0xABB;

    [Constructible]
    public SoulPhylactery() : base(0x1F1C)
    {
        Name = "a soul phylactery";
        Hue = EmptyHue;
        Weight = 1.0;
    }

    public static void CheckSoulPhylactery(PlayerMobile pm)
    {
        if (pm == null || pm.Deleted || pm.Alive)
            return;

        //Searches for a phylactery in the player's backpack that is bound to their soul.
        SoulPhylactery phylactery = pm.Backpack?.FindItemByType<SoulPhylactery>(true, sp => sp.m_ownerSerial == pm.Serial);
        if (phylactery is not null)
        {
            pm.SendGump(new ResurrectGump(pm, phylactery));
        }
    }

    public override void GetProperties(IPropertyList list)
    {
        base.GetProperties(list);
        list.Add("Keep in your backpack to be resurrected on death...");
        list.Add("Once");

        //Update this to show appropriate info depending on whether it has been bound to a soul.

    }

    public override void OnDoubleClick(Mobile from)
    {
        base.OnDoubleClick(from);

        var pm = from as PlayerMobile;
        if (pm == null || pm.Deleted)
            return;

        if (m_ownerSerial == (Serial)0)
        {

            //Check if they are near a shrine
            throw new NotImplementedException();

            if(!from.Backpack.Items.Contains(this))
            {
                pm.SendMessage(MessageHues.RedErrorHue,
                    "You must have this phylactery in your backpack to bind it to your soul.");
                return;
            }
            m_ownerSerial = pm.Serial;
            Name = "a soul phylactery of " + pm.Name;
            Hue = FullHue;
            LootType = LootType.Blessed;
            pm.SendMessage(MessageHues.GreenSuccessHue,
                "This phylactery is now bound to your soul. Keep it in your backpack to be resurrected if you perish.");

            //Make a sound and maybe a graphic effect here?
            throw new NotImplementedException();

        }
        else if (m_ownerSerial == (Serial)from.Serial)
        {
            pm.SendMessage(MessageHues.YellowNoticeHue,
                "This phylactery is bound to your soul. Keep it in your backpack to be resurrected if you perish.");
        }
        else
        {
            pm.SendMessage(MessageHues.RedErrorHue,
                "This phylactery is bound to another soul. It will not work for you.");
        }
    }
}
