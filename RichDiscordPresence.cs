using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace RichDiscordPresence
{
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    [BepInProcess("valheim.exe")]
    public class RichDiscordPresence : BaseUnityPlugin
    {
        const string pluginID = "shudnal.RichDiscordPresence";
        const string pluginName = "Rich Discord Presence";
        const string pluginVersion = "1.0.5";

        private Harmony harmony = new Harmony(pluginID);

        private static ConfigEntry<bool> modEnabled;
        private static ConfigEntry<bool> loggingEnabled;
        private static ConfigEntry<string> ApplicationID;
        private static ConfigEntry<string> localizationLanguage;

        private static ConfigEntry<string> msgMainMenu;
        private static ConfigEntry<string> msgSingleplayer;
        private static ConfigEntry<string> msgMultiplayer;
        private static ConfigEntry<string> msgMultiplayerAlone;
        private static ConfigEntry<string> msgRoaming;
        private static ConfigEntry<string> msgBoss;
        private static ConfigEntry<string> msgAtHome;
        private static ConfigEntry<string> msgSailing;
        private static ConfigEntry<string> msgInDungeon;
        private static ConfigEntry<string> msgTrader;

        private static ConfigEntry<string> msgBiomeMeadows;
        private static ConfigEntry<string> msgBiomeSwamp;
        private static ConfigEntry<string> msgBiomeMountain; 
        private static ConfigEntry<string> msgBiomeBlackForest;
        private static ConfigEntry<string> msgBiomePlains;
        private static ConfigEntry<string> msgBiomeAshLands;
        private static ConfigEntry<string> msgBiomeDeepNorth;
        private static ConfigEntry<string> msgBiomeOcean;
        private static ConfigEntry<string> msgBiomeMistlands;

        private static ConfigEntry<bool> showIngameTime;
        private static ConfigEntry<bool> showServerSize;
        private static ConfigEntry<bool> showServerName;
        private static ConfigEntry<bool> showGameState;
        private static ConfigEntry<int> serverMaxCapacity;
        private static ConfigEntry<string> serverName;
        private static ConfigEntry<bool> showTrader;

        private static ConfigEntry<int> safeInHomeBaseValue;

        private static ConfigEntry<string> liKeyDefault;
        private static ConfigEntry<string> liTextDefault;
        private static ConfigEntry<string> siKeyDefault;
        private static ConfigEntry<string> siTextDefault;

        private static ConfigEntry<string> liDescriptions;
        private static ConfigEntry<string> siDescriptions;

        private static readonly long started = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        private static readonly System.Random random = new System.Random();

        private static RichDiscordPresence instance;

        private static DiscordRpc.EventHandlers handlers;

        private static Heightmap.Biome currentBiome = Heightmap.Biome.None;
        private static string forceEnv = "";
        private static bool safeInHome;
        private static int playerCount = 0;
        private static Trader activeTrader;
        private static bool showingBossHud;
        private static bool onShip;

        private static readonly Dictionary<string, string> imageDescription = new Dictionary<string, string>();

        private static string details = "";
        private static string state = "";

        private void Awake()
        {
            harmony.PatchAll();

            instance = this;

            ConfigInit();

            if (ApplicationID.Value.IsNullOrWhiteSpace())
            {
                instance.Logger.LogWarning("Application ID is not set. Set and relaunch the game to load discord.");
                return;
            }

            DiscordRpc.Initialize(ApplicationID.Value, ref handlers, true, "");

            Game.isModded = true;

            UpdateImageDescription();
        }

        private void OnDestroy()
        {
            Config.Save();
            instance = null;
            harmony?.UnpatchSelf();
        }

        private void OnDisable()
        {
            if (modEnabled.Value)
            {
                LogInfo("Shutdown");
                DiscordRpc.Shutdown();
            }
        }

        public static void LogInfo(object data)
        {
            if (loggingEnabled.Value)
                instance.Logger.LogInfo(data);
        }
        
        private void ConfigInit()
        {
            Config.Bind("General", "NexusID", 2555, "Nexus mod ID for updates");

            modEnabled = Config.Bind("General", "Enabled", defaultValue: true, "Enable the mod.");
            loggingEnabled = Config.Bind("General", "Logging enabled", defaultValue: false, "Enable logging.");
            ApplicationID = Config.Bind("General", "Application ID", defaultValue: "", "The APPLICATION ID you get from discord developer portal -> Applications -> Your app -> General Information");
            localizationLanguage = Config.Bind("General", "Loсalization language", defaultValue: "", "Specify language to make bosses, locations and biomes localized to it. If empty - current game language is used");

            msgMainMenu = Config.Bind("Messages", "State - Main menu", defaultValue: "Main Menu", "Message while in menu");
            msgSingleplayer = Config.Bind("Messages", "State - Singleplayer", defaultValue: "On their own", "Semicolon separated in singleplayer messages");
            msgMultiplayer = Config.Bind("Messages", "State - Multiplayer", defaultValue: "In a Group", "Semicolon separated in multiplayer messages");
            msgMultiplayerAlone = Config.Bind("Messages", "State - Alone in multiplayer", defaultValue: "Feeling lonely", "Semicolon separated in multiplayer but alone messages"); 

            msgRoaming = Config.Bind("Messages", "Details - Roaming", defaultValue: "Roaming around;Taking a stroll;Moving around;Wandering around;Prowling around;Walking around;Sauntering;", "Semicolon separated Biome: messages");
            msgBoss = Config.Bind("Messages", "Details - Fighting boss", defaultValue: "Fighting;Brawling;In combat;Is being fought;In a fight;Struggling against;Battling;In a battle;Hell of a fight", "Semicolon separated Boss: messages");
            msgAtHome = Config.Bind("Messages", "Details - Safe at home", defaultValue: "Hanging out at home;Keeping warm safely;Working at base;Chilling at home;Warm and safe;Safe at home;Warm and cozy", "Semicolon separated while safe at home messages");
            msgSailing = Config.Bind("Messages", "Details - Sailing", defaultValue: "Sailing;Seafaring;Boating;Yachting;Boat trip", "Semicolon separated sailing messages");
            msgInDungeon = Config.Bind("Messages", "Details - Dungeon", defaultValue: "Conquering;Killing things;Wiping out;Tomb Raiding;Grave-robbing;Plundering;Looting;Pillaging", "Semicolon separated in dungeon messages");
            msgTrader = Config.Bind("Messages", "Details - Trader", defaultValue: "Trading;Making a deal;Exchanging;Doing business;Negotiating;Bargaining;Haggling", "Semicolon separated when Trader nearby messages");

            msgBiomeMeadows = Config.Bind("Messages - Roaming Biome Specific", "Meadows", defaultValue: "", "Semicolon separated Biome: messages");
            msgBiomeSwamp = Config.Bind("Messages - Roaming Biome Specific", "Swamp", defaultValue: "", "Semicolon separated Biome: messages");
            msgBiomeMountain = Config.Bind("Messages - Roaming Biome Specific", "Mountain", defaultValue: "", "Semicolon separated Biome: messages");
            msgBiomeBlackForest = Config.Bind("Messages - Roaming Biome Specific", "BlackForest", defaultValue: "", "Semicolon separated Biome: messages");
            msgBiomePlains = Config.Bind("Messages - Roaming Biome Specific", "Plains", defaultValue: "", "Semicolon separated Biome: messages");
            msgBiomeAshLands = Config.Bind("Messages - Roaming Biome Specific", "AshLands", defaultValue: "", "Semicolon separated Biome: messages");
            msgBiomeDeepNorth = Config.Bind("Messages - Roaming Biome Specific", "DeepNorth", defaultValue: "", "Semicolon separated Biome: messages");
            msgBiomeOcean = Config.Bind("Messages - Roaming Biome Specific", "Ocean", defaultValue: "", "Semicolon separated Biome: messages");
            msgBiomeMistlands = Config.Bind("Messages - Roaming Biome Specific", "Mistlands", defaultValue: "", "Semicolon separated Biome: messages");

            safeInHomeBaseValue = Config.Bind("Misc", "Base value limit to be safe at home", defaultValue: 3, "How much PlayerBase objects should be near to consider player be safe at home");

            showIngameTime = Config.Bind("State", "Show ingame time", defaultValue: true, "Show time since game start");
            showServerSize = Config.Bind("State", "Show server size", defaultValue: true, "Show server size");
            showServerName = Config.Bind("State", "Show server name", defaultValue: true, "Show server name");
            showGameState = Config.Bind("State", "Show game state", defaultValue: true, "Show game state (singleplayer, multiplayer, multiplayer alone)");
            serverName = Config.Bind("State", "Server name", defaultValue: "", "If left emtpy the ingame server name will be used");
            serverMaxCapacity = Config.Bind("State", "Server max capacity", defaultValue: 0, "If left empty default Valheim capacity will be used");
            showTrader = Config.Bind("State", "Show active Trader", defaultValue: true, "Show Trader name and trader message if any Trader is nearby");

            liKeyDefault = Config.Bind("Images - Default", "Large image key", defaultValue: "logo", "Default Large image key");
            liTextDefault = Config.Bind("Images - Default", "Large image text", defaultValue: "Valheim", "Default Large image text");
            siKeyDefault = Config.Bind("Images - Default", "Small image key", defaultValue: "", "Default Small image key");
            siTextDefault = Config.Bind("Images - Default", "Small image text", defaultValue: "", "Default Small image text");

            liDescriptions = Config.Bind("Images - Descriptions", "Large images descriptions", defaultValue: "", "Semicolon separated list of key-text Large Image descriptions where key is what's before : in details string. More at mod's page.");
            liDescriptions.SettingChanged += (sender, args) => UpdateImageDescription();
            siDescriptions = Config.Bind("Images - Descriptions", "Small images descriptions", defaultValue: "", "Semicolon separated list of key-text Small Image descriptions where key is what's after : in details string. More at mod's page.");
            siDescriptions.SettingChanged += (sender, args) => UpdateImageDescription();

            new Terminal.ConsoleCommand("richdiscordpresence", "Force Rich Presence update", args =>
            {
                if (!modEnabled.Value)
                {
                    instance.Logger.LogInfo("Mod disabled");
                    return;
                }

                SetPresence(updateState: true);
            });
        }

        private static void UpdateImageDescription()
        {
            imageDescription.Clear();

            foreach (string keytext in liDescriptions.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int pos = keytext.IndexOf("-");
                if (pos == -1 || pos == keytext.Length) continue;

                LogInfo($"Loaded image description: {keytext.Substring(0, pos).Trim()}, {keytext.Substring(pos + 1).Trim()}");

                imageDescription.Add(keytext.Substring(0, pos).Trim(), keytext.Substring(pos + 1).Trim());
            }

            foreach (string keytext in siDescriptions.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int pos = keytext.IndexOf("-");
                if (pos == -1 || pos == keytext.Length) continue;

                LogInfo($"Loaded image description: {keytext.Substring(0, pos).Trim()}, {keytext.Substring(pos + 1).Trim()}");

                imageDescription.Add(keytext.Substring(0, pos).Trim(), keytext.Substring(pos + 1).Trim());
            }
        }

        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
        private class FejdStartup_Awake_MainMenu
        {
            private static void Postfix()
            {
                if (!modEnabled.Value) return;

                LogInfo("FejdStartup.Awake");
                SetPresence(updateState: true);
            }
        }

        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.OnDestroy))]
        private class FejdStartup_OnDestroy_MainMenu
        {
            private static void Postfix()
            {
                if (!modEnabled.Value) return;

                LogInfo("FejdStartup.OnDestroy");
                SetPresence(updateState: true);
            }
        }

        [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
        private class ZNet_Awake_PlayersCount
        {
            private static void Postfix()
            {
                if (!modEnabled.Value) return;

                playerCount = ZNet.instance.GetNrOfPlayers();

                LogInfo("ZNet.Awake");
                SetPresence();
            }
        }

        [HarmonyPatch(typeof(ZNet), nameof(ZNet.UpdatePlayerList))]
        private class ZNet_UpdatePlayerList_PlayersCount
        {
            private static void Postfix()
            {
                if (!modEnabled.Value) return;

                if (!showServerSize.Value) return;

                if (ZNet.instance.GetNrOfPlayers() == playerCount) return;

                playerCount = ZNet.instance.GetNrOfPlayers();

                LogInfo("ZNet.UpdatePlayerList");
                SetPresence();
            }
        }

        [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PlayerList))]
        private class ZNet_RPC_PlayerList_PlayersCount
        {
            private static void Postfix()
            {
                if (!modEnabled.Value) return;

                if (!showServerSize.Value) return;

                if (ZNet.instance.GetNrOfPlayers() == playerCount) return;

                playerCount = ZNet.instance.GetNrOfPlayers();

                LogInfo("ZNet.RPC_PlayerList");
                SetPresence();
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateBiome))]
        private class Player_UpdateBiome_Roaming
        {
            private static void Postfix(Player __instance)
            {
                if (!modEnabled.Value) return;

                if (Player.m_localPlayer != __instance) return;

                if (currentBiome == __instance.GetCurrentBiome()) return;

                currentBiome = __instance.GetCurrentBiome();

                LogInfo("Player.UpdateBiome");
                SetPresence(updateState: true);
            }
        }

        [HarmonyPatch(typeof(Ship), nameof(Ship.OnTriggerEnter))]
        private class Ship_OnTriggerEnter_Sailing
        {
            private static void Postfix(Collider collider)
            {
                if (!modEnabled.Value) 
                    return;

                if (((Component)(object)collider).GetComponent<Player>() != Player.m_localPlayer)
                    return;

                if (onShip == (Ship.GetLocalShip() != null)) return;

                onShip = Ship.GetLocalShip() != null;

                LogInfo("Ship.OnTriggerEnter");
                SetPresence(updateState: true);
            }
        }

        [HarmonyPatch(typeof(Ship), nameof(Ship.OnTriggerExit))]
        private class Ship_OnTriggerExit_Sailing
        {
            private static void Postfix(Collider collider)
            {
                if (!modEnabled.Value)
                    return;

                if (((Component)(object)collider).GetComponent<Player>() != Player.m_localPlayer)
                    return;

                if (onShip == (Ship.GetLocalShip() != null)) return;

                onShip = Ship.GetLocalShip() != null;

                LogInfo("Ship.OnTriggerExit");
                SetPresence(updateState: true);
            }
        }

        [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.SetForceEnvironment))]
        private class EnvMan_SetForceEnvironment_SpecialEnv
        {
            private static void Postfix(string ___m_forceEnv)
            {
                if (!modEnabled.Value)
                    return;

                if (forceEnv == ___m_forceEnv)
                    return;

                forceEnv = ___m_forceEnv;

                LogInfo("EnvMan.SetForceEnvironment");
                SetPresence(updateState: true);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateEnvStatusEffects))]
        private class Player_UpdateEnvStatusEffects_Roaming
        {
            private static void Postfix(Player __instance)
            {
                if (!modEnabled.Value) return;

                if (Player.m_localPlayer != __instance) return;

                bool safeInHomeStatus = __instance.IsSafeInHome() && __instance.GetBaseValue() >= safeInHomeBaseValue.Value;

                if (safeInHome == safeInHomeStatus) 
                    return;

                safeInHome = safeInHomeStatus;

                LogInfo("Player.UpdateEnvStatusEffects");
                SetPresence(updateState: true);
            }
        }

        [HarmonyPatch(typeof(Trader), nameof(Trader.Update))]
        private class Trader_Update_Trader
        {
            private static void Postfix(Trader __instance, float ___m_byeRange)
            {
                if (!modEnabled.Value) return;

                if (!showTrader.Value) return;

                if (Player.m_localPlayer == null) return;

                float distance = Utils.DistanceXZ(Player.m_localPlayer.transform.position, __instance.transform.position);

                if (distance > ___m_byeRange && activeTrader != null)
                {
                    activeTrader = null;
                    LogInfo("Trader.Update");
                    SetPresence(updateState: true);
                }
                else if (distance <= ___m_byeRange && activeTrader == null)
                {
                    activeTrader = __instance;
                    LogInfo("Trader.Update");
                    SetPresence(updateState: true);
                }
            }
        }

        [HarmonyPatch(typeof(EnemyHud), nameof(EnemyHud.UpdateHuds))]
        private class EnemyHud_UpdateHuds_Boss
        {
            private static void Postfix(EnemyHud __instance, Player player)
            {
                if (!modEnabled.Value) return;

                if (Player.m_localPlayer != player) return;

                if (showingBossHud == __instance.ShowingBossHud()) return;

                showingBossHud = __instance.ShowingBossHud();

                LogInfo("EnemyHud.UpdateHuds");
                SetPresence(updateState: true);
            }
        }
        
        private static string GetRandomState(string state)
        {
            string[] states = state.Split(new char[] { ';' }, StringSplitOptions.None);
            return states[random.Next(states.Length)];
        }

        private static string BiomeSpecific(Heightmap.Biome biome)
        {
            switch (biome)
            {
                case Heightmap.Biome.Meadows:
                    return GetRandomState(msgBiomeMeadows.Value);
                case Heightmap.Biome.Swamp:
                    return GetRandomState(msgBiomeSwamp.Value);
                case Heightmap.Biome.Mountain:
                    return GetRandomState(msgBiomeMountain.Value);
                case Heightmap.Biome.BlackForest:
                    return GetRandomState(msgBiomeBlackForest.Value);
                case Heightmap.Biome.Plains:
                    return GetRandomState(msgBiomePlains.Value);
                case Heightmap.Biome.AshLands:
                    return GetRandomState(msgBiomeAshLands.Value);
                case Heightmap.Biome.DeepNorth:
                    return GetRandomState(msgBiomeDeepNorth.Value);
                case Heightmap.Biome.Ocean:
                    return GetRandomState(msgBiomeOcean.Value);
                case Heightmap.Biome.Mistlands:
                    return GetRandomState(msgBiomeMistlands.Value);
            }

            return "";
        }

        public static void SetPresence(bool updateState = false)
        {
            if (!modEnabled.Value)
                return;

            string liKey = "";
            string liText = "";
            string siKey = "";
            string siText = "";

            if (updateState)
            {
                if (FejdStartup.instance != null)
                {
                    details = msgMainMenu.Value;
                    state = "";
                }
                else if (Player.m_localPlayer == null)
                {
                    details = "";
                    state = "";
                }
                else
                {
                    StringBuilder detailsBuilder = new StringBuilder(256);
                    string detailsString = GetRandomState(msgRoaming.Value);
                    if (showingBossHud)
                    {
                        string bossName = EnemyHud.instance.GetActiveBoss().m_name;

                        detailsString = GetRandomState(msgBoss.Value);
                        detailsBuilder.Append(localizationLanguage.Value.IsNullOrWhiteSpace() ? bossName : Localization.instance.TranslateSingleId(bossName, localizationLanguage.Value));
                        detailsBuilder.Append(": ");
                        detailsBuilder.Append(detailsString.IsNullOrWhiteSpace() ? "$menu_combat" : detailsString);
                    }
                    else if (showTrader.Value && activeTrader != null)
                    {
                        detailsString = GetRandomState(msgTrader.Value);
                        detailsBuilder.Append(localizationLanguage.Value.IsNullOrWhiteSpace() ? activeTrader.m_name : Localization.instance.TranslateSingleId(activeTrader.m_name, localizationLanguage.Value));
                        detailsBuilder.Append(": ");
                        detailsBuilder.Append(detailsString.IsNullOrWhiteSpace() ? "$npc_haldor_random_talk2" : detailsString);
                    }
                    else if (Player.m_localPlayer != null && Player.m_localPlayer.InInterior())
                    {
                        detailsString = GetRandomState(msgInDungeon.Value);
                        string dungeonName = "$tutorial_hildirdungeon_label";

                        Location location = Location.GetZoneLocation(Player.m_localPlayer.transform.position);
                        if (location != null)
                        {
                            Teleport gateway = location.GetComponentsInChildren<Teleport>().Where(tp => !tp.m_enterText.IsNullOrWhiteSpace()).FirstOrDefault();
                            if (gateway != null)
                                dungeonName = gateway.m_enterText;
                        }

                        detailsBuilder.Append(localizationLanguage.Value.IsNullOrWhiteSpace() ? dungeonName : Localization.instance.TranslateSingleId(dungeonName, localizationLanguage.Value));

                        if (!detailsString.IsNullOrWhiteSpace())
                        {
                            detailsBuilder.Append(": ");
                            detailsBuilder.Append(detailsString);
                        }

                    }
                    else if (currentBiome != Heightmap.Biome.None)
                    {
                        string biomeSpecific = BiomeSpecific(currentBiome);
                        if (safeInHome)
                            detailsString = GetRandomState(msgAtHome.Value);
                        else if (onShip)
                            detailsString = GetRandomState(msgSailing.Value);
                        else if (!biomeSpecific.IsNullOrWhiteSpace())
                        {
                            detailsString = biomeSpecific;
                        }

                        string biomeName = "$biome_" + currentBiome.ToString().ToLower();

                        detailsBuilder.Append(localizationLanguage.Value.IsNullOrWhiteSpace() ? biomeName : Localization.instance.TranslateSingleId(biomeName, localizationLanguage.Value));

                        if (!detailsString.IsNullOrWhiteSpace())
                        {
                            detailsBuilder.Append(": ");
                            detailsBuilder.Append(detailsString);
                        }
                    }

                    details = Localization.instance.Localize(detailsBuilder.ToString());

                    StringBuilder stateBuilder = new StringBuilder(256);
                    if (ZNet.IsSinglePlayer)
                    {
                        if (showGameState.Value)
                            stateBuilder.Append(GetRandomState(msgSingleplayer.Value));
                    }
                    else
                    {
                        if (showServerName.Value)
                            stateBuilder.Append(serverName.Value.IsNullOrWhiteSpace() ? ZNet.m_ServerName : serverName.Value);

                        if (showServerName.Value && showGameState.Value)
                            stateBuilder.Append(": ");

                        if (showGameState.Value)
                            if ((ZNet.instance == null || playerCount == 1) && !msgMultiplayerAlone.Value.IsNullOrWhiteSpace())
                                stateBuilder.Append(GetRandomState(msgMultiplayerAlone.Value));
                            else
                                stateBuilder.Append(GetRandomState(msgMultiplayer.Value));
                    }

                    state = Localization.instance.Localize(stateBuilder.ToString());

                }
            }

            if (imageDescription.Count > 0)
            {
                string[] detailsList = details.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (imageDescription.ContainsKey(detailsList[0].Trim().Replace(" ", "_").ToLower()))
                {
                    liKey = detailsList[0].Trim().Replace(" ", "_").ToLower();
                    liText = imageDescription[liKey];
                }
                if (detailsList.Length > 1)
                {
                    if (imageDescription.ContainsKey(detailsList[1].Trim().Replace(" ", "_").ToLower()))
                    {
                        siKey = detailsList[1].Trim().Replace(" ", "_").ToLower();
                        siText = imageDescription[siKey];
                    }
                }
            }

            LogInfo($"Details: \"{details}\"{(!liKey.IsNullOrWhiteSpace() ? ", Large Image: " + liKey : String.Empty)}{(!liText.IsNullOrWhiteSpace() ? ", Large Text: \"" + liText + "\"": String.Empty)}{(!siKey.IsNullOrWhiteSpace() ? ", Small Image: " + siKey : String.Empty)}{(!siText.IsNullOrWhiteSpace() ? ", Small Text: \"" + siText + "\"" : String.Empty)}");

            try
            {
                if (state.IsNullOrWhiteSpace() && details.IsNullOrWhiteSpace())
                {
                    DiscordRpc.ClearPresence();
                }
                else
                {
                    DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence()
                    {
                        details = details,
                        state = state,
                        largeImageKey = liKey.IsNullOrWhiteSpace() ? liKeyDefault.Value : liKey,
                        largeImageText = liText.IsNullOrWhiteSpace() ? liTextDefault.Value : liText,
                        smallImageKey = siKey.IsNullOrWhiteSpace() ? siKeyDefault.Value : siKey,
                        smallImageText = siText.IsNullOrWhiteSpace() ? siTextDefault.Value : siText,
                        startTimestamp = showIngameTime.Value ? started : 0,
                        partySize = showServerSize.Value && !ZNet.IsSinglePlayer && ZNet.instance != null ? playerCount : 0,
                        partyMax = showServerSize.Value && !ZNet.IsSinglePlayer ? (serverMaxCapacity.Value == 0 ? ZNet.ServerPlayerLimit : serverMaxCapacity.Value) : 0,
                    };

                    DiscordRpc.UpdatePresence(presence);
                }
            }
            catch (Exception ex)
            {
                instance.Logger.LogWarning(ex);
            }
            
        }
    }
}