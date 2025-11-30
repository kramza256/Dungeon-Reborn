using UnityEngine;

public class Potion : Item
{
    public int AmountHealth = 20;
    public AudioClip PotionSound;
    public override void OnCollect(Player player)
    {
        base.OnCollect(player);
        player.Heal(AmountHealth);
        Destroy(gameObject);
        SoundManager.instance.PlaySFX(PotionSound);
    }
}
