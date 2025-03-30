using UnityEngine;

public interface IClosableUI
{
    public static IClosableUI openingUI = null;
    void CloseUI();
}
