using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_status : MonoBehaviour
{
    private PlayerInventory playerInventory;
    private Label moneyLabel;

    // Start is called before the first frame update
    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        moneyLabel = uiDocument.rootVisualElement.Q<Label>("MoneyAmount");
        if (playerInventory == null)
        {
            playerInventory = PlayerInventory.Instance;
        }
        if (playerInventory != null)
        {
            playerInventory.OnMoneyChanged += UpdateMoneyAmount;
            UpdateMoneyAmount();
        }
    }

    void UpdateMoneyAmount()
    {
        if (moneyLabel != null && playerInventory != null)
        {
            int currentMoney = int.Parse(moneyLabel.text);
            int newMoney = playerInventory.GetMoney();
            DOTween.To(() => currentMoney, x => currentMoney = x, newMoney, 1.0f).OnUpdate(() =>
            {
                moneyLabel.text = currentMoney.ToString();
            });
        }
    }

    void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnMoneyChanged -= UpdateMoneyAmount;
        }
    }
}
