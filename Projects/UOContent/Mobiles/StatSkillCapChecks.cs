using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Accounting;
using Server.Mobiles;

namespace Server
{
    /// <summary>
    /// This class handles updating per-character when we change the skill or stat caps which are
    /// normally set when the character is created. It includes an Initialize() method so that
    /// it runs when the server starts.
    /// </summary>
    public static class StatSkillCapChecks
    {
        /// <summary>
        /// This value should be incremented with each new update to the skill/stat caps.
        /// </summary>
        private const int CurrentCapVersion = 1;

        private static readonly int NoCapVersion = int.MinValue;
        private static readonly string CapVersionFilePath = Path.Combine("Saves", "CapVersionFile.txt");
        private static readonly string CapMaintLogFilePath = Path.Combine("Logs", "CapMaintLog.txt");
        private static StreamWriter _logWriter;
        private const string CapVersionAccountTag = "SkillStatCapVersion";
        public static void Initialize()
        {
            Task.Run(() =>
            {
                CheckSkillAndStatCaps();
            });
        }
        private static void CheckSkillAndStatCaps()
        {
            try
            {
                var lastCapVersion = GetLastCapVersion();
                if (CurrentCapVersion > lastCapVersion)
                {
                    using (var logWriter = new StreamWriter(CapMaintLogFilePath, true))
                    {
                        _logWriter = logWriter;
                        Log();
                        Log($"Updating skill/stat caps to version {CurrentCapVersion}");
                        Log();
                        var allAccounts = Accounts.GetAccounts();
                        foreach (var acct in allAccounts)
                        {
                            var account = acct as Account;
                            if (CurrentCapVersion > GetCapVersionFromAccount(account))
                            {
                                Log($"Checking account {account.Username}");
                                foreach(Mobile mob in account)
                                {
                                    if(mob is PlayerMobile pm)
                                    {
                                        CheckSkillAndStatCaps(pm);
                                    }
                                }
                                SetCapVersionToAccount(account);
                            }
                        }
                        //foreach (var m in World.Mobiles.Values.OfType<PlayerMobile>())
                        //{
                        //    CheckSkillAndStatCaps(m);
                        //}
                    }
                    SetLastCapVersion(CurrentCapVersion);
                }
                else
                {
                    Console.WriteLine($"Cap version already {CurrentCapVersion}/{lastCapVersion}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckSkillAndStatCaps: {ex.Message}");
            }
        }

        /// <summary>
        /// Does the wor of updating skill/stat caps for a single player character.
        /// Future updates to the caps should be handled in here, to update these values for existing characters.
        /// </summary>
        private static void CheckSkillAndStatCaps(PlayerMobile m)
        {
            Log("Checking for character " + m.Name + " with serial " + m.Serial.ToString(), 1);
            try
            {
                if (m.StatCap < PlayerStatAndSkillCapInfo.StatCap)
                {
                    int delta = m.StatCap - PlayerStatAndSkillCapInfo.OldStatCap;
                    int newCap = PlayerStatAndSkillCapInfo.StatCap + delta;
                    Log($"Updating {m.Name}'s stat cap from {m.StatCap} to {newCap}", 2);
                    m.StatCap = newCap;
                }
                else
                {
                    Log($"{m.Name}'s stat cap is {m.StatCap}", 2);
                }
            }
            finally { }
            try
            {
                if (m.SkillsCap < PlayerStatAndSkillCapInfo.SkillsCap)
                {
                    int delta = m.SkillsCap - PlayerStatAndSkillCapInfo.OldSkillsCap;
                    int newCap = PlayerStatAndSkillCapInfo.SkillsCap + delta;
                    Log($"Updating {m.Name}'s skills cap from {m.SkillsCap} to {newCap}", 2);
                    m.SkillsCap = newCap;
                }
                else
                {
                    Log($"{m.Name}'s skills cap is {m.SkillsCap}", 2);
                }
            }
            finally { }
        }
        private static int GetCapVersionFromAccount(Account account)
        {
            var val = account.GetTag(CapVersionAccountTag);
            if (val == null) return NoCapVersion;
            if(int.TryParse(val, out int actVer))
            {
                return actVer;
            }
            return NoCapVersion;
        }
        private static void SetCapVersionToAccount(Account account)
        {
            account?.SetTag(CapVersionAccountTag, CurrentCapVersion.ToString());
        }

        private static int GetLastCapVersion()
        {
            lock (CapVersionFilePath)
            {
                if (!File.Exists(CapVersionFilePath))
                {
                    return NoCapVersion;
                }
                var res = File.ReadAllText(CapVersionFilePath).Trim();
                if (int.TryParse(res, out int id))
                    return id;
                else
                    return NoCapVersion;
            }
        }
        private static void SetLastCapVersion(int id)
        {
            lock (CapVersionFilePath)
            {
                File.WriteAllText(CapVersionFilePath, id.ToString());
            }
            
        }
        private static void Log(string line = null, int indents = 0)
        {
            if (line == null)
            {
                _logWriter?.WriteLine();
            }
            else
            {
                string indentedLine = new string(' ', indents * 2) + line;
                Console.WriteLine($"[{DateTime.Now.ToString("MM/dd/yy HH:mm:ss")}]: {indentedLine}");
                _logWriter?.WriteLine($"[{DateTime.Now.ToString("MM/dd/yy HH:mm:ss")}]: {indentedLine}");
            }
        }
    }
}
