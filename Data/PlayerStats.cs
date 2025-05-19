using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private float health = 100f;
    public float Health => health;

    private float hunger = 100f;
    public float Hunger => hunger;

    private float thirst = 100f;
    public float Thirst => thirst;

    private int money = 0;
    public int Money => money;

    [SerializeField] private float statsDecreaseInterval = 5f;
    [SerializeField] private float healthDecreaseAmountFromHunger = 2f;
    [SerializeField] private float healthDecreaseAmountFromThirst = 3f;
    [SerializeField] private float healthIncreaseAmountFromHunger = 2f;
    [SerializeField] private float healthIncreaseAmountFromThirst = 3f;
    [SerializeField] private float hungerDecreaseAmount = 3f;
    [SerializeField] private float thirstDecreaseAmount = 5f;
    [SerializeField] private GameObject homeGrid;
    [SerializeField] private GameObject bed;
    [SerializeField] private UIHandler_FishingMinigame uiHandlerFishingMinigame;

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

            // Decrease hunger and thirst over time
            hunger = Mathf.Max(hunger - hungerDecreaseAmount, 0f);
            thirst = Mathf.Max(thirst - thirstDecreaseAmount, 0f);

            // Decrease health if hunger or thirst is 0
            // Increase if hunger or thirst is above 50 and both are not 0
            if (hunger <= 0f)
            {
                health = Mathf.Max(health - healthDecreaseAmountFromHunger, 0f);
            }
            else if (hunger > 30f)
            {
                health = Mathf.Min(health + healthIncreaseAmountFromHunger, 100f);
            }

            if (thirst <= 0f)
            {
                health = Mathf.Max(health - healthDecreaseAmountFromThirst, 0f);
            }
            else if (thirst > 30f)
            {
                health = Mathf.Min(health + healthIncreaseAmountFromThirst, 100f);
            }

            OnStatsChanged?.Invoke(); // Trigger the event when stats change
        }

        StartCoroutine(UIHandler_screenFader.Instance.FadeEndOfDay(
            () => {
                // Quit the fishing if the player is fishing
                if (PlayerController.Instance.IsFishing)
                {
                    PlayerController.Instance.StopFishing();
                }
                if (uiHandlerFishingMinigame.IsActive)
                {
                    PlayerController.Instance.StopFishing();
                    uiHandlerFishingMinigame.HideFishingUI();
                }

                // Set the next day to 6 AM
                TimeManager.Instance.SetNextDayBySix();

                // Activate the home grid and deactivate the current map
                homeGrid.SetActive(true);
                Collider2D collider = Physics2D.OverlapPoint(PlayerController.Instance.Rigidbody2D.position, LayerMask.GetMask("Confiner"));
                if (collider != null)
                {
                    collider.transform.root.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("No map found at the player's position.");
                }

                // Teleport the player to the bed position
                PlayerController.Instance.transform.position = bed.transform.position - new Vector3(0, 2f, 0);
                VirtualCameraConfiner.Instance.SetConfiner2D(bed.transform.position);
            }
        ));
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

    public void ResetStats()
    {
        AddHealth(50f);
        AddHunger(50f);
        AddThirst(50f);
    }

    public void PlayerDied()
    {
        health = 0f;
        hunger = 0f;
        thirst = 0f;
        OnStatsChanged?.Invoke();
    }

    public PlayerStatData ToSerializableData()
    {
        return new PlayerStatData
        {
            _health = health,
            _hunger = hunger,
            _thirst = thirst,
            _money = money
        };
    }

    public void LoadFromSerializableData(PlayerStatData data)
    {
        health = data._health;
        hunger = data._hunger;
        thirst = data._thirst;
        money = data._money;

        OnStatsChanged?.Invoke();
        OnMoneyChanged?.Invoke();
    }
}
