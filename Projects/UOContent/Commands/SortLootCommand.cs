using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Server.Accounting;
using Server.Engines.Virtues;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Commands
{
    public static class SortCommand
    {
        private const int maxStackAmount = 60000;
        private const string Text_Clear = "clear";
        private const string Text_Reagent = "reagent";
        private const string Text_Resource = "resource";
        private const string Text_Loot = "loot";
        private static Dictionary<uint, SortBags> m_sortBagsCache = new();

        public static void Configure()
        {
            CommandSystem.Register("Sort", AccessLevel.Player, new CommandEventHandler(Sort_OnCommand));
            CommandSystem.Register("SetBag", AccessLevel.Player, new CommandEventHandler(SetBag_OnCommand));
            CommandSystem.Register("Loot", AccessLevel.Player, new CommandEventHandler(Loot_OnCommand));
        }

        private static void Loot_OnCommand(CommandEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;
            if (!pm.Alive)
            {
                pm.SendMessage(MessageHues.RedErrorHue, "You must be alive to use this command.");
                return;
            }
            InitializeSortBagsAccountCache(pm);

            //Task.Run(() => {  //Seems using the Task isn't necessary. Calling directly for now.
            DoLootPickup(pm);
            //});
        }

        private static async Task DoLootPickup(PlayerMobile pm)
        {
            if (pm.LootingProcId != 0)
            {
                pm.SendMessage(MessageHues.RedErrorHue, "You are already looting.");
                return;
            }
            pm.LootingProcId = 1; //doesn't actually seem to need to be a unique value, so just use 1 for now
            var lootBag = m_sortBagsCache[pm.Serial.Value].LootBag;
            if (lootBag == null || (lootBag != pm.Backpack && lootBag.Parent != pm.Backpack)) lootBag = pm.Backpack;
            var reagentBag = m_sortBagsCache[pm.Serial.Value].ReagentBag;
            if (reagentBag == null || (reagentBag != pm.Backpack && reagentBag.Parent != pm.Backpack)) reagentBag = lootBag;
            var resourceBag = m_sortBagsCache[pm.Serial.Value].ResourceBag;
            if (resourceBag == null || (resourceBag != pm.Backpack && resourceBag.Parent != pm.Backpack)) resourceBag = lootBag;
            try
            {
                var things = new List<Item>();
                foreach (var thing in pm.GetItemsInRange(2))
                {
                    things.Add(thing);
                }
                foreach (var thing in things)
                {
                    if (thing is Corpse corpse)
                    {
                        foreach (var item in corpse.Items.ToList())
                        {
                            if (CanLootItem(pm, item, corpse))
                            {
                                if (item is BaseReagent)
                                {
                                    TryToMoveItemToBag(item, reagentBag);
                                }
                                else if (ItemIsResourceItem(item))
                                {
                                    TryToMoveItemToBag(item, resourceBag);
                                }
                                else
                                {
                                    TryToMoveItemToBag(item, lootBag);
                                }
                                pm.PublicOverheadMessage(MessageType.Regular, MessageHues.GreenNoticeHue, false, "*yoink*");
                                await Task.Delay(1000);
                            }
                        }
                    }
                    else if (thing is Item item && item.Parent == null && item.Visible)
                    {
                        if (CanLootItem(pm, item))
                        {
                            if (item is BaseReagent)
                            {
                                TryToMoveItemToBag(item, reagentBag);
                            }
                            else if (ItemIsResourceItem(item))
                            {
                                TryToMoveItemToBag(item, resourceBag);
                            }
                            else
                            {
                                TryToMoveItemToBag(item, lootBag);
                            }
                            pm.PublicOverheadMessage(MessageType.Regular, MessageHues.GreenNoticeHue, false, "*yoink*");
                            await Task.Delay(1000);
                        }
                    }
                }
            }
            finally
            {
                pm.LootingProcId = 0;
            }
        }

        private static bool CanLootItem(PlayerMobile pm, Item item, Corpse corpse = null)
        {
            if (corpse == null)
            {
                return
                    pm.CanSee(item)
                    && (item.Parent == null)
                    && item.Visible
                    && item.Movable
                    && !item.IsLockedDown
                    && !item.IsSecure
                    && !item.Deleted;
            }
            else
            {
                return
                    pm.CanSee(corpse)
                    && item.Parent == corpse
                    && item.Visible
                    && item.Movable
                    && !item.IsLockedDown
                    && !item.IsSecure
                    && !item.Deleted;
            }
        }

        private static bool CheckContainerMoves(PlayerMobile pm, Container containerToSearch, Container reagentBag, Container resourceBag, bool movedAny, bool doAll)
        {
            try
            {
                foreach (var item in containerToSearch.Items.ToList())
                {
                    if (doAll && item is Container cont)
                    {
                        if (CheckContainerMoves(pm, cont, reagentBag, resourceBag, movedAny, doAll))
                            movedAny = true;
                        continue;
                    }
                    if (reagentBag is not null && reagentBag.Serial != containerToSearch.Serial)
                    {
                        if (item is BaseReagent)
                        {
                            TryToMoveItemToBag(item, reagentBag);
                            movedAny = true;
                            continue;
                        }
                    }
                    if (resourceBag is not null && resourceBag.Serial != containerToSearch.Serial)
                    {
                        if (ItemIsResourceItem(item))
                        {
                            TryToMoveItemToBag(item, resourceBag);
                            movedAny = true;
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                pm.SendMessage(MessageHues.RedErrorHue, $"Error checking container moves: {ex.Message}");
            }
            return movedAny;
        }

        private static string GetSortBagSetKeyString(PlayerMobile pm)
        {
            return $"SortBagSet_{pm.Serial.Value}";
        }

        private static void InitializeSortBagsAccountCache(PlayerMobile pm)
        {
            Maint.RuccisCommandMaint.RunMaint();
            if (m_sortBagsCache.ContainsKey(pm.Serial.Value)) return;
            var account = pm.Account as Account;
            if (account == null) return;
            string val = account.GetTag(GetSortBagSetKeyString(pm));
            m_sortBagsCache.Add(pm.Serial.Value, new SortBags(val));
        }

        private static bool ItemIsResourceItem(Item item)
        {
            return
                (item is ICommodity && !CommodityResources.IsNonResourceCommodity(item))
                ||
                CommodityResources.IsNonCommodityResource(item);
        }

        [Usage("SetBag [reagent|resource|clear]")]
        [Description("Sets the bag to be used for sorting Reagents or Resources. Must be in your main backpack.")]
        private static void SetBag_OnCommand(CommandEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;
            InitializeSortBagsAccountCache(pm);
            var kind = e.ArgString.ToLower().Trim();
            bool validBagKind = false;
            ValidateBagKind(ref kind, ref validBagKind);
            if (!validBagKind)
            {
                pm.SendMessage(MessageHues.RedErrorHue, $"Invalid bag type '{e.ArgString}'. Use 'reagent', 'resource', or 'clear'.");
            }
            else if (kind == Text_Clear)
            {
                var acct = pm.Account as Account;
                pm.SendMessage("Cleared sorting bags.");
                m_sortBagsCache[pm.Serial.Value].ReagentBag = null;
                m_sortBagsCache[pm.Serial.Value].ResourceBag = null;
                m_sortBagsCache[pm.Serial.Value].LootBag = null;
                acct.SetTag(GetSortBagSetKeyString(pm), m_sortBagsCache[pm.Serial.Value].ToSerializeString());
            }
            else
            {
                pm.Target = new InternalTarget(kind);
                pm.SendMessage(MessageHues.YellowNoticeHue, $"Target the {kind} bag you want to use for sorting. It must be in your main backpack.");
            }
        }

        [Usage("Sort [all]")]
        [Description("Sorts the items in your backpack into pre-defined Reagent and Resources bags. Recurses through subpacks with [all] option.")]
        private static void Sort_OnCommand(CommandEventArgs e)
        {
            var pm = e.Mobile as PlayerMobile;
            if (pm == null)
                return;
            bool doAll = e.ArgString.ToLower().Trim().StartsWith("a");
            try
            {
                InitializeSortBagsAccountCache(pm);
                var reagentBag = m_sortBagsCache[pm.Serial.Value].ReagentBag;
                var resourceBag = m_sortBagsCache[pm.Serial.Value].ResourceBag;
                if (reagentBag != null || resourceBag != null)
                {
                    if (reagentBag is not null && reagentBag.Parent != pm.Backpack)
                    {
                        reagentBag = null;
                        pm.SendMessage(MessageHues.RedErrorHue, "Not sorting reagents.");
                        pm.SendMessage(MessageHues.RedErrorHue, "Reagent bag must be directly in your main backpack.");
                    }
                    if (resourceBag is not null && resourceBag.Parent != pm.Backpack)
                    {
                        resourceBag = null;
                        pm.SendMessage(MessageHues.RedErrorHue, "Not sorting resources.");
                        pm.SendMessage(MessageHues.RedErrorHue, "Resource bag must be directly in your main backpack.");
                    }
                    else
                    {
                        bool movedAny = false;
                        var mainPack = pm.Backpack;
                        movedAny = CheckContainerMoves(pm, mainPack, reagentBag, resourceBag, movedAny, doAll);
                        if (movedAny)
                        {
                            pm.SendMessage(MessageHues.GreenNoticeHue, "Sorted items into bags.");
                            pm.PlaySound(0x48);
                        }
                        else
                        {
                            pm.SendMessage(MessageHues.YellowNoticeHue, "No items to sort.");
                        }
                    }
                }
                else
                {
                    pm.SendMessage(MessageHues.RedErrorHue, "No bags set for sorting. Use [SetBag [reagent|resource] to set the bags.");
                }
            }
            catch (Exception ex)
            {
                if (pm.AccessLevel > AccessLevel.Player)
                {
                    pm.SendMessage(MessageHues.RedErrorHue, $"Error sorting items: {ex.Message}");
                }
                else
                {
                    pm.SendMessage(MessageHues.RedErrorHue, $"Error sorting items: {ex.Message}");
                    //pm.SendMessage("An error occurred while sorting items.");
                }
            }
        }

        private static void TryToMoveItemToBag(Item item, Container bag)
        {
            if (item == null || bag == null || item.Amount <= 0)
                return;

            if (!item.Stackable)
            {
                bag.DropItem(item);
                return;
            }

            int remainingAmount = item.Amount;
            List<Item> existingStacks = new List<Item>();

            // Find existing stacks of the same item type in the bag
            foreach (Item i in bag.Items)
            {
                if (i.GetType() == item.GetType() && i.Amount < maxStackAmount)
                {
                    existingStacks.Add(i);
                }
            }

            // Try merging into existing stacks first
            foreach (Item stack in existingStacks)
            {
                int spaceLeft = maxStackAmount - stack.Amount;
                if (spaceLeft > 0)
                {
                    int toMove = Math.Min(spaceLeft, remainingAmount);
                    stack.Amount += toMove;
                    remainingAmount -= toMove;

                    if (remainingAmount <= 0)
                    {
                        item.Delete(); // Remove original item if fully moved
                        return;
                    }
                }
            }

            if ((item.Amount = remainingAmount) > 0)
            {
                bag.DropItem(item);
            }
        }

        private static void ValidateBagKind(ref string kind, ref bool validBagKind)
        {
            if (kind.StartsWith("reg") || kind.StartsWith("reag"))
            {
                kind = Text_Reagent;
                validBagKind = true;
            }
            else if (kind.StartsWith("res"))
            {
                kind = Text_Resource;
                validBagKind = true;
            }
            else if (kind.StartsWith("cl"))
            {
                kind = Text_Clear;
                validBagKind = true;
            }
            else if (kind.StartsWith("lo"))
            {
                kind = Text_Loot;
                validBagKind = true;
            }
            else
            {
                validBagKind = false;
            }
        }
        public class SortBags
        {
            public SortBags(Container reagentBag, Container resourceBag, Container lootBag = null)
            {
                ReagentBag = reagentBag;
                ResourceBag = resourceBag;
                LootBag = lootBag;
            }

            public SortBags(string serializedString)
            {
                if (string.IsNullOrWhiteSpace(serializedString))
                    return;

                string[] parts = serializedString.Split('|');
                if (parts.Length == 0)
                    return;
                uint first = 0; uint second = 0; uint third = 0;
                if (uint.TryParse(parts[0], out first) && first > 0)
                {
                    try { ReagentBag = World.FindItem((Serial)first, false) as Container; }
                    catch { ReagentBag = null; }
                }
                if (parts.Length >= 2 && uint.TryParse(parts[1], out second) && second > 0)
                {
                    try { ResourceBag = World.FindItem((Serial)second, false) as Container; }
                    catch { ResourceBag = null; }
                }
                if (parts.Length >= 3 && uint.TryParse(parts[2], out third) && third > 0)
                {
                    try { LootBag = World.FindItem((Serial)third, false) as Container; }
                    catch { LootBag = null; }
                }
            }

            public Items.Container LootBag { get; set; } = null;
            public Items.Container ReagentBag { get; set; } = null;
            public Items.Container ResourceBag { get; set; } = null;

            internal string ToSerializeString()
            {
                var regBagSerial = ReagentBag != null ? ReagentBag.Serial.Value : 0;
                var resBagSerial = ResourceBag != null ? ResourceBag.Serial.Value : 0;
                var lootBagSerial = LootBag != null ? LootBag.Serial.Value : 0;
                return $"{regBagSerial}|{resBagSerial}|{lootBagSerial}";
            }
        }

        private class InternalTarget : Target
        {
            private string m_bagKind = null;

            public InternalTarget(string bagKind) : base(0, false, TargetFlags.None)
            {
                m_bagKind = bagKind;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                var pm = from as PlayerMobile;
                if (targeted == null)
                {
                    return;
                }
                var acct = pm.Account as Account;
                var bag = targeted as Container;
                try
                {
                    if (bag == null)
                    {
                        pm.SendMessage(MessageHues.RedErrorHue, "That is not a container.");
                        return;
                    }
                    if (bag.Parent != pm.Backpack)
                    {
                        pm.SendMessage(MessageHues.RedErrorHue, "The bag must be in your main backpack.");
                        return;
                    }
                    if (m_bagKind == Text_Reagent)
                    {
                        pm.SendMessage(MessageHues.GreenNoticeHue, "Reagent bag set.");
                        m_sortBagsCache[pm.Serial.Value].ReagentBag = bag;
                        acct.SetTag(GetSortBagSetKeyString(pm), m_sortBagsCache[pm.Serial.Value].ToSerializeString());
                    }
                    else if (m_bagKind == Text_Resource)
                    {
                        pm.SendMessage(MessageHues.GreenNoticeHue, "Resource bag set.");
                        m_sortBagsCache[pm.Serial.Value].ResourceBag = bag;
                        acct.SetTag(GetSortBagSetKeyString(pm), m_sortBagsCache[pm.Serial.Value].ToSerializeString());
                    }
                    else if (m_bagKind == Text_Loot)
                    {
                        pm.SendMessage(MessageHues.GreenNoticeHue, "Loot bag set.");
                        m_sortBagsCache[pm.Serial.Value].LootBag = bag;
                        acct.SetTag(GetSortBagSetKeyString(pm), m_sortBagsCache[pm.Serial.Value].ToSerializeString());
                    }
                    else
                    {
                        pm.SendMessage(MessageHues.RedErrorHue, "Invalid bag type. Use 'reagent', 'resource' or 'loot'.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    pm.SendMessage(MessageHues.RedErrorHue, $"Error setting bag: {ex.Message}");
                }
            }
        }
    }
}

namespace Server.Mobiles
{
    public partial class PlayerMobile
    {
        public int LootingProcId { get; set; }
    }
}
