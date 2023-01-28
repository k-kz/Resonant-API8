﻿using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using ImGuiNET;

namespace Resonant
{
    public struct Brush
    {
        public float Thickness;
        public Vector4 Color;
        public Vector4 Fill;
        public Brush(Vector4 color, float thickness, Vector4 fill = default(Vector4))
        {
            Color = color;
            Thickness = thickness;
            Fill = fill;
        }
        public bool HasFill() => Fill.W != 0;
    }

    internal class Canvas
    {
        private Configuration Config { get; }
        private ConfigurationProfile Profile => Config.Active;

        private GameGui Gui { get; }

        internal Canvas(Configuration config, GameGui gui)
        {
            Config = config;
            Gui = gui;
        }

        internal void Begin()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Config.ViewportWindowBox.TopLeft);
            _ = ImGui.Begin("ResonantOverlay",
                ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);

            Vector2 displaySize = ImGui.GetIO().DisplaySize;
            ImGui.SetWindowSize(Config.ViewportWindowBox.SizeWith(displaySize));
        }

        // ----------- Actor-aware draw methods --------------
        internal void ActorConeXZ(GameObject actor, float radius, float startRads, float endRads, Brush brush) =>
            ConeXZ(actor.Position, radius, startRads + actor.Rotation, endRads + actor.Rotation, brush);

        internal void ActorArrowXZ(GameObject actor, float radius, float angle, float scale, Brush brush)
        {
            float direction = angle + actor.Rotation;

            // Scale the drawing by shifting the "circle center" up the radial and reducing the radius accordingly
            float centerOffset = radius * (1 - scale);
            Vector3 pos = actor.Position + new Vector3(centerOffset * (float)Math.Sin(direction), 0, centerOffset * (float)Math.Cos(direction));
            float arrowSize = radius - centerOffset;

            // Edge case: when == 1 and there is a thickness, the arrow pokes out the sides.
            bool drawBottom = scale != 1f;
            ConvexShape? shape = new(Gui, brush);
            if (drawBottom) shape.Point(pos);
            shape.PointRadial(pos, arrowSize, direction + Maths.Radians(90));
            shape.PointRadial(pos, arrowSize, direction + Maths.Radians(0));
            shape.PointRadial(pos, arrowSize, direction + Maths.Radians(-90));
            if (drawBottom) shape.Point(pos);
            shape.Done();
        }

        internal void ActorDonutSliceXZ(GameObject actor, float innerRadius, float outerRadius, float startRads, float endRads, Brush brush) =>
            DonutSliceXZ(actor.Position, innerRadius, outerRadius, startRads + actor.Rotation, endRads + actor.Rotation, brush);

        internal void CircleXZ(Vector3 position, float radius, Brush brush) => CircleArcXZ(position, radius, 0f, Maths.TAU, brush);

        // ----------- Position-based draw methods --------------
        internal void ConeXZ(Vector3 center, float radius, float startRads, float endRads, Brush brush)
        {
            ConvexShape? shape = new(Gui, brush);
            shape.Point(center);
            shape.Arc(center, radius, startRads, endRads);
            shape.Point(center);
            shape.Done();
        }

        internal void DonutSliceXZ(Vector3 center, float innerRadius, float outerRadius, float startRads, float endRads, Brush brush)
        {
            if (innerRadius == 0 && endRads - startRads <= (Maths.PI + Maths.Epsilon))
            {
                // Special case: a cone, which is a convex polygon
                ConeXZ(center, outerRadius, startRads, endRads, brush);
                return;
            }

            // A donut slice is a non-convex object so is not cleanly handled by imgui instead, approximate with slices
            int segments = Maths.ArcSegments(startRads, endRads);
            float radsPerSegment = (endRads - startRads) / segments;

            // Outline
            Brush outlineBrush = brush with { Fill = new() };
            ConvexShape? outline = new(Gui, outlineBrush);
            outline.Arc(center, outerRadius, startRads, endRads);
            outline.Arc(center, innerRadius, endRads, startRads);
            outline.PointRadial(center, outerRadius, startRads);
            outline.Done();

            // Fill
            if (brush.HasFill())
            {
                Brush sliceBrush = brush with { Thickness = 0f };
                for (int i = 0; i < segments; i++)
                {
                    float start = startRads + (i * radsPerSegment);
                    float end   = startRads + ((i + 1) * radsPerSegment);

                    ConvexShape? shape = new(Gui, sliceBrush);
                    shape.Arc(center, outerRadius, start, end);
                    shape.Arc(center, innerRadius, end, start);
                    shape.PointRadial(center, outerRadius, start);
                    shape.Done();
                }
            }
        }

        internal void ConeCenteredXZ(Vector3 center, float radius, float directionRads, float angleRads, Brush brush)
        {
            float startRads = directionRads - (angleRads / 2);
            float endRads   = directionRads + (angleRads / 2);

            ConeXZ(center, radius, startRads, endRads, brush);
        }

        internal void CircleArcXZ(Vector3 gamePos, float radius, float startRads, float endRads, Brush brush)
        {
            ConvexShape? shape = new(Gui, brush);
            shape.Arc(gamePos, radius, startRads, endRads);
            shape.Done();
        }

        internal void Segment(Vector3 startPos, Vector3 endPos, Brush brush)
        {
            ConvexShape? shape = new(Gui, brush);
            shape.Point(startPos);
            shape.Point(endPos);
            shape.Done();
        }
    }
}
