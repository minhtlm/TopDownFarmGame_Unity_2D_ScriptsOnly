using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_healthBar : MonoBehaviour
{
    public static UIHandler_healthBar instance { get; private set; }

    private VisualElement healthValue;
    private VisualElement energyValue;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        healthValue = uiDocument.rootVisualElement.Q<VisualElement>("HealthValue");
        energyValue = uiDocument.rootVisualElement.Q<VisualElement>("EnergyValue");
        SetHealthValue(1.0f);
        SetEnergyValue(1.0f);
    }

    public void SetHealthValue(float percentage)
    {
        healthValue.style.width = Length.Percent(percentage * 100);
    }

    public void SetEnergyValue(float percentage)
    {
        energyValue.style.width = Length.Percent(percentage * 100);
    }
}
