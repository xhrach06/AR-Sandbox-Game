using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates the UI image fill amount based on target health.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image fillImage;

    private void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponentInChildren<Image>();
    }

    private void Update()
    {
        if (health != null && fillImage != null)
            fillImage.fillAmount = health.GetHealthPercentage();
    }

    public void SetHealth(Health targetHealth)
    {
        health = targetHealth;
    }
}
