using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Logging;
using ImGuiNET;

namespace Resonant
{
    internal class ResonantCore : IDrawable
    {
        private const float RangeAutoAttack = 2.1f;
        private const float RangeAbilityMelee = 3f;

        private readonly ConfigurationManager ConfigManager;
        private readonly ClientState ClientState;
        private readonly GameGui Gui;
        private readonly Canvas Canvas;
        private readonly GameStateObserver GameStateObserver;

        private ConfigurationProfile Profile => ConfigManager.Config.Active;

        public ResonantCore(ConfigurationManager configManager, ClientState clientState, GameGui gui, DataManager dataManager)
        {
            ConfigManager = configManager;
            ClientState = clientState;
            Gui = gui;
            Canvas = new Canvas(ConfigManager.Config, Gui);
            GameStateObserver = new(clientState, dataManager);

            Initialize();
        }

        internal void Initialize() => GameStateObserver.JobChangedEvent += OnJobChange;

        public void Draw()
        {
            GameStateObserver.Observe();

            PlayerCharacter? player = ClientState.LocalPlayer;
            if (player == null)
            {
                return;
            }

            Canvas.Begin();

            if (Profile.PlayerRing.Enabled)
            {
                DrawPlayerRing(player);
            }

            if (Profile.Cone.Enabled)
            {
                DrawPlayerCone(player);
            }

            if (Profile.TargetRing.Enabled)
            {
                DrawTargetRing(player);
            }

            if (Profile.Positionals.Enabled)
            {
                DrawPositionals(player);
            }

            if (Profile.Hitbox.Enabled)
            {
                DrawHitbox(player);
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }

        private void DrawHitbox(Character player)
        {
            Vector3 pos = player.Position;
            ConfigurationProfile.HitboxSettings c = Profile.Hitbox;

            if (c.UseTargetY && player.TargetObject != null)
            {
                pos.Y = player.TargetObject.Position.Y;

                if (c.ShowTargetDeltaY)
                {
                    Canvas.Segment(pos, player.Position, new(c.Color, 2));
                }
            }

            Canvas.CircleXZ(pos, .02f, new(c.OutlineColor, 5));
            Canvas.CircleXZ(pos, .01f, new(c.Color, 4));
        }

        private void DrawPlayerRing(Character player)
        {
            ConfigurationProfile.RingSettings c = Profile.PlayerRing;
            Canvas.CircleXZ(player.Position, c.Radius, c.Brush);
        }

        private void DrawPlayerCone(Character player)
        {
            // Rotate arc towards target (if it exists)
            ConfigurationProfile.ConeSettings c = Profile.Cone;
            GameObject? target = player.TargetObject;
            double rotation = target != null
                ? Math.Atan2(target.Position.X - player.Position.X, target.Position.Z - player.Position.Z)
                : player.Rotation;

            Canvas.ConeCenteredXZ(player.Position, c.Radius, (float)rotation, Maths.Radians(c.Angle), c.Brush);
        }

        private void DrawTargetRing(Character player)
        {
            if (player.TargetObject != null)
            {
                Canvas.CircleXZ(player.TargetObject.Position, Profile.TargetRing.Radius, Profile.TargetRing.Brush);
            }
        }

        private void DrawPositionals(Character player)
        {
            ConfigurationProfile.PositionalsSettings c = Profile.Positionals;
            GameObject? target = player.TargetObject;

            // Don't draw positionals if not targeting a battle NPC
            if (target == null || target.ObjectKind != ObjectKind.BattleNpc)
            {
                return;
            }

            // Annoyingly, the hitbox size changes on mounts. Maybe detect and hardcode, its a slight annoyance in the world.
            float playerHitbox = player.HitboxRadius;

            // For an ability to be in range, the character's hitbox (plus the range of the attack) has to overlap with the target's hitbox
            float hitboxes = playerHitbox + target.HitboxRadius;
            float melee = hitboxes + RangeAutoAttack;               // XXX: Is this fully accurate? Is there a real analysis around this value?
            float ability = hitboxes + RangeAbilityMelee;

            List<(Region Region, Brush Brush)>? regionBrushes = Regions.FromConfig(c, melee, ability);

            if (c.ArrowEnabled)
            {
                DrawEnemyArrow(target, 0, melee);
            }

            // TODO: If the target doesn't need positionals then don't draw sectors
            foreach ((Region region, Brush brush) in regionBrushes)
            {
                Canvas.ActorDonutSliceXZ(
                    target,
                    region.Radius.Inner,
                    region.Radius.Outer,
                    region.Positional.StartRads,
                    region.Positional.EndRads,
                    brush
                );
            }

            if (c.HighlightCurrentRegion)
            {
                Actor targetActor = new(target);
                foreach ((Region region, Brush brush) in regionBrushes)
                {
                    if (targetActor.RegionContains(region, player.Position))
                    {
                        Brush fillBrush = brush with
                        {
                            Fill = brush.Color with
                            {
                                W = brush.Color.W * c.HighlightTransparencyMultiplier
                            }
                        };

                        Canvas.ActorDonutSliceXZ(
                            target,
                            region.Radius.Inner, region.Radius.Outer,
                            region.Positional.StartRads, region.Positional.EndRads,
                            fillBrush
                        );
                        break;
                    }
                }
            }
        }

        private void DrawEnemyArrow(GameObject target, float angle, float pointRadius)
        {
            ConfigurationProfile.PositionalsSettings c = Profile.Positionals;
            Canvas.ActorArrowXZ(target, pointRadius, angle, c.ArrowScale, c.BrushFront);
        }

        internal void Debug(string message, params object[] values)
        {
            if (ConfigManager.Config.Debug)
            {
                PluginLog.Log(message, values);
            }
        }

        private void OnJobChange(object? sender, string classJobAbbrev)
        {
            PluginLog.Log($"Detected class change: {classJobAbbrev}");

            ConfigurationProfile? profile = ConfigManager.Config.ProfileForClassJob(classJobAbbrev);
            if (profile != null)
            {
                ConfigManager.Config.Active = profile;
            }
        }
    }
}
