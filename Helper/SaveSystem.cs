using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    
    private string savePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        savePath = Application.persistentDataPath + "/save.json";
    }

    void ActivateInitialMap(Vector2 position)
    {
        Collider2D collider = Physics2D.OverlapPoint(position, LayerMask.GetMask("Confiner"));

        GameObject activeMap = collider.transform.root.gameObject;

        foreach (GameObject map in GameObject.FindGameObjectsWithTag("Map"))
        {
            map.SetActive(map == activeMap); // Deactivate all the other maps
        }
        
        if (activeMap == null)
        {
            Debug.LogWarning("No map found at the player's position.");
        }
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }

    void SaveGameDataToFile(GameData gameData)
    {
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved to " + savePath);
    }

    GameData LoadGameDataFromFile()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);

            GameData gameData = JsonUtility.FromJson<GameData>(json);
            return gameData;
        }
        else
        {
            GameData gameData = new GameData();

            gameData.inventory = PlayerInventory.Instance.InitializeInventory();
            gameData.playerStats = PlayerStats.Instance.InitializeStats();
            gameData.playerData = PlayerController.Instance.InitializePlayerData();
            gameData.timeData = TimeManager.Instance.InitializeTime();

            return gameData;
        }
    }

    public void SaveGame()
    {
        GameData gameData = new GameData();

        gameData.inventory = PlayerInventory.Instance.ToSerializableData();
        gameData.playerStats = PlayerStats.Instance.ToSerializableData();
        gameData.playerData = PlayerController.Instance.ToSerializableData();
        gameData.timeData = TimeManager.Instance.ToSerializableData();

        SaveGameDataToFile(gameData);
    }

    public void LoadGame()
    {
        GameData gameData = LoadGameDataFromFile();

        if (gameData != null)
        {
            PlayerInventory.Instance.LoadFromSerializableData(gameData.inventory);
            PlayerStats.Instance.LoadFromSerializableData(gameData.playerStats);
            PlayerController.Instance.LoadFromSerializableData(gameData.playerData);
            TimeManager.Instance.LoadFromSerializableData(gameData.timeData);
        }

        Vector2 playerPosition = PlayerController.Instance.Rigidbody2D.position;
        ActivateInitialMap(playerPosition); // Activate the initial map
        VirtualCameraConfiner.Instance.SetConfiner2D(playerPosition); // Set the confiner to the player's position
    }
}
