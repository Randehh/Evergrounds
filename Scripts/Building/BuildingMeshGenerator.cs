using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class BuildingMeshGenerator : Node3D
{

    [Export]
    private MeshInstance3D meshInstanceRight;

    [Export]
    private MeshInstance3D meshInstanceLeft;

    [Export]
    private MeshInstance3D meshInstanceTop;

    [Export]
    private PackedScene pillarScene;

    [Export]
    private float wallThickness = 0.2f;

    [Export]
    private float wallHeight = 1;

    [Export]
    private Node3D pillarParent;

    public override void _Ready()
    {
        base._Ready();


    }

    [ExportToolButton("Build demo wall")]
    public Callable BuildDemoWallCallable => Callable.From(BuildDemoWall);

    public void BuildDemoWall()
    {
        foreach (var node in pillarParent.GetChildren())
        {
            node.QueueFree();
        }

        GenerateWalls(new List<Vector3> {
            new Vector3(0, 0, 0),
            new Vector3(2, 0, 0),
            new Vector3(2, 0, 4),
            new Vector3(0, 0, 2),
        }, true);
    }

    public void GenerateWalls(List<Vector3> points, bool isLoop)
    {
        SurfaceTool stLeft = new SurfaceTool();
        SurfaceTool stRight = new SurfaceTool();
        SurfaceTool stTop = new SurfaceTool();

        stLeft.Begin(Mesh.PrimitiveType.Triangles);
        stRight.Begin(Mesh.PrimitiveType.Triangles);
        stTop.Begin(Mesh.PrimitiveType.Triangles);

        for (int i = 0; i < points.Count - 1; i++)
        {
            GenerateWall(stLeft, stRight, points[i], points[i + 1]);
            GenerateTop(stTop, points[i], points[i + 1]);

            AddPillar(points[i]);
        }

        AddPillar(points[points.Count - 1]);

        if (isLoop)
        {
            GenerateWall(stLeft, stRight, points[points.Count - 1], points[0]);
            GenerateTop(stTop, points[points.Count - 1], points[0]);
        }

        //stLeft.GenerateNormals(true);
        //stLeft.GenerateTangents();
        //
        //stRight.GenerateNormals(false);
        //stRight.GenerateTangents();

        ArrayMesh meshLeft = stLeft.Commit();
        meshInstanceLeft.Mesh = meshLeft;

        ArrayMesh meshRight = stRight.Commit();
        meshInstanceRight.Mesh = meshRight;

        ArrayMesh meshTop = stTop.Commit();
        meshInstanceTop.Mesh = meshTop;
    }

    private void GenerateWall(SurfaceTool stLeft, SurfaceTool stRight, Vector3 point1, Vector3 point2)
    {
        Vector3 delta = point2 - point1;
        Vector3 left = delta.Rotated(Vector3.Up, Mathf.Pi * 0.5f).Normalized();
        Vector3 right = delta.Rotated(Vector3.Up, Mathf.Pi * -0.5f).Normalized();

        GenerateSubWallLeft(stLeft, point1, point2, left);
        GenerateSubWallRight(stRight, point1, point2, right);
    }

    private void GenerateSubWallRight(SurfaceTool st, Vector3 point1, Vector3 point2, Vector3 direction)
    {
        float subWallThickness = wallThickness * 0.5f;

        Vector3 bottomLeft = point1 + (direction * subWallThickness);
        Vector3 bottomRight = point2 + (direction * subWallThickness);
        Vector3 topLeft = bottomLeft + (Vector3.Up * wallHeight);
        Vector3 topRight = bottomRight + (Vector3.Up * wallHeight);

        // Triangle top left
        st.SetNormal(direction);
        st.SetUV(new Vector2(0, 0));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(bottomLeft);

        st.SetNormal(direction);
        st.SetUV(new Vector2(0, 1));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(topLeft);

        st.SetNormal(direction);
        st.SetUV(new Vector2(1, 1));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(topRight);

        // Triangle bottom right
        st.SetNormal(direction);
        st.SetUV(new Vector2(0, 0));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(bottomLeft);

        st.SetNormal(direction);
        st.SetUV(new Vector2(1, 1));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(topRight);

        st.SetNormal(direction);
        st.SetUV(new Vector2(1, 0));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(bottomRight);
    }

    private void GenerateSubWallLeft(SurfaceTool st, Vector3 point1, Vector3 point2, Vector3 direction)
    {
        float subWallThickness = wallThickness * 0.5f;

        Vector3 bottomLeft = point1 + (direction * subWallThickness);
        Vector3 bottomRight = point2 + (direction * subWallThickness);
        Vector3 topLeft = bottomLeft + (Vector3.Up * wallHeight);
        Vector3 topRight = bottomRight + (Vector3.Up * wallHeight);

        // Triangle top left
        st.SetNormal(direction);
        st.SetUV(new Vector2(1, 1));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(topRight);

        st.SetNormal(direction);
        st.SetUV(new Vector2(0, 1));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(topLeft);

        st.SetNormal(direction);
        st.SetUV(new Vector2(0, 0));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(bottomLeft);

        // Triangle bottom right
        st.SetNormal(direction);
        st.SetUV(new Vector2(1, 0));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(bottomRight);

        st.SetNormal(direction);
        st.SetUV(new Vector2(1, 1));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(topRight);

        st.SetNormal(direction);
        st.SetUV(new Vector2(0, 0));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(bottomLeft);
    }

    private void GenerateTop(SurfaceTool st, Vector3 point1, Vector3 point2)
    {
        Vector3 direction = Vector3.Up;
        Vector3 wallVector = Vector3.Up * wallHeight;
        float subWallThickness = wallThickness * 0.5f;

        Vector3 delta = point2 - point1;
        Vector3 left = delta.Rotated(Vector3.Up, Mathf.Pi * 0.5f).Normalized();
        Vector3 right = delta.Rotated(Vector3.Up, Mathf.Pi * -0.5f).Normalized();

        Vector3 bottomLeft = point1 + wallVector + (left * subWallThickness);
        Vector3 bottomRight = point1 + wallVector + (right * subWallThickness);
        Vector3 topLeft = point2 + wallVector + (left * subWallThickness);
        Vector3 topRight = point2 + wallVector + (right * subWallThickness);

        // Triangle top left
        st.SetNormal(direction);
        st.SetUV(new Vector2(1, 1));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(bottomLeft);

        st.SetNormal(direction);
        st.SetUV(new Vector2(0, 1));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(topLeft);

        st.SetNormal(direction);
        st.SetUV(new Vector2(0, 0));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(topRight);

        // Triangle bottom right
        st.SetNormal(direction);
        st.SetUV(new Vector2(1, 0));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(bottomLeft);

        st.SetNormal(direction);
        st.SetUV(new Vector2(1, 1));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(topRight);

        st.SetNormal(direction);
        st.SetUV(new Vector2(0, 0));
        st.SetSmoothGroup(UInt32.MaxValue);
        st.AddVertex(bottomRight);
    }

    private void AddPillar(Vector3 position)
    {
        Node3D pillar = pillarScene.Instantiate<Node3D>();
        pillarParent.AddChild(pillar);
        pillar.Owner = this.GetParent();
        pillar.Position = position;
        pillar.Name = $"Pillar_{position.X}_{position.Z}";
    }
}
