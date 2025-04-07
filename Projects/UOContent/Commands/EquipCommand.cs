using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

    public Item? OneHanded { get; set; } = null;

    public Item? TwoHanded { get; set; } = null;

    public string Serialize()
    {
        return (OneHanded?.Serial.Value ?? 0) + "|" + (TwoHanded?.Serial.Value ?? 0);
    }
}

public static class EquipCommand
{
    private static readonly string LogFilePath = Path.Combine("Logs", "EquipSetWipeMaintLog.txt");

    private static Dictionary<uint, CharacterWeaponSets> m_playerWeaponsCache
            = new Dictionary<uint, CharacterWeaponSets>();

    private static bool maintHasRun = false;
    private static object maintLock = new object();

    public static void Configure()
    {
        CommandSystem.Register("Equip", AccessLevel.Player, Equip_OnCommand);
        CommandSystem.Register("SetEquip", AccessLevel.Player, SetEquip_OnCommand);
        CommandSystem.Register("UnEquip", AccessLevel.Player, UnEquip_OnCommand);
    }

    private static void DeleteInvalidEquipSetTag(AccountTag tag, Account account, string reason)
    {
        account.RemoveTag(tag.Name);
        string line = $"Deleted invalid EquipSet tag: {tag.Name}:{tag.Value} from account {account.Username} for reason: {reason}";
        WriteLogEntry(line);
    }

    [Usage("Equip (set#)")]
    [Description("Used to equip specified items from set#. Use [setequip to define sets.")]
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

    private static string GetEquipItemSetKeyString(PlayerMobile pm, int setId)
    {
        return $"EquipSet_{pm.Serial.Value}_{setId}";
    }

    private static void InitializeEquipItemAccountCache(PlayerMobile pm)
    {
        RunMaint();
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

    private static void RunMaint()
    {
        lock (maintLock)
        {
            if (maintHasRun) return;
            maintHasRun = true;
        }
        Task.Run(async () =>
        {
            await Task.Delay(1000);
            var allAccounts = Accounts.GetAccounts();
            foreach (var act in allAccounts)
            {
                try
                {
                    //Tag related properties and methods only exist on the Account class,
                    //not on the IAccount interface, so we need to cast to Account.
                    var account = act as Account;
                    var allTags = account.Tags.ToList();
                    foreach (var tag in allTags)
                    {
                        //only check tags that are EquipSet_ tags
                        if (tag.Name.StartsWith("EquipSet_"))
                        {
                            var parts = tag.Name.Split('_');
                            //if the tag doesn't have 3 parts, it's invalid
                            if (parts.Length != 3)
                            {
                                DeleteInvalidEquipSetTag(tag, account, "Tag name appears invalid.");
                                continue;
                            }
                            uint serial = 0;
                            //if the serial isn't a valid uint, it's invalid
                            if (!uint.TryParse(parts[1], out serial))
                            {
                                DeleteInvalidEquipSetTag(tag, account, "character serial is not a valid uint");
                                continue;
                            }
                            var pm = World.FindMobile((Serial)serial) as PlayerMobile;
                            //if the player isn't found or is deleted, it's invalid
                            if (pm == null || pm.Deleted)
                            {
                                DeleteInvalidEquipSetTag(tag, account, "Character does not exist");
                                continue;
                            }
                            int setId = 0;
                            //if the setId isn't a valid int between 0 and 9, it's invalid
                            if (!int.TryParse(parts[2], out setId) || setId < 0 || setId > 9)
                            {
                                DeleteInvalidEquipSetTag(tag, account, "SetId out of 0-9 range");
                                continue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        });
    }

    [Usage("SetEquip (set#)")]
    [Description("Used to save the current item(s) in hands to a set, to be equipped with [equip")]
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

    [Usage("UnEquip")]
    [Description("Used to unequip all items in hands, placing them in your backpack.")]
    private static void UnEquip_OnCommand(CommandEventArgs e)
    {
        var pm = e.Mobile as PlayerMobile;
        if (pm == null)
            return;
        UnequipHands(pm, null);
        pm.PlaySound(0x57);
        InitializeEquipItemAccountCache(pm);
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

    [Conditional("DEBUG")]
    private static void WriteLogEntry(string line)
    {
        using (var sw = new StreamWriter(LogFilePath, true))
        {
            sw.WriteLine($"{DateTime.Now.ToString("s")} :{line}");
        }
    }
}
