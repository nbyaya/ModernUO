using System;
using System.Collections.Generic;
using Server.Accounting;
using Server.Mobiles;

namespace Server.Commands;

public class CharacterWeaponSets
{
    private Dictionary<int, WeaponPair> m_weaponSets = new Dictionary<int, WeaponPair>();

    public WeaponPair this[int index]
    {
        get
        {
            if (index > 9 || index < 0) throw new ArgumentOutOfRangeException("index", "Index must be between 0 and 9.");
            if (m_weaponSets.ContainsKey(index))
                return m_weaponSets[index];
            return null;
        }
        set
        {
            if (index > 9 || index < 0) throw new ArgumentOutOfRangeException("index", "Index must be between 0 and 9.");
            if (m_weaponSets.ContainsKey(index))
                m_weaponSets[index] = value;
            else
                m_weaponSets.Add(index, value);
        }
    }
}

public class WeaponPair
{
    public WeaponPair(Item? oneHanded, Item? twoHanded)
    {
        OneHanded = oneHanded;
        TwoHanded = twoHanded;
    }
    public WeaponPair(string serializedString)
    {
        if (string.IsNullOrWhiteSpace(serializedString))
            return;

        string[] parts = serializedString.Split('|');
        if (parts.Length > 2)
            throw new FormatException("Invalid input format. Expected format: \"12345|987234\".");
        uint first = 0;
        uint second = 0;
        if (uint.TryParse(parts[0], out first))
        {
            try { OneHanded = World.FindItem((Serial)first, false); }
            catch { OneHanded = null; }
        }
        if (uint.TryParse(parts[1], out second))
        {
            try { TwoHanded = World.FindItem((Serial)second, false); }
            catch { TwoHanded = null; }
        }
    }
    public string Serialize()
    {
        return (OneHanded?.Serial.Value ?? 0) + "|" + (TwoHanded?.Serial.Value ?? 0);
    }
    public Item? OneHanded { get; set; } = null;
    public Item? TwoHanded { get; set; } = null;
}

public static class EquipCommand
{
    private static Dictionary<uint, CharacterWeaponSets> m_playerWeaponsCache
        = new Dictionary<uint, CharacterWeaponSets>();

    public static void Configure()
    {
        CommandSystem.Register("Equip", AccessLevel.Player, Equip_OnCommand);
        CommandSystem.Register("SetEquip", AccessLevel.Player, SetEquip_OnCommand);
        CommandSystem.Register("UnEquip", AccessLevel.Player, UnEquip_OnCommand);
    }

    private static void InitializeEquipItemAccountCache(PlayerMobile pm)
    {
        if (m_playerWeaponsCache.ContainsKey(pm.Serial.Value)) return;
        var account = pm.Account as Account;
        if (account == null) return;
        m_playerWeaponsCache.Add(pm.Serial.Value, new CharacterWeaponSets());
        for (int x = 0; x <= 9; x++)
        {
            string val = account.GetTag(GetEquipItemSetKeyString(pm, x));
            if (string.IsNullOrWhiteSpace(val)) continue;
            m_playerWeaponsCache[pm.Serial.Value][x] = new WeaponPair(val);
        }
    }

    private static string GetEquipItemSetKeyString(PlayerMobile pm, int setId)
    {
        return $"EquipSet_{pm.Serial.Value}_{setId}";
    }

    [Usage("Equip (set#)")]
    [Description("Used to equip specified item(s) in hands.")]
    private static void Equip_OnCommand(CommandEventArgs e)
    {
        var pm = e.Mobile as PlayerMobile;
        if (pm == null)
            return;
        InitializeEquipItemAccountCache(pm);
        if (e.Length != 1)
        {
            pm.SendMessage("Usage: [Equip (0-9)");
            return;
        }
        var setId = e.GetInt32(0);
        if (setId < 0 || setId > 9)
        {
            pm.SendMessage("Usage: [Equip (0-9)");
            return;
        }
        var pmSerial = pm.Serial.Value;
        var pair = m_playerWeaponsCache[pmSerial][setId];
        if (pair == null)
        {
            pm.SendMessage($"No weapons found for set {setId}.");
            return;
        }
        UnequipHands(pm, pair);
        bool makeSound = false;
        if (pair.OneHanded != null && pair.OneHanded != pm.FindItemOnLayer(Layer.OneHanded))
        {
            if (!pm.Backpack.Items.Contains(pair.OneHanded))
            {
                pm.SendMessage($"One handed item not found in backpack.");
            }
            else
            {
                makeSound = true;
                pm.EquipItem(pair.OneHanded);
            }
        }
        if (pair.TwoHanded != null && pair.TwoHanded != pm.FindItemOnLayer(Layer.TwoHanded))
        {
            if (!pm.Backpack.Items.Contains(pair.TwoHanded))
            {
                pm.SendMessage($"Two handed item not found in backpack.");
            }
            else
            {
                makeSound = true;
                pm.EquipItem(pair.TwoHanded);
            }
        }
        if (makeSound)
            pm.PlaySound(0x48);
    }

    private static void SetEquip_OnCommand(CommandEventArgs e)
    {
        var pm = e.Mobile as PlayerMobile;
        if (pm == null)
            return;
        InitializeEquipItemAccountCache(pm);
        if (e.Length == 0)
        {
            pm.SendMessage("You must specify a set number.");
            return;
        }
        var setId = e.GetInt32(0);
        if (setId < 0 || setId > 9)
        {
            pm.SendMessage("Invalid set number.");
            return;
        }
        var weaponSets = m_playerWeaponsCache[pm.Serial.Value];
        var pair = new WeaponPair(pm.FindItemOnLayer(Layer.OneHanded), pm.FindItemOnLayer(Layer.TwoHanded));
        var acct = pm.Account as Account;
        acct.SetTag(GetEquipItemSetKeyString(pm, setId), pair.Serialize());
        weaponSets[setId] = pair;
        pm.SendMessage($"Weapon set {setId} saved.");
    }

    private static void UnEquip_OnCommand(CommandEventArgs e)
    {
        var pm = e.Mobile as PlayerMobile;
        if (pm == null)
            return;
        UnequipHands(pm, null);
        pm.PlaySound(0x57);
    }

    private static void UnequipHands(PlayerMobile pm, WeaponPair exemptions)
    {
        var oneHanded = pm.FindItemOnLayer(Layer.OneHanded);
        var twoHanded = pm.FindItemOnLayer(Layer.TwoHanded);
        if (oneHanded != null && oneHanded != exemptions?.OneHanded)
        {
            pm.Backpack.DropItem(oneHanded);
        }
        if (twoHanded != null && twoHanded != exemptions?.TwoHanded)
        {
            pm.Backpack.DropItem(twoHanded);
        }
    }
}
