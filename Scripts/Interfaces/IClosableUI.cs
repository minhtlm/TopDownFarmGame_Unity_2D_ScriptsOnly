using UnityEngine;

public abstract class IClosableUI : MonoBehaviour
{
    public static IClosableUI openingUI = null;
    public abstract void ShowUI();
    public abstract void CloseUI();
}
