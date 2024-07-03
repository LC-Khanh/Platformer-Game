using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Health playerHealth;

    private string saveFilePath;

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            position = playerTransform.position,
            health = playerHealth.currentHealth
        };

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game Saved");
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            playerTransform.position = saveData.position;
            playerHealth.currentHealth = saveData.health;
            playerHealth.ResetPlayer();
            Debug.Log("Game Loaded");
        }
        else
        {
            Debug.LogError("No save file found");
        }
    }

    [System.Serializable]
    private class SaveData
    {
        public Vector3 position;
        public float health;
    }
}
