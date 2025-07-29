using TMPro;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText;

    private void Start()
    {
        UpdateUI();
        InvokeRepeating(nameof(UpdateUI), 1f, 1f);
    }

    private void UpdateUI()
    {
        coinsText.text = $"Coins: {CurrencyManager.Instance.Coins}";
    }
}
