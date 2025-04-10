using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_time : MonoBehaviour
{
    private Label timeLabel;
    private TimeManager timeManager;

    // Start is called before the first frame update
    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        timeLabel = uiDocument.rootVisualElement.Q<Label>("TimeLabel");

        timeManager = TimeManager.Instance;
        if (timeManager == null)
        {
            Debug.LogError("TimeManager instance not found.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeLabel.text = timeManager.GetFormattedTime();
    }
}
