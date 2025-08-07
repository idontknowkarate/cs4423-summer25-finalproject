using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public PlayerMovement playerMovement;
    public PlayerShooting playerShooting;

    [Header("UI Elements")]
    public Image healthBarFill;
    public Image boostGaugeFill;
    public TMP_Text missileText;
    public TMP_Text scoreLabelText;
    public TMP_Text scoreCountText;
    public TMP_Text timerLabelText;
    public TMP_Text timerCountText;

    [Header("Missile Cooldown Icons")]
    public List<Image> missileCooldownIcons;

    private float timer = 0f;

    void Update()
    {
        UpdateHealthUI();
        UpdateBoostUI();
        UpdateMissileUI();
        UpdateMissileCooldownUI();
        UpdateScoreUI();
        UpdateTimerUI();
    }

    void UpdateHealthUI()
    {
        if (playerHealth && healthBarFill)
        {
            float healthPercent = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
            healthBarFill.fillAmount = healthPercent;
        }
    }

    void UpdateBoostUI()
    {
        if (playerMovement && boostGaugeFill)
        {
            float boostPercent = playerMovement.CurrentBoost / playerMovement.MaxBoost;
            boostGaugeFill.fillAmount = boostPercent;
        }
    }

    void UpdateMissileUI()
    {
        if (playerShooting && missileText)
        {
            missileText.text = $">MSL       {playerShooting.ReserveMissiles}";
        }
    }

    void UpdateMissileCooldownUI()
    {
        if (playerShooting == null || missileCooldownIcons == null)
            return;

        for (int i = 0; i < missileCooldownIcons.Count; i++)
        {
            if (i >= playerShooting.PylonCount)
                break;

            Image icon = missileCooldownIcons[i];
            if (icon == null || !icon.gameObject.activeInHierarchy)
                continue;

            float timeRemaining = playerShooting.GetMissileCooldown(i);
            float fillAmount = Mathf.Clamp01(1f - (timeRemaining / playerShooting.missileCooldown));
            icon.fillAmount = fillAmount;
        }
    }

    void UpdateScoreUI()
    {
        if (scoreLabelText && scoreCountText)
        {
            scoreLabelText.text = "SCORE";
            scoreCountText.text = ScoreManager.Instance.GetScore().ToString("000");
        }
    }

    void UpdateTimerUI()
    {
        timer += Time.deltaTime;
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        int milliseconds = Mathf.FloorToInt((timer * 100f) % 100f);

        if (timerLabelText && timerCountText)
        {
            timerLabelText.text = "TIME";
            timerCountText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
    }

    public void ForceHealthBarEmpty()
    {
        if (healthBarFill)
            healthBarFill.fillAmount = 0f;
    }
}
