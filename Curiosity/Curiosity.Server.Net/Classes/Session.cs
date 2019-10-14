using Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using static CitizenFX.Core.Native.API;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Server.net.Classes
{
    public class Session
    {
        const string LICENSE_IDENTIFIER = "license";
        // Session data
        public string NetId { get; private set; }
        public string Name { get; private set; }
        public string[] Identities { get; private set; }
        public string License { get; private set; } = null;

        // Player data
        public int UserID { get; set; }
        public Privilege Privilege { get; set; }
        public long LocationId { get; set; }

        // Player states
        public bool HasSpawned { get; private set; }
        public bool IsLoggedIn { get { return UserID > 0; } private set { } }
        // public bool IsPlaying { get { return Character != null; } private set { } }

        // Aliases
        public int Ping => GetPlayerPing(NetId);
        public int LastMsg => GetPlayerLastMsg(NetId);
        public string EndPoint => GetPlayerEndpoint(NetId);
        public bool IsDeveloper => (Privilege == Privilege.DEVELOPER || Privilege == Privilege.PROJECTMANAGER);
        public bool IsAdmin => (Privilege == Privilege.COMMUNITYMANAGER || Privilege == Privilege.ADMINISTRATOR || Privilege == Privilege.SENIORADMIN || Privilege == Privilege.HEADADMIN || Privilege == Privilege.DEVELOPER || Privilege == Privilege.PROJECTMANAGER);
        public bool IsStaff => (Privilege == Privilege.COMMUNITYMANAGER || Privilege == Privilege.MODERATOR || Privilege == Privilege.ADMINISTRATOR || Privilege == Privilege.SENIORADMIN || Privilege == Privilege.HEADADMIN || Privilege == Privilege.DEVELOPER || Privilege == Privilege.PROJECTMANAGER);
        public void Drop(string reason) => DropPlayer(NetId, reason);

        public CitizenFX.Core.Player Player { get; private set; }
        public SemaphoreSlim Mutex { get; private set; }
        public GlobalEntity.User User { get; set; }
        public Dictionary<string, GlobalEntity.Skills> Skills = new Dictionary<string, GlobalEntity.Skills>();

        public int Wallet { get; private set; }
        public int BankAccount { get; private set; }

        public Session(CitizenFX.Core.Player player)
        {
            Mutex = new SemaphoreSlim(1, 1);
            Player = player;
            NetId = player.Handle;
            Name = player.Name;

            int numIdents = GetNumPlayerIdentifiers(NetId);
            List<string> idents = new List<string>();

            for (int i = 0; i < numIdents; i++)
            {
                idents.Add(GetPlayerIdentifier(NetId, i));
            }

            Identities = idents.ToArray();

            License = player.Identifiers[LICENSE_IDENTIFIER];

            // Set unknowns
            // Character = null;
            UserID = 0;
            Privilege = Privilege.USER;
            HasSpawned = false;
            Wallet = 0;
            BankAccount = 0;
        }

        public void SetWallet(int amount)
        {
            this.Wallet = amount;
        }

        public void SetBankAccount(int amount)
        {
            this.BankAccount = amount;
        }

        public void IncreaseWallet(int amount)
        {
            this.Wallet = this.Wallet + amount;
        }

        public void DecreaseWallet(int amount)
        {
            this.Wallet = this.Wallet - amount;
        }

        public void IncreaseBankAccount(int amount)
        {
            this.BankAccount = this.BankAccount + amount;
        }

        public void DecreaseBankAccount(int amount)
        {
            this.BankAccount = this.BankAccount - amount;
        }

        public void UpdatePrivilege(Privilege privilegeIn)
        {
            this.Privilege = privilegeIn;
        }

        public override string ToString()
        {
            return $"Player: {Name} (#{UserID}) [{NetId}] -> Privilege: {Privilege} | Number Of Skills: {Skills.Count}";
        }

        public bool Activate()
        {
            if (SessionManager.PlayerList.ContainsKey(NetId))
            {
                return false;
            }

            SessionManager.PlayerList[NetId] = this;

            return true;
        }

        public void Deactivate()
        {
            try
            {
                SessionManager.PlayerList.Remove(NetId);
            }
            catch (Exception ex)
            {
                Log.Verbose($"Deactivated session couldn't clean up -> {ex.Message}");
            }
        }

        public void Dropped(string reason)
        {
            try
            {
                SessionManager.PlayerList.Remove(NetId);
            }
            catch (Exception ex)
            {
                Log.Verbose($"Disconnecting player couldn't clean up -> {ex.Message}");
            }
        }

        public void IncreaseSkill(string skill, GlobalEntity.Skills skills, int experience)
        {
            if (!Skills.ContainsKey(skill))
            {
                Skills.Add(skill, skills);
                Skills[skill].Value = 0 + experience;
            }
            else
            {
                Skills[skill].Value = Skills[skill].Value + experience;
            }
        }

        public void DecreaseSkill(string skill, GlobalEntity.Skills skills, int experience)
        {
            if (!Skills.ContainsKey(skill))
            {
                Skills.Add(skill, skills);
                Skills[skill].Value = 0 - experience;
            }
            else
            {
                Skills[skill].Value = Skills[skill].Value - experience;
            }
        }
    }
}
