using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedController : MonoBehaviour, IInteractable
{
    [SerializeField] private UIHandler_SleepConfirm sleepConfirmUI;

    public void OnInteract()
    {
        if (TimeManager.Instance.Hour < 20 && TimeManager.Instance.Hour > 2)
        {
            UIHandler_Popup.Instance.CreatePopup("It's not night yet!");
            return;
        }

        sleepConfirmUI.ShowSleepConfirm();
    }
}
