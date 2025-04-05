using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    public float Health => health;

    [SerializeField] private float hunger = 100f;
    public float Hunger => hunger;

    [SerializeField] private float thirst = 100f;
    public float Thirst => thirst;

    [SerializeField] private int money = 0;
    public int Money => money;

    [SerializeField] private float statsDecreaseInterval = 5f;
    [SerializeField] private float healthDecreaseAmountFromHunger = 2f;
    [SerializeField] private float healthDecreaseAmountFromThirst = 3f;
    [SerializeField] private float hungerDecreaseAmount = 3f;
    [SerializeField] private float thirstDecreaseAmount = 5f;

    public event System.Action OnMoneyChanged; // Event to trigger when the money changes
    public event System.Action OnStatsChanged; // Event to trigger when the health, thirst or hunger changes

    public static PlayerStats Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(DecreaseStatsOverTime());
    }

    IEnumerator DecreaseStatsOverTime()
    {
        while (health > 0f)
        {
            yield return new WaitForSeconds(statsDecreaseInterval);
            hunger = Mathf.Max(hunger - hungerDecreaseAmount, 0f);
            thirst = Mathf.Max(thirst - thirstDecreaseAmount, 0f);

            if (hunger <= 0f)
            {
                health = Mathf.Max(health - healthDecreaseAmountFromHunger, 0f);
            }
            if (thirst <= 0f)
            {
                health = Mathf.Max(health - healthDecreaseAmountFromThirst, 0f);
            }

            OnStatsChanged?.Invoke(); // Trigger the event when stats change
        }
    }

    public void AddHealth(float amount)
    {
        if (amount <= 0) return;

        health = Mathf.Min(health + amount, 100f);
        OnStatsChanged?.Invoke();
    }

    public void AddHunger(float amount)
    {
        if (amount <= 0) return;

        hunger = Mathf.Min(hunger + amount, 100f);
        OnStatsChanged?.Invoke();
    }

    public void AddThirst(float amount)
    {
        if (amount <= 0) return;

        thirst = Mathf.Min(thirst + amount, 100f);
        OnStatsChanged?.Invoke();
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        money += amount;
        OnMoneyChanged?.Invoke();
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return false;

        if (money >= amount)
        {
            money -= amount;
            OnMoneyChanged?.Invoke();
            return true;
        }

        return false;
    }

    public bool CanAfford(int amount)
    {
        return money >= amount;
    }
}
