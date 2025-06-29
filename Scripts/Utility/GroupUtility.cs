using Godot;

public static class GroupUtility
{

    public static StringName GROUP_WORLD_MAP = new("worldmap");

    // Statics with states are ew, but we'll do it for now
    private static Node TreeRoot;

    public static void SetTreeRoot(Node node) => TreeRoot = node;

    public static T GetNodeFromGroup<T>(StringName groupName) where T : class
    {
        Node n = TreeRoot.GetTree().GetFirstNodeInGroup(groupName);

        if(n == null || n is not T castNode)
        {
            return null;
        }

        return castNode;
    }

    // Shortcuts
    public static IWorldMap GetWorldMap()
    => GetNodeFromGroup<IWorldMap>(GROUP_WORLD_MAP);

    public static TMap GetWorldMap<TMap>() where TMap : class, IWorldMap
        => GetNodeFromGroup<TMap>(GROUP_WORLD_MAP);
}