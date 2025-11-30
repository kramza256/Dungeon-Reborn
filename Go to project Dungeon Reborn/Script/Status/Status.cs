using UnityEngine;

[System.Serializable]
public class Status
{
    [Header("Health")]
    public float maxHP = 100f;
    public float hp = 100f;

    [Header("Stamina")]
    public float maxStamina = 50f;
    public float stamina = 50f;
    public float staminaRegenRate = 5f;
    public float staminaDrainRate = 10f;

    [Header("Combat")]
    public float damage = 10f;

    [Header("Movement")]
    public float speed = 5f;
    public float sprintSpeed = 8f;

    public void TakeDamage(float amount)
    {
        hp = Mathf.Max(0f, hp - amount);
    }

    public void Heal(float amount)
    {
        hp = Mathf.Min(maxHP, hp + amount);
    }

    public void DrainStamina(float amount)
    {
        stamina = Mathf.Max(0f, stamina - amount);
    }

    public void RegenStamina(float deltaTime)
    {
        stamina = Mathf.Min(maxStamina, stamina + staminaRegenRate * deltaTime);
    }

    public float CurrentSpeed(bool sprinting)
    {
        if (sprinting && stamina > 0)
            return sprintSpeed;

        return speed;
    }
}
