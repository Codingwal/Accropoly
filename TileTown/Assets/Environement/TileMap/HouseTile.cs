using UnityEngine;

public class HouseTile : MapTileScript
{
    public override bool CanBePlaced()
    {
        if (!base.CanBePlaced())
        {
            return false;
        }
        
        GameObject neighbour;

        // Prevent out of index exceptions
        try
        {
            neighbour = transform.eulerAngles.y switch
            {
                0 => MapHandler.Instance.map[X, Y + 1],
                90 => MapHandler.Instance.map[X + 1, Y],
                180 => MapHandler.Instance.map[X, Y - 1],
                _ => MapHandler.Instance.map[X - 1, Y],
            };
        }
        catch
        {
            return false;
        }

        IHouseConnectable houseConnectableScript = neighbour.GetComponent<IHouseConnectable>();
        if (houseConnectableScript == null)
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
