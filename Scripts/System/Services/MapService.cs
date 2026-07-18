using Godot;
using System.Collections.Generic;

public class MapService : IService
{

    private HashSet<Vector2I> emptySlots = new();

    public void OnInit()
    {

    }

    public void OnReady()
    {

    }

    public void OnDestroy()
    {

    }

    public void SetEmptySlot(Vector2I slot, bool isEmpty)
    {
        if(isEmpty) {
            emptySlots.Add(slot);
        } else {
            emptySlots.Remove(slot);
        }
    }

    public bool IsEmptySlot(Vector2I slot)
    {
        return emptySlots.Contains(slot);
    }
}