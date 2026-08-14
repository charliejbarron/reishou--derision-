using Godot;

internal static class Debugging
{
    static readonly Basis Cylinder = Basis.Identity.Scaled(new Vector3(0.2f, 1e-08f, 0.2f));

    internal static void HandleDebug(Transform3D transform3D, Vector3 interpolated, Player player, ChunkInfo info, Chunk[] connectedChunks)
    {
        DrawDebugSplits(info.Splits, connectedChunks);
        DrawDebugBlockers(info.Blockers);
        DrawPlayerDebug(transform3D, interpolated);
        DrawDebugAxis(transform3D, player);
    }
    internal static void DrawPlayerDebug(Transform3D transform3D, Vector3 interpolated)
    {
        Vector3 head = interpolated + new Vector3(0, GameSettings.CameraOffset, 0);
        float dist = transform3D.Origin.DistanceTo(head);

        if (dist <= 1f) 
            return;
        
        DebugDraw3D.DrawLine(head, interpolated, Colors.White);
        DebugDraw3D.DrawCylinder(new Transform3D(Cylinder, interpolated), Colors.MediumSpringGreen);
        DebugDraw3D.DrawCylinder(new Transform3D(Cylinder, interpolated + Vector3.Up * GameSettings.StepUpHeight), Colors.Chartreuse);
        DebugDraw3D.DrawCylinder(new Transform3D(Cylinder, interpolated + Vector3.Down * GameSettings.StepDownHeight), Colors.Crimson);
    }

    internal static void DrawDebugAxis(Transform3D transform3D, Player player)
    {
        Vector3 pos = -transform3D.Basis.Z + transform3D.Origin;
        float arrowLength = 0.2f;

        var conf = DebugDraw3D.NewScopedConfig();
        conf.SetThickness(0.005f);

        DebugDraw3D.DrawRay(pos, Vector3.Up, arrowLength, Colors.MediumSpringGreen);
        DebugDraw3D.DrawRay(pos, Vector3.Right, arrowLength, Colors.MediumVioletRed);
        DebugDraw3D.DrawRay(pos, Vector3.Back, arrowLength, Colors.MediumPurple);
                
        conf.SetThickness(0.0075f);
        Vector3 vel = player.CharacterBody.Velocity / 20f;

        if (vel.Length() > 5e-03f)
        {
            DebugDraw3D.DrawLine(pos, pos + vel, Colors.Crimson);
        }
        
        conf.Dispose();
    }

    internal static void DrawDebugSplits(Split[] splits, Chunk[] connections)
    {
        for (int s = 0; s < splits.Length; s++)
        {
            Split split = splits[s];
            if (split.Connected > 0)
                DebugDraw3D.DrawArrowRay(split.Position, new Vector3(split.Direction.X, 0, split.Direction.Y), 1f, Colors.MediumSpringGreen, 0.3f);

            Vector3 widthOffset = new Vector3(-split.Direction.Y, 0, split.Direction.X) * Mathf.Abs(split.Width);
            Vector3 heightOffset = Vector3.Up * Mathf.Abs(split.Height);

            Color heightDCol = split.Height < 0f ? Colors.GreenYellow : Colors.MediumSpringGreen;
            Color heightUCol = split.Height < 0f ? Colors.MediumVioletRed : Colors.PaleVioletRed;
            Color widthLCol = split.Width < 0f ? Colors.BlueViolet : Colors.MediumPurple;
            Color widthRCol = split.Width < 0f ? Colors.Blue : Colors.MediumTurquoise;

            DebugDraw3D.DrawLine(split.Position + widthOffset, split.Position - widthOffset, heightDCol);
            DebugDraw3D.DrawLine(split.Position + widthOffset + heightOffset, split.Position - widthOffset + heightOffset, heightUCol);
            DebugDraw3D.DrawLine(split.Position - widthOffset, split.Position - widthOffset + heightOffset, widthLCol);
            DebugDraw3D.DrawLine(split.Position + widthOffset, split.Position + widthOffset + heightOffset, widthRCol);

            DebugDraw3D.DrawText(split.Position + new Vector3(split.Direction.X, Mathf.Min(1.5f, Mathf.Abs(split.Height) / 2f), split.Direction.Y), connections[s].Name, 52);
        }
    }

    internal static void DrawDebugBlockers(Split[] blockers)
    {
        for (int b = 0; b < blockers.Length; b++)
        {
            Split blocker = blockers[b];

            Color blockerCol = Colors.Red;
            Color blockerColArrow = Colors.White;
            Color blockerColArrowDir = Colors.DarkRed;

            DebugDraw3D.DrawArrowRay(blocker.Position, new Vector3(blocker.Direction.X, 0, blocker.Direction.Y), 1f, blockerColArrowDir, 0.3f);

            Vector3 widthOffset = new Vector3(-blocker.Direction.Y, 0, blocker.Direction.X) * Mathf.Abs(blocker.Width);
            Vector3 heightOffset = Vector3.Up * Mathf.Abs(blocker.Height);

            DebugDraw3D.DrawArrowRay(blocker.Position + heightOffset / 2f, -new Vector3(blocker.Direction.X, 0, blocker.Direction.Y), 1f, blockerColArrow, 0.2f);

            Color heightCol = blocker.Height > 0f ? blockerCol : Colors.Aquamarine;
            Color widthCol = blocker.Width > 0f ? blockerCol : Colors.GreenYellow;
            
            DebugDraw3D.DrawLine(blocker.Position + widthOffset + heightOffset, blocker.Position - widthOffset + heightOffset, heightCol);
            DebugDraw3D.DrawLine(blocker.Position + widthOffset, blocker.Position - widthOffset, heightCol);
            
            DebugDraw3D.DrawLine(blocker.Position - widthOffset, blocker.Position - widthOffset + heightOffset, widthCol);
            DebugDraw3D.DrawLine(blocker.Position + widthOffset, blocker.Position + widthOffset + heightOffset, widthCol);

            DebugDraw3D.DrawRay(blocker.Position + widthOffset, -new Vector3(blocker.Direction.X, 0, blocker.Direction.Y), 1f, blockerColArrow);
            DebugDraw3D.DrawRay(blocker.Position - widthOffset, -new Vector3(blocker.Direction.X, 0, blocker.Direction.Y), 1f, blockerColArrow);
            DebugDraw3D.DrawRay(blocker.Position + widthOffset + heightOffset, -new Vector3(blocker.Direction.X, 0, blocker.Direction.Y), 1f, blockerColArrow);
            DebugDraw3D.DrawRay(blocker.Position - widthOffset + heightOffset, -new Vector3(blocker.Direction.X, 0, blocker.Direction.Y), 1f, blockerColArrow);
        }
    }
}