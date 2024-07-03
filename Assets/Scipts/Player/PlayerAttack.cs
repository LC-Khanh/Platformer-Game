using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform arrowPoint;
    [SerializeField] private GameObject[] arrows;
    Animator aim;
    PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;

    void Start()
    {
        aim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && cooldownTimer > attackCooldown && playerMovement.CanAttack())
            Attack();

        cooldownTimer += Time.deltaTime;
    }

    public void Attack()
    {
        aim.SetTrigger("IsAttacking");
        cooldownTimer = 0;
    }

    public void ShootArrow() // This function will be called by the Animation Event
    {
        if (arrows.Length > 0 && arrows[0] != null)
        {
            arrows[FindArrow()].transform.position = arrowPoint.position;
            arrows[FindArrow()].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));

            Debug.Log("Arrow shot");
        }
        else
        {
            Debug.LogError("Arrows array is empty or arrow[0] is null");
        }
    }

    private int FindArrow()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            if (!arrows[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}
