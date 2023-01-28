﻿using System;
using System.Numerics;
using Dalamud.Game.Gui;
using ImGuiNET;

namespace Resonant
{
    // Makes complete shapes using coordinates from in-game space
    internal class ConvexShape
    {
        internal readonly Brush Brush;
        internal readonly GameGui Gui;
        internal readonly ImDrawListPtr DrawList;

        internal bool cullObject = true;

        internal ConvexShape(GameGui gui, Brush brush)
        {
            Gui = gui;
            Brush = brush;
            DrawList = ImGui.GetWindowDrawList();
        }

        internal void Point(Vector3 worldPos)
        {
            // TODO: Implement proper clipping. Everything goes crazy when drawing lines outside the clip window and behind the camera point
            bool visible = Gui.WorldToScreen(worldPos, out Vector2 pos);
            DrawList.PathLineTo(pos);
            if (visible) { cullObject = false; }
        }

        internal void PointRadial(Vector3 center, float radius, float radians) => Point(new Vector3(
                center.X + (radius * (float)Math.Sin(radians)),
                center.Y,
                center.Z + (radius * (float)Math.Cos(radians))));

        internal void Arc(Vector3 center, float radius, float startRads, float endRads)
        {
            int segments = Maths.ArcSegments(startRads, endRads);
            float deltaRads = (endRads - startRads) / segments;

            for (int i = 0; i < segments + 1; i++)
            {
                PointRadial(center, radius, startRads + (deltaRads * i));
            }
        }

        internal void Done()
        {
            if (cullObject)
            {
                DrawList.PathClear();
                return;
            }

            if (Brush.HasFill())
            {
                DrawList.PathFillConvex(ImGui.GetColorU32(Brush.Fill));
            }
            else if (Brush.Thickness != 0)
            {
                DrawList.PathStroke(ImGui.GetColorU32(Brush.Color), ImDrawFlags.None, Brush.Thickness);
            }

            DrawList.PathClear();
        }
    }
}
