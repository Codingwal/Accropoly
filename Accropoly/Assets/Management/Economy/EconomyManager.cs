using UnityEngine;

public class EconomyManager : Singleton<EconomyManager>
{
    public float balance;
    [SerializeField] private SerializableDictionary<TileType, int> tileBuyPrices;
    [SerializeField] private SerializableDictionary<TileType, int> tileSellPrices;

    private void OnEnable()
    {
        GameLoopManager.Instance.InitWorld += InitEconomy;
        GameLoopManager.Instance.SaveWorld += SaveEconomy;

        GameLoopManager.Instance.PayTaxes += PayTaxes;
    }
    private void OnDisable()
    {
        GameLoopManager.Instance.InitWorld -= InitEconomy;
        GameLoopManager.Instance.SaveWorld -= SaveEconomy;

        GameLoopManager.Instance.PayTaxes -= PayTaxes;
    }


    /// <returns>Can the tile be bought</returns>
    public bool BuyTile(TileType tileType)
    {
        balance -= tileBuyPrices[tileType];
        if (balance < 0)
        {
            balance += tileBuyPrices[tileType];
            return false;
        }
        return true;
    }
    public void SellTile(TileType tileType)
    {
        balance += tileSellPrices[tileType];
    }
    private void PayTaxes()
    {
        balance += PopulationManager.Instance.CalculateTaxes();
    }

    private void InitEconomy(World world)
    {
        balance = world.balance;
    }
    private void SaveEconomy(ref World world)
    {
        world.balance = balance;
    }


}
