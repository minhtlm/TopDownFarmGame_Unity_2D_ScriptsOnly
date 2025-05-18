using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public PlayerData playerData;
    public List<ItemStackData> inventory;
    public PlayerStatData playerStats;
    public TimeData timeData;
}
