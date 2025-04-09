using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private static Dictionary<uint, SortBags> m_sortBagsCache = new();

        public static void Configure()
        {
            CommandSystem.Register("Sort", AccessLevel.Player, new CommandEventHandler(Sort_OnCommand));
            CommandSystem.Register("SetSortBag", AccessLevel.Player, new CommandEventHandler(SetBag_OnCommand));
        }

        private static bool CheckContainerMoves(PlayerMobile pm, Container containerToSearch, Container reagentBag, Container resourceBag, bool movedAny, bool doAll)
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
            return item is ICommodity && item is not BaseReagent;
        }

        [Usage("SetBag [reagent|resource]")]
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
                pm.SendMessage($"Invalid bag type '{e.ArgString}'. Use 'reagent' or 'resource'.");
            }
            else
            {
                pm.Target = new InternalTarget(kind);
                pm.SendMessage($"Target the {kind} bag you want to use for sorting. It must be in your main backpack.");
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
                    if (reagentBag.Parent != pm.Backpack)
                    {
                        reagentBag = null;
                        pm.SendMessage("Not sorting reagents. Reagent bag must be directly in your main backpack.");
                    }
                    if (resourceBag.Parent != pm.Backpack)
                    {
                        resourceBag = null;
                        pm.SendMessage("Not sorting resources. Resource bag must be directly in your main backpack.");
                    }
                    else
                    {
                        bool movedAny = false;
                        var mainPack = pm.Backpack;
                        movedAny = CheckContainerMoves(pm, mainPack, reagentBag, resourceBag, movedAny, doAll);
                        if (movedAny)
                        {
                            pm.SendMessage("Sorted items into bags.");
                            pm.PlaySound(0x48);
                        }
                        else
                        {
                            pm.SendMessage("No items to sort.");
                        }
                    }
                }
                else
                {
                    pm.SendMessage("No bags set for sorting. Use [SetSortBag [reagent|resource] to set the bags.");
                }
            }
            catch (Exception ex)
            {
                if (pm.AccessLevel > AccessLevel.Player)
                {
                    pm.SendMessage($"Error sorting items: {ex.Message}");
                }
                else
                {
                    pm.SendMessage("An error occurred while sorting items.");
                }
            }
        }

        private static void TryToMoveItemToBag(Item item, Container bag)
        {
            if (item == null || bag == null || item.Amount <= 0)
                return;

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
                kind = "reagent";
                validBagKind = true;
            }
            else if (kind.StartsWith("res"))
            {
                kind = "resource";
                validBagKind = true;
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
                        pm.SendMessage("That is not a container.");
                        return;
                    }
                    if (bag.Parent != pm.Backpack)
                    {
                        pm.SendMessage("The bag must be in your main backpack.");
                        return;
                    }
                    if (m_bagKind == "reagent")
                    {
                        pm.SendMessage("Reagent bag set.");
                        m_sortBagsCache[pm.Serial.Value].ReagentBag = bag;
                        acct.SetTag(GetSortBagSetKeyString(pm), m_sortBagsCache[pm.Serial.Value].ToSerializeString());
                    }
                    else if (m_bagKind == "resource")
                    {
                        pm.SendMessage("Resource bag set.");
                        m_sortBagsCache[pm.Serial.Value].ResourceBag = bag;
                        acct.SetTag(GetSortBagSetKeyString(pm), m_sortBagsCache[pm.Serial.Value].ToSerializeString());
                    }
                    else
                    {
                        pm.SendMessage("Invalid bag type. Use 'reagent' or 'resource'.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (pm.AccessLevel > AccessLevel.Player)
                    {
                        pm.SendMessage($"Error setting bag: {ex.Message}");
                    }
                    else
                    {
                        pm.SendMessage("An error occurred while setting the bag.");
                    }
                }
            }
        }
    }
}
