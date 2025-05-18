using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class GlobalLightController : MonoBehaviour
{
    private enum LightPhase
    {
        Dawn,
        Day,
        Dusk,
        Night
    }

    private Color dawnColor = new Color(0.5f, 0.55f, 0.6f, 1f); // Light red color for dawn
    private Color dayColor = new Color(1f, 1f, 1f, 1f); // White color for day
    private Color duskColor = new Color(1f, 0.6f, 0.3f, 1f); // Light red color for dusk
    private Color nightColor = new Color(0.2f, 0.2f, 0.5f, 1f); // Dark blue color for night

    private int dawnHour = 5; // 5 am - Dawn
    private int dayHour = 7; // 7 am - Day
    private int duskHour = 17; // 5 pm - Dusk
    private int nightHour = 19; // 7 pm - Night

    private const float transitionDuration = 3f; // Duration for color transition
    private Light2D globalLight;
    private LightPhase currentPhase = LightPhase.Night;

    public static GlobalLightController Instance { get; private set; }

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

    // Start is called before the first frame update
    void Start()
    {
        globalLight = GetComponent<Light2D>();
        globalLight.color = GetColorForPhase(currentPhase); // Set initial color
    }

    LightPhase GetPhaseByHour(int hour)
    {
        if (hour >= dawnHour && hour < dayHour)
        {
            return LightPhase.Dawn;
        }
        else if (hour >= dayHour && hour < duskHour)
        {
            return LightPhase.Day;
        }
        else if (hour >= duskHour && hour < nightHour)
        {
            return LightPhase.Dusk;
        }
        else
        {
            return LightPhase.Night;
        }
    }

    Color GetColorForPhase(LightPhase phase)
    {
        return phase switch
        {
            LightPhase.Dawn => dawnColor,
            LightPhase.Day => dayColor,
            LightPhase.Dusk => duskColor,
            LightPhase.Night => nightColor,
            _ => nightColor // Night color if phase is not recognized
        };
    }

    public void UpdateLightByHour(int currentHour, float duration = transitionDuration)
    {
        LightPhase newPhase = GetPhaseByHour(currentHour);
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            Color targetColor = GetColorForPhase(newPhase);

            DOTween.To(() => globalLight.color, x => globalLight.color = x, targetColor, duration)
                .SetEase(Ease.Linear); // Smooth transition to the new color
        }
    }
}
