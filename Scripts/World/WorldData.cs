using Godot;

[GlobalClass]
public partial class WorldData : Node
{
    public static WorldData Instance { get; private set; }
    public static StringName RecreateSaveGroup = new StringName("RecreateSaveNode");
    public static StringName ReloadSaveGroup = new StringName("ReloadSaveNode");

    private const string SAVE_FILE_LOCATION = "user://savegame.save";
    private const string SAVE_KEY_FILEPATH = "FilePath";
    private const string SAVE_KEY_PARENT = "Parent";
    private const string SAVE_KEY_POS_X = "PosX";
    private const string SAVE_KEY_POS_Y = "PosY";
    private const string SAVE_KEY_POS_Z = "PosZ";

    private Node rootNode;

    public WorldData()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        rootNode = GetParent();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("save"))
        {
            Save();
        }

        if(Input.IsActionJustPressed("load"))
        {
            Load();
        }
    }

    public void Load()
    {
        if (!FileAccess.FileExists(SAVE_FILE_LOCATION))
        {
            return;
        }

        var saveNodes = rootNode.GetTree().GetNodesInGroup(RecreateSaveGroup);
        foreach (Node saveNode in saveNodes)
        {
            saveNode.QueueFree();
        }

        using var saveFile = FileAccess.Open(SAVE_FILE_LOCATION, FileAccess.ModeFlags.Read);

        while (saveFile.GetPosition() < saveFile.GetLength())
        {
            var jsonString = saveFile.GetLine();

            // Creates the helper class to interact with JSON.
            var json = new Json();
            var parseResult = json.Parse(jsonString);
            if (parseResult != Error.Ok)
            {
                GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
                continue;
            }

            // Get the data from the JSON object.
            var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
            IWorldSaveable saveable = null;

            // Firstly, we need to create the object and add it to the tree and set its position.
            if (nodeData.ContainsKey(SAVE_KEY_FILEPATH))
            {
                var newObjectScene = GD.Load<PackedScene>(nodeData[SAVE_KEY_FILEPATH].ToString());
                var newObject = newObjectScene.Instantiate<Node>();
                GetNode(nodeData[SAVE_KEY_PARENT].ToString()).AddChild(newObject);
                newObject.Set(Node3D.PropertyName.Position, new Vector3((float)nodeData[SAVE_KEY_POS_X], (float)nodeData[SAVE_KEY_POS_Y], (float)nodeData[SAVE_KEY_POS_Z]));

                saveable = newObject as IWorldSaveable;
            }
            else
            {
                saveable = GetNode(nodeData[SAVE_KEY_PARENT].ToString()) as IWorldSaveable;
            }

            saveable.SetSaveData(nodeData);
        }
    }


    public void Save() {

        using var saveFile = FileAccess.Open(SAVE_FILE_LOCATION, FileAccess.ModeFlags.Write);

        Godot.Collections.Array<Node> saveNodes = rootNode.GetTree().GetNodesInGroup(RecreateSaveGroup);
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

                if (node is Node3D positionalNode)
                {
                    saveDataDictionary.Add(SAVE_KEY_POS_X, positionalNode.Position.X);
                    saveDataDictionary.Add(SAVE_KEY_POS_Y, positionalNode.Position.Y);
                    saveDataDictionary.Add(SAVE_KEY_POS_Z, positionalNode.Position.Z);
                }
            }

            var saveDataJson = Json.Stringify(saveDataDictionary);
            saveFile.StoreLine(saveDataJson);
        }

        Godot.Collections.Array<Node> reloadNodes = rootNode.GetTree().GetNodesInGroup(ReloadSaveGroup);
        foreach (Node node in reloadNodes)
        {
            if (node is not IWorldSaveable saveableNode)
            {
                GD.PushError($"Node that is not implementing {nameof(IWorldSaveable)} cannot be saved: {node.Name}");
                continue;
            }

            var saveDataDictionary = saveableNode.GetSaveData();
            saveDataDictionary.Add(SAVE_KEY_PARENT, node.GetPath());

            var saveDataJson = Json.Stringify(saveDataDictionary);
            saveFile.StoreLine(saveDataJson);
        }

        GD.Print($"Saved file to: {saveFile.GetPathAbsolute()}");
    }
}