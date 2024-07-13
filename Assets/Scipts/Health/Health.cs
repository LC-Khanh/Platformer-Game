using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float startingHealth;
    public float currentHealth { get;  set; }
    public float maxHealth { get; set; }
    bool dead;
    Animator anim;
    PlayerMovement playerMovement;
    private SaveSystem saveSystem;

    private void Start()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        currentHealth = startingHealth;
        maxHealth = startingHealth;
        saveSystem = FindObjectOfType<SaveSystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            TakeDamage(1);
    }

    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hurt");
        }
        else
        {
            if (!dead)
            {
                Die();
            }
        }
    }

    public void ChangeHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, startingHealth);

        if (currentHealth <= 0 && !dead)
        {
            Die();
        }
        else
        {
            anim.SetTrigger("Hurt");
        }
    }
    public void Die()
    {
        dead = true;
        anim.SetTrigger("Die");
        playerMovement.enabled = false;
        playerMovement.rb.velocity = Vector2.zero;
        playerMovement.rb.isKinematic = true;

        StartCoroutine(Respawn());
    }

    public void ResetPlayer()
    {
        dead = false;
        currentHealth = startingHealth;
        anim.ResetTrigger("Die");
        anim.Play("Idle");
        playerMovement.enabled = true;
        playerMovement.rb.isKinematic = false;
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2f);
        saveSystem.LoadGame();
    }
}
