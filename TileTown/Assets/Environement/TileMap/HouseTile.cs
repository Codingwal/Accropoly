using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class HouseTile : MapTileScript
{
    public override bool CanPersist()
    {
        GameObject neighbour;

        neighbour = MapHandler.GetTileFromNeighbour(new(X, Y), transform.eulerAngles.y);

        if (neighbour == null) return false;

        if (!neighbour.TryGetComponent(out IHouseConnectable houseConnectableScript))
        {
            return false;
        }
        if (houseConnectableScript.ArableTiles.Contains(new(X, Y)))
        {
            return true;
        }
        return false;
    }
}
