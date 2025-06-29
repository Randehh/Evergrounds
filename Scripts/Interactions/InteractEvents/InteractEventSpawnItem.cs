using Godot;
using Godot.Collections;
using System.Linq;

[GlobalClass]
public partial class InteractEventSpawnItem : InteractEvent
{
    [Export]
    private Array<WorldItemSpawnerItemData> itemsToSpawn;

    public override void Execute()
    {
        WorldItemSpawner spawner = new WorldItemSpawner(itemsToSpawn.ToArray());
        GroupUtility.GetWorldMap<WorldMap>().AddChild(spawner);
        spawner.GlobalPosition = GlobalPosition;
    }
}