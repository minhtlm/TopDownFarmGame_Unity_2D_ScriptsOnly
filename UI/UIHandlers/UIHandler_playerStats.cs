using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_playerStats : MonoBehaviour
{
    private VisualElement healthValueContainer;
    private VisualElement hungerValueContainer;
    private VisualElement thirstValueContainer;
    private Label moneyLabel;
    private PlayerStats playerStats;

    // Start is called before the first frame update
    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        healthValueContainer = uiDocument.rootVisualElement.Q<VisualElement>("HealthValueContainer");
        hungerValueContainer = uiDocument.rootVisualElement.Q<VisualElement>("HungerValueContainer");
        thirstValueContainer = uiDocument.rootVisualElement.Q<VisualElement>("ThirstValueContainer");
        moneyLabel = uiDocument.rootVisualElement.Q<Label>("MoneyLabel");

        if (playerStats == null)
        {
            playerStats = PlayerStats.Instance;
        }
        if (playerStats != null)
        {
            playerStats.OnMoneyChanged += UpdateMoneyAmount; // Subscribe to the money changed event
            playerStats.OnStatsChanged += UpdateStatsBars; // Subscribe to the stats changed event
            UpdateMoneyAmount();
            UpdateStatsBars();
        }
    }
    
    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnMoneyChanged -= UpdateMoneyAmount;
            playerStats.OnStatsChanged -= UpdateStatsBars;
        }
    }

    void UpdateMoneyAmount()
    {
        if (moneyLabel != null && playerStats != null)
        {
            int currentMoney = int.Parse(moneyLabel.text);
            int newMoney = playerStats.Money;
            DOTween.To(() => currentMoney, x => currentMoney = x, newMoney, 1.0f).OnUpdate(() =>
            {
                moneyLabel.text = currentMoney.ToString();
            });
        }
    }

    void UpdateStatsBars()
    {
        if (playerStats != null && healthValueContainer != null && hungerValueContainer != null && thirstValueContainer != null)
        {
            SetHealthValue(playerStats.Health / 100f);
            SetHungerValue(playerStats.Hunger / 100f);
            SetThirstValue(playerStats.Thirst / 100f);
        }
    }

    public void SetHealthValue(float percentage)
    {
        healthValueContainer.style.width = Length.Percent(percentage * 100);
    }

    public void SetHungerValue(float percentage)
    {
        hungerValueContainer.style.width = Length.Percent(percentage * 100);
    }

    public void SetThirstValue(float percentage)
    {
        thirstValueContainer.style.width = Length.Percent(percentage * 100);
    }

}
