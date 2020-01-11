using System;
using System.Linq;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Diagnostics;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment
{
    public class AtmManager : Manager<AtmManager>
    {
        public Position[] Positions { get; } =
        {
            // ATMS

            new Position(-386.733f, 6045.953f, 31.501f),
            new Position(-283.075f, 6226.059f, 31.187f),
            new Position(-135.165f, 6365.738f, 31.101f),
            new Position(-96.999f, 6455.301f, 31.784f),
            new Position(155.4300f, 6641.991f, 31.784f),
            new Position(174.6720f, 6637.218f, 31.784f),
            new Position(1703.138f, 6426.783f, 32.730f),
            new Position(1735.114f, 6411.035f, 35.164f),
            new Position(1702.948f, 4933.593f, 42.051f),
            new Position(1967.333f, 3744.293f, 32.272f),
            new Position(1821.917f, 3683.483f, 34.244f),
            new Position(1174.532f, 2705.278f, 38.027f),
            new Position(540.0420f, 2671.007f, 42.177f),
            new Position(2564.399f, 2585.100f, 38.016f),
            new Position(2558.683f, 349.6010f, 108.050f),
            new Position(2558.051f, 389.4817f, 108.660f),
            new Position(1077.692f, -775.796f, 58.218f),
            new Position(1139.018f, -469.886f, 66.789f),
            new Position(1168.975f, -457.241f, 66.641f),
            new Position(1153.884f, -326.540f, 69.245f),
            new Position(381.2827f, 323.2518f, 103.270f),
            new Position(265.0043f, 212.1717f, 106.780f),
            new Position(285.2029f, 143.5690f, 104.970f),
            new Position(157.7698f, 233.5450f, 106.450f),
            new Position(-164.568f, 233.5066f, 94.919f),
            new Position(-1827.04f, 785.5159f, 138.020f),
            new Position(-1409.39f, -99.2603f, 52.473f),
            new Position(-1205.044f, -326.03f, 37.870f),
            new Position(-2072.41f, -316.959f, 13.345f),
            new Position(-2975.72f, 379.7737f, 14.992f),
            new Position(-2955.70f, 488.7218f, 15.486f),
            new Position(-3044.22f, 595.2429f, 7.595f),
            new Position(-3144.13f, 1127.415f, 20.868f),
            new Position(-3241.10f, 996.6881f, 12.500f),
            new Position(-3241.11f, 1009.152f, 12.877f),
            new Position(-1305.40f, -706.240f, 25.352f),
            new Position(-538.225f, -854.423f, 29.234f),
            new Position(-711.156f, -818.958f, 23.768f),
            new Position(-717.614f, -915.880f, 19.268f),
            new Position(-526.566f, -1222.90f, 18.434f),
            new Position(-256.831f, -719.646f, 33.444f),
            new Position(-203.548f, -861.588f, 30.205f),
            new Position(112.4102f, -776.162f, 31.427f),
            new Position(112.9290f, -818.710f, 31.386f),
            new Position(119.9000f, -883.826f, 31.191f),
            new Position(146.6516f, -1035.43f, 29.344f),
            new Position(-846.304f, -340.402f, 38.687f),
            new Position(-1204.35f, -324.391f, 37.877f),
            new Position(-56.1935f, -1752.53f, 29.452f),
            new Position(-261.692f, -2012.64f, 30.121f),
            new Position(-273.001f, -2025.60f, 30.197f),
            new Position(24.589f, -946.056f, 29.357f),
            new Position(-254.112f, -692.483f, 33.616f),
            new Position(-1570.197f, -546.651f, 34.955f),
            new Position(-1415.9501f, -211.976f, 46.500f),
            new Position(-1430.112f, -211.014f, 46.500f),
            new Position(33.232f, -1347.849f, 29.497f),
            new Position(129.216f, -1292.347f, 29.269f),
            new Position(287.645f, -1282.646f, 29.659f),
            new Position(289.012f, -1256.545f, 29.440f),
            new Position(295.839f, -895.640f, 29.217f),
            new Position(1686.753f, 4815.809f, 42.008f),
            new Position(-302.408f, -829.945f, 32.417f),
            new Position(5.134f, -919.949f, 29.557f),
            new Position(-1391.0083f, -590.312744f, 29.319f),
            new Position(-2294.68359f, 356.442932f, 174.6017f),

            // BANKS

            new Position(148.834f, -1041.858f, 29.374f),
            new Position(-1212.980f, -332.367f, 37.787f),
            new Position(-2961.582f, 482.627f, 15.703f),
            new Position(-112.202f, 6469.295f, 31.626f),
            new Position(313.187f, -280.621f, 54.170f),
            new Position(-351.534f, -51.529f, 49.042f)
        };

        public bool IsAtmOpened { get; set; }
        
        public override void Begin()
        {
            foreach (var position in Positions)
            {
                new BlipInfo()
                {
                    Name = "Bankomat / Bank",
                    Sprite = 431,
                    Color = 11,
                    Scale = 0.7f,
                    Position = position
                }.Commit();
            }

            Atlas.AttachNuiHandler("CLOSE_ATM", new EventCallback(metadata =>
            {
                API.SetNuiFocus(false, false);

                return null;
            }));

            Atlas.AttachNuiHandler("DEPOSIT_ATM", new EventCallback(metadata =>
            {
                var amount = 0L;

                try
                {
                    amount = long.Parse(metadata.Find<string>(0));
                }
                catch (Exception ex)
                {
                    Logger.Info($"{ex}");

                    // Silenced
                }

                var character = Cache.Character;

                if (amount > 0)
                {
                    if (character.Cash >= amount)
                    {
                        character.Cash -= amount;
                        character.BankAccount.Balance += amount;
                        character.BankAccount.History.Add(new BankTransaction
                        {
                            Type = BankTransactionType.Deposit,
                            Amount = amount,
                            Date = DateTime.Now,
                            Information = "Bankomat"
                        });
                    }
                    else
                    {
                        Cache.Player.ShowNotification($"Du har inte {amount} SEK på dig!");
                    }
                }

                API.SetNuiFocus(false, false);

                IsAtmOpened = false;

                return this;
            }));

            Atlas.AttachNuiHandler("WITHDRAW_ATM", new EventCallback(metadata =>
            {
                var amount = 0L;

                try
                {
                    amount = long.Parse(metadata.Find<string>(0));
                }
                catch (Exception)
                {
                    // Silenced
                }

                var character = Cache.Character;

                if (amount > 0)
                {
                    if (character.BankAccount.Balance >= amount)
                    {
                        character.Cash += amount;
                        character.BankAccount.Balance -= amount;
                        character.BankAccount.History.Add(new BankTransaction
                        {
                            Type = BankTransactionType.Withdraw,
                            Amount = amount,
                            Date = DateTime.Now,
                            Information = "Bankomat"
                        });
                    }
                    else
                    {
                        Cache.Player.ShowNotification($"Du har inte {amount} SEK på ditt konto!");
                    }
                }

                API.SetNuiFocus(false, false);

                IsAtmOpened = false;

                return this;
            }));
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            var entity = Cache.Entity;

            if (!IsAtmOpened)
            {
                var atm = Positions.FirstOrDefault(self => self.Distance(entity.Position) < 3);

                if (atm != null)
                {
                    API.BeginTextCommandDisplayHelp("STRING");
                    API.AddTextComponentSubstringPlayerName("Tryck ~INPUT_CONTEXT~ för att öppna bankomaten");
                    API.EndTextCommandDisplayHelp(0, false, true, -1);

                    if (Game.IsControlJustPressed(0, Control.Context))
                    {
                        IsAtmOpened = true;

                        var character = Cache.Character;

                        API.SendNuiMessage(new JsonBuilder().Add("Operation", "OPEN_ATM").Add("Cash", character.Cash)
                            .Add("Bank", character.BankAccount.Balance).Add("History",
                                character.BankAccount.History.OrderByDescending(self => self.Date.Ticks).ToList())
                            .Build());
                        API.SetNuiFocus(true, true);
                    }
                }
            }

            await Task.FromResult(0);
        }
    }
}