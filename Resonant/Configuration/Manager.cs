using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace Resonant
{
    public class ConfigurationManager
    {
        readonly DalamudPluginInterface DalamudInterface;

        internal Configuration Config;

        internal bool ConfigUIVisible = false;
        internal bool ViewportUIVisible = false;

        internal bool DebugUIVisible => Config.Debug;

        internal ConfigurationProfile ActiveProfile {
            get { return Config.Active; }
            set { Config.Active = value; }
        }

        public ConfigurationManager(DalamudPluginInterface dalamudInterface)
        {
            DalamudInterface = dalamudInterface;
            Config = GetSavedConfig();
        }

        public Configuration GetSavedConfig() => DalamudInterface.GetPluginConfig() as Configuration ?? new Configuration();

        public void Save() => DalamudInterface.SavePluginConfig(Config);
    }
}
