using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    public Image cooldownMask;
    public Text cooldownText;

    private float cooldownRemain;
    private float cooldownTotal;
    private bool isCooldown;

    public void StartCooldown(float cd)
    {
        cooldownTotal = cd;
        cooldownRemain = cd;
        isCooldown = true;

        cooldownMask.fillAmount = 1;
        cooldownText.text = Mathf.Ceil(cd).ToString();
    }

    void Update()
    {
        if (!isCooldown) return;

        cooldownRemain -= Time.deltaTime;

        if (cooldownRemain > 0)
        {
            cooldownMask.fillAmount = cooldownRemain / cooldownTotal;
            cooldownText.text = Mathf.Ceil(cooldownRemain).ToString();
        }
        else
        {
            cooldownMask.fillAmount = 0;
            cooldownText.text = "";
            isCooldown = false;
        }
    }
}


