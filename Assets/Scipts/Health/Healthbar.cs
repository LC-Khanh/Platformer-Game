using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image currenthealthbar;
    [SerializeField] private Image totalhealthbar;
    [SerializeField] private Image healthBar;
    void Start()
    {
        totalhealthbar.fillAmount = playerHealth.currentHealth / 10;
    }

    // Update is called once per frame
    void Update()
    {
        currenthealthbar.fillAmount = playerHealth.currentHealth / 10;
    }

}
