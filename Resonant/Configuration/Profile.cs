using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Resonant
{
    [Serializable]
    public class ConfigurationProfile
    {
        public string Name;

        public Guid ID;

        public struct HitboxSettings
        {
            public bool Enabled = true;
            public Vector4 Color = ColorPresets.Green;
            public bool Outline = true;
            public Vector4 OutlineColor = ColorPresets.Black;
            public bool UseTargetY = true;
            public bool ShowTargetDeltaY = true;

            public HitboxSettings(bool enabled, Vector4 color, bool outline, Vector4 outlineColor, bool useTargetY, bool showTargetDeltaY)
            {
                Enabled = enabled;
                Color = color;
                Outline = outline;
                OutlineColor = outlineColor;
                UseTargetY = useTargetY;
                ShowTargetDeltaY = showTargetDeltaY;
            }
        }
        public HitboxSettings Hitbox = new();

        public struct RingSettings
        {
            public bool Enabled = false;
            public float Radius = 5f;
            public Brush Brush = new(ColorPresets.Green, 1);

            public RingSettings(bool enabled, float radius, Brush brush)
            {
                Enabled = enabled;
                Radius = radius;
                Brush = brush;
            }
        }
        public RingSettings TargetRing = new();
        public RingSettings PlayerRing = new();

        public struct ConeSettings
        {
            public bool Enabled = false;
            public float Radius = 7f;
            public int Angle = 90;
            public Brush Brush = new(ColorPresets.Blurple, 3);

            public ConeSettings(bool enabled, float radius, int angle, Brush brush)
            {
                Enabled = enabled;
                Radius = radius;
                Angle = angle;
                Brush = brush;
            }
        }
        public ConeSettings Cone = new();

        public struct PositionalsSettings
        {
            public bool Enabled = true;

            public bool MeleeAbilityRange = true;
            public int MeleeAbilityThickness = 1;

            public int Thickness = 3;
            public Vector4 ColorFront = ColorPresets.Red;
            public bool FrontSeparate = false;
            public Vector4 ColorRear = ColorPresets.Magenta;
            public bool RearSeparate = false;
            public Vector4 ColorFlank = ColorPresets.Blurple;
            public FlankRegionSetting FlankType = FlankRegionSetting.RearOnly;

            public bool HighlightCurrentRegion = true;
            public float HighlightTransparencyMultiplier = 0.1f;

            public bool ArrowEnabled = true;
            public float ArrowScale = 1f;

            public PositionalsSettings(bool enabled, bool meleeAbilityRange, int meleeAbilityThickness, int thickness, Vector4 colorFront, bool frontSeparate, Vector4 colorRear, bool rearSeparate, Vector4 colorFlank, FlankRegionSetting flankType, bool highlightCurrentRegion, float highlightTransparencyMultiplier, bool arrowEnabled, float arrowScale)
            {
                Enabled = enabled;
                MeleeAbilityRange = meleeAbilityRange;
                MeleeAbilityThickness = meleeAbilityThickness;
                Thickness = thickness;
                ColorFront = colorFront;
                FrontSeparate = frontSeparate;
                ColorRear = colorRear;
                RearSeparate = rearSeparate;
                ColorFlank = colorFlank;
                FlankType = flankType;
                HighlightCurrentRegion = highlightCurrentRegion;
                HighlightTransparencyMultiplier = highlightTransparencyMultiplier;
                ArrowEnabled = arrowEnabled;
                ArrowScale = arrowScale;
            }

            public Brush BrushFront => new(ColorFront, Thickness);
            public Brush BrushRear => new(ColorRear, Thickness);
            public Brush BrushFlank => new(ColorFlank, Thickness);
        }
        public PositionalsSettings Positionals = new();

        public List<string> Jobs = new();

        public ConfigurationProfile(string name) {
            Name = name;
            ID = Guid.NewGuid();
        }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    internal static class ColorPresets
    {
        public static readonly Vector4 Red = new(1, 0, 0, 1.0f);
        public static readonly Vector4 Black = new(0, 0, 0, 1.0f);
        public static readonly Vector4 Green = new(0.34f, 0.92f, 0.05f, 1.0f);

        public static readonly Vector4 Blurple = new(0.275f, 0.05f, 0.92f, 1.0f);
        public static readonly Vector4 Magenta = new(0.92f, 0.05f, 0.829f, 1.0f);
    }

    // TODO: Better name
    public enum FlankRegionSetting
    {
        Full,           // Draw the full  90deg
        RearOnly,       // Only draw rear 45deg
        FullSeparated,  // Draw the full  90deg, but separate the regions
    }

    internal static class FlankRegionSettingExtension
    {
        public static string Description(this FlankRegionSetting setting) => setting switch
        {
            FlankRegionSetting.Full => "Full (90 degrees)",
            FlankRegionSetting.RearOnly => "Rear Only (45 degrees)",
            FlankRegionSetting.FullSeparated => "Separated (90 degrees, separated)",
            _ => "error - unknown setting",
        };
    }
}
