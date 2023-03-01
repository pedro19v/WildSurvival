﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    protected Animator animator;
    protected EntityMovement movement;

    public string entityName;

    public float maxHealth;
    [ReadOnly] public CharacterStats stats;
    [ReadOnly] public float health;

    public GameObject healthBarPrefab;
    protected GameObject healthBar;

    protected void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<EntityMovement>();
        stats = GetComponent<CharacterStats>();
        health = maxHealth;
        CreateHealthBar();
        OnAwake();
    }

    abstract protected void OnAwake();

    protected void Start()
    {
        FullRestore();
        OnStart();
    }

    abstract protected void OnStart();

    private void CreateHealthBar()
    {
        healthBar = Instantiate(healthBarPrefab);
        healthBar.transform.position = new Vector3(0, 2, 0);
        healthBar.transform.SetParent(transform, false);
    }

    private void FixedUpdate()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        Transform midRect = healthBar.transform.Find("Middle Rect");
        Transform frontRect = healthBar.transform.Find("Front Rect");

        float percentage = Mathf.Max(health / maxHealth, 0);
        float newPosition = midRect.localScale.x * (percentage - 1) / 2;
        float newWidth = midRect.localScale.x * percentage;
        frontRect.localScale = new Vector3(newWidth, midRect.localScale.y);
        frontRect.localPosition = new Vector3(newPosition, midRect.localPosition.y);

        frontRect.GetComponent<SpriteRenderer>().color = ChooseBarColor(percentage);
    }

    public Color ChooseBarColor(float percentage)
    {
        int index = Mathf.FloorToInt(4 * percentage);
        return Colors.HP[index];
    }
    

    public virtual void TakeDamage(float damage)
    {
        // caso um ativista morto seja atacado evita que onDeath seja chamado novamente
        if (health > 0)
        {
            health -= damage;
            if (health <= 0)
                OnDeath();
        }
    }

    public virtual void TakeDamage(float damage, Entity attacker)
    {
        if (health > 0)
        {
            health -= damage;
            if (health <= 0)
                OnDeath(attacker);
        }
    }

    protected virtual void OnDeath() { }

    protected virtual void OnDeath(Entity killer)
    {
        OnDeath();
    }

    public virtual void Knock(float knockTime, float damage)
    {
        StartCoroutine(movement.KnockCo(knockTime));
        TakeDamage(damage);
    }

    public virtual void Knock(float knockTime, float damage, Collider2D attackerCol)
    {
        Knock(knockTime, damage);
    }

    public virtual void FullRestore()
    {
        health = maxHealth;
    }

    public virtual void Heal(float healValue) {
        health = Mathf.Min(health + healValue, maxHealth);
    }

    public virtual float GetAttack()
    {
        return stats.damage.GetValue();
    }

    public bool IsOtherEntity(GameObject gameObject)
    {
        return (gameObject.CompareTag("player") ||
                gameObject.CompareTag("dummy") ||
                gameObject.CompareTag("rhino") ||
                gameObject.CompareTag("enemy")) &&
                !(gameObject == this.gameObject);
    }

    public static bool AreOpponents(Entity obj1, Entity obj2)
    {
        return GetTeam(obj1) != GetTeam(obj2);
    }

    private static int GetTeam(Entity entity)
    {
        switch (entity.tag)
        {
            case "enemy":
                return 0; // Evil
            case "dummy":
                return 1; // Neutral
            default:
                return 2; // Good
        }
    }

    // Get the collider which collides with the environment (tiles in the map)
    public BoxCollider2D GetEnvironmentCollider()
    {
        BoxCollider2D[] boxCollider2Ds = GetComponents<BoxCollider2D>();

        foreach (BoxCollider2D boxCollider2D in boxCollider2Ds)
            if (!boxCollider2D.isTrigger)
                return boxCollider2D;

        return null;
    }
}
