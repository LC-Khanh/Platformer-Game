using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private SaveSystem saveSystem;

    private void Start()
    {
        saveSystem = FindObjectOfType<SaveSystem>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            saveSystem.SaveGame();
        }
    }
}
