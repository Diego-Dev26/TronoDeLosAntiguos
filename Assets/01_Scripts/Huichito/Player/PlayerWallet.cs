using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public int coins = 0;

    public void AddCoins(int amount)
    {
        coins = Mathf.Max(0, coins + amount);
    }

    public bool TrySpend(int price)
    {
        if (price <= 0) return true;
        if (coins < price) return false;
        coins -= price;
        return true;
    }
}