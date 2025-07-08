using UnityEngine;

public class LivesShop : MonoBehaviour
{
    [SerializeField] private int pricePerLife = 20;

    public void BuyLives(int count)
    {
        int totalPrice = pricePerLife * count;
        if (CurrencyManager.Instance.TrySpendCoins(totalPrice))
        {
            LivesManager.Instance.AddLives(count);
            Debug.Log($"Покупка {count} жизней прошла успешно!");
        }
        else
        {
            Debug.Log("Недостаточно монет.");
        }
    }
}
