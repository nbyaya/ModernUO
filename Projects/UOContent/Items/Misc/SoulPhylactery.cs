using ModernUO.Serialization;
using Server.Mobiles;
using Server.Gumps;
using System;
using Server;

namespace Server.Items;

[SerializationGenerator(0, false)]
public partial class SoulPhylactery : Item
{

    [SerializableProperty(0)]
    [CommandProperty(AccessLevel.GameMaster)]
    public Serial OwnerSerial
    {
        get { return _ownerSerial; }
        set
        {
            if (value == _ownerSerial)
                return;
            _ownerSerial = value;
            InvalidateProperties();
        }
    }
    private const int EmptyHue = 0x3a9;
    private const int FullHue = 0xABB;

    [Constructible]
    public SoulPhylactery() : base(0x99C7)
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
        SoulPhylactery phylactery = pm.Backpack?.FindItemByType<SoulPhylactery>(true, sp => sp.OwnerSerial == pm.Serial);
        if (phylactery is not null)
        {
            pm.SendGump(new ResurrectGump(pm, phylactery));
        }
    }

    public override void GetProperties(IPropertyList list)
    {
        base.GetProperties(list);
        if (OwnerSerial == (Serial)0)
        {
            list.Add("This phylactery is not bound to a soul.");
            list.Add("Double-click at a shrine to bind it to yours.");
        }
        else
        {
            list.Add("Keep in your backpack to be resurrected on death...");
            list.Add("Once");
            list.Add("(only works for its owner)");
        }
    }

    public override void OnDoubleClick(Mobile from)
    {
        base.OnDoubleClick(from);

        var pm = from as PlayerMobile;
        if (pm == null || pm.Deleted)
            return;

        if (OwnerSerial == (Serial)0)
        {
            try
            {
                bool foundShrine = false;
                foreach (var item in pm.GetItemsInRange(5))
                {
                    if (item is IAnkh)
                    {
                        foundShrine = true;
                        break;
                    }
                }
                if (!foundShrine)
                {
                    pm.SendMessage(MessageHues.RedErrorHue,
                        "You must be near a shrine to bind this phylactery to your soul.");
                    return;
                }
                if (!from.Backpack.Items.Contains(this))
                {
                    pm.SendMessage(MessageHues.RedErrorHue,
                        "You must have this phylactery in your backpack to bind it to your soul.");
                    return;
                }
                OwnerSerial = pm.Serial;
                Name = "a soul phylactery of " + pm.Name;
                Hue = FullHue;
                LootType = LootType.Blessed;
                pm.SendMessage(MessageHues.GreenNoticeHue,
                    "This phylactery is now bound to your soul. Keep it in your backpack to be resurrected if you perish.");
                InvalidateProperties();
                Effects.SendTargetParticles(pm, 0x373A, 10, 30, 5007, EffectLayer.Waist);
                pm.PlaySound(0x1F3);
            }
            catch (Exception ex)
            {
                pm.SendMessage(MessageHues.RedErrorHue,
                    "ERROR: " + ex.Message);
            }
        }
        else if (OwnerSerial == (Serial)from.Serial)
        {
            pm.SendMessage(MessageHues.BlueNoticeHue,
                "This phylactery is bound to your soul. Keep it in your backpack to be resurrected if you perish.");
        }
        else
        {
            pm.SendMessage(MessageHues.YellowNoticeHue,
                "This phylactery is bound to another soul. It will not work for you.");
        }
    }
}
