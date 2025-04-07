using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image fillImage;

    void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponentInChildren<Image>(); // auto assigns the first found image (likely Fill)
    }

    void Update()
    {
        if (health != null && fillImage != null)
        {
            fillImage.fillAmount = health.GetHealthPercentage();
        }
    }

    public void SetHealth(Health targetHealth)
    {
        health = targetHealth;
    }
}
