﻿using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViveportLibrary
{
    public class ViveportLibrarySettings : ObservableObject
    {
        private bool useCovers = false;
        private bool importHeadsetsAsPlatforms = true;

        public bool UseCovers { get => useCovers; set => SetValue(ref useCovers, value); }
        public bool ImportHeadsetsAsPlatforms { get => importHeadsetsAsPlatforms; set => SetValue(ref importHeadsetsAsPlatforms, value); }
    }

    public class ViveportLibrarySettingsViewModel : PluginSettingsViewModel<ViveportLibrarySettings, ViveportLibrary>
    {
        public ViveportLibrarySettingsViewModel(ViveportLibrary plugin) : base(plugin, plugin.PlayniteApi)
        {
            // Load saved settings.
            Settings = LoadSavedSettings() ?? new ViveportLibrarySettings();
        }
    }
}