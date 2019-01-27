using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceHandler : Singleton<ResourceHandler>
{
    public BasicIntegerSciptableObject moneyValue;

    public bool Purchase(int price)
    {
        if (moneyValue.value < price) return false;
        
        moneyValue.value -= price;
        Debug.Log(moneyValue.value);
        return true;
    }

    public void GainMoney(int amount)
    {
        moneyValue.value += amount;
    }
}
