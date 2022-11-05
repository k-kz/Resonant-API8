using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using System;
using System.Numerics;

namespace Resonant
{
    internal class DebugUI : IDrawable
    {
        ConfigurationManager ConfigManager { get; }
        ConfigurationProfile Profile => ConfigManager.ActiveProfile;

        readonly ClientState ClientState;

        public DebugUI(ConfigurationManager configManager, ClientState clientState)
        {
            ConfigManager = configManager;
            ClientState = clientState;
        }

        public void Draw()
        {
            PlayerCharacter? player = ClientState.LocalPlayer;
            GameObject? target = ClientState.LocalPlayer?.TargetObject;
            if (!player || !ConfigManager.DebugUIVisible) { return; }

            ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.Always);
            if (ImGui.Begin("Resonant Debug", ref ConfigManager.Config.Debug))
            {
                ImGui.Text($"Player hitbox: {player!.HitboxRadius}");

                if (target != null)
                {
                    ImGui.Text($"== Target ==");
                    float distance = Distance(player, target);
                    ImGui.Text($"XZ Distance: {distance}");
                    ImGui.Text($"Hitbox: {target.HitboxRadius}");
                    ImGui.Text($"YalmDistance: X: {target.YalmDistanceX} Z: {target.YalmDistanceZ}");
                    ImGui.Text($"Objectkind: {target.ObjectKind}");
                    ImGui.Text($"Subkind: {target.SubKind}");
                    ImGui.Text($"Type: {target.GetType()}");

                    BattleNpc? battle = target as BattleNpc;
                    if (battle != null)
                    {
                        ImGui.Text($"Kind: {battle.BattleNpcKind}");
                        ImGui.Text($"Custom: {battle.Customize}");
                        ImGui.Text($"StatusFlags: {battle.StatusFlags}");
                        ImGui.Text($"WTB: The flag that says positionals aren't required");
                    }
                    else
                    {
                        ImGui.Text($"Not battle NPC");
                    }
                }
            }

            ImGui.End();
        }

        private static float Distance(GameObject? a, GameObject? b)
        {
            if (a == null || b == null) { return 0f; }

            float dx = b.Position.X - a.Position.X;
            float dz = b.Position.Z - a.Position.Z;
            return (float)Math.Sqrt((dx * dx) + (dz * dz));
        }
    }
}
