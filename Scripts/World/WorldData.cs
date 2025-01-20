using Godot;
using System.Collections.Generic;

public class WorldData
{
    public static StringName SaveNodeGroup = new StringName("SaveNode");

    private const string SAVE_KEY_FILEPATH = "FilePath";
    private const string SAVE_KEY_PARENT = "Parent";
    private const string SAVE_KEY_POS_X = "PosX";
    private const string SAVE_KEY_POS_Y = "PosY";

    private Node rootNode;

    public WorldData(Node rootNode)
    {
        this.rootNode = rootNode;
    }

    public void Load()
    {

    }


    public void Save() {

        using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);

        Godot.Collections.Array<Node> saveNodes = rootNode.GetTree().GetNodesInGroup(SaveNodeGroup);
        foreach(Node node in saveNodes)
        {
            if(node is not IWorldSaveable saveableNode)
            {
                GD.PushError($"Node that is not implementing {nameof(IWorldSaveable)} cannot be saved: {node.Name}");
                continue;
            }

            var saveDataDictionary = saveableNode.GetSaveData();

            // Attach default info
            if (!string.IsNullOrEmpty(node.SceneFilePath))
            {
                saveDataDictionary.Add(SAVE_KEY_FILEPATH, node.SceneFilePath);
                saveDataDictionary.Add(SAVE_KEY_PARENT, node.GetParent().GetPath());

                if (node is Node2D positionalNode)
                {
                    saveDataDictionary.Add(SAVE_KEY_POS_X, positionalNode.Position.X);
                    saveDataDictionary.Add(SAVE_KEY_POS_Y, positionalNode.Position.Y);
                }
            }

            var saveDataJson = Json.Stringify(saveDataDictionary);
            saveFile.StoreLine(saveDataJson);
        }

        GD.Print($"Saved file to: {saveFile.GetPathAbsolute()}");
    }
}