using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statoToChange = new StatToChange();
    public int amountToChangeStat;

    public AttributesToChange attributesToChange = new AttributesToChange();
    public int amountToChangeAttribute;


    public bool UseItem()
    {
        if (statoToChange == StatToChange.health)
        {
            Health playerHealth = GameObject.FindWithTag("Player").GetComponent<Health>();
            if (playerHealth.currentHealth == playerHealth.maxHealth)
            {
                return false;
            }
            else
            {
                playerHealth.ChangeHealth(amountToChangeStat);
                return true;
            }
        }
        return false;
    }
    public enum StatToChange
    {
        none,
        health,
        mana,
        stamina
    };
    public enum AttributesToChange
    {
        none,
        strenght,
        defense,
        intelligence,
        aglity
    };
}
