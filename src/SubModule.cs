﻿using HarmonyLib;

using System.ComponentModel;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace HousesCalradia
{
    public class SubModule : MBSubModuleBase
    {
        public static string Version => "1.2.9.1";

        public static string Name => typeof(SubModule).Namespace;

        public static string DisplayName => "Houses of Calradia"; // to be shown to humans in-game

        public static string HarmonyDomain => "com.zijistark.bannerlord." + Name.ToLower();

        internal static readonly Color ImportantTextColor = Color.FromUint(0x00F16D26); // orange

        private static readonly Patch[] HarmonyPatches = new Patch[]
        {
            new Patches.RomanceCampaignBehaviorPatch(),
            new Patches.KillCharacterActionPatch(),
        };

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Util.EnableLog = true; // enable various debug logging
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            if (hasLoaded)
                return;

            Util.Log.Print($"Loading {DisplayName}...");
            bool usedMcm = false;

            try
            {
                if (Settings.Instance is { } settings)
                {
                    Util.Log.Print("MCM settings instance found!");

                    // Copy current settings to master config
                    Config.CopyFromSettings(settings);

                    // Register for settings property-changed events
                    settings.PropertyChanged += Settings_OnPropertyChanged;

                    usedMcm = true;
                }
            }
            catch (System.Exception) { }

            if (!usedMcm)
                Util.Log.Print("MCM settings instance NOT found! Using defaults.");

            Util.Log.Print("\nConfiguration:");
            Util.Log.Print(Config.ToStringLines(indentSize: 4));

            Util.Log.Print("\nApplying Harmony patches...");
            var harmony = new Harmony(HarmonyDomain);

            foreach (var patch in HarmonyPatches)
            {
                patch.Apply(harmony);
                Util.Log.Print($"Applied {patch}");
            }

            Util.Log.Print($"\nLoaded {DisplayName}!\n");
            InformationManager.DisplayMessage(new InformationMessage($"Loaded {DisplayName}", ImportantTextColor));

            hasLoaded = true;
        }

        protected override void OnGameStart(Game game, IGameStarter starterObject)
        {
            base.OnGameStart(game, starterObject);

            if (game.GameType is Campaign)
            {
                var initializer = (CampaignGameStarter)starterObject;
                initializer.AddBehavior(new MarriageBehavior());
            }
        }

        protected static void Settings_OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender is Settings settings && args.PropertyName == Settings.SaveTriggered)
            {
                Util.Log.Print("Received Settings save-triggered event...\n\nNew Settings:");
                Config.CopyFromSettings(settings);
                Util.Log.Print(Config.ToStringLines(indentSize: 4));
                Util.Log.Print(string.Empty);
            }
        }

        private bool hasLoaded;
    }
}
