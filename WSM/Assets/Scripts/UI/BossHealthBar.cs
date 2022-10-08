using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public float changeSpeed;

    [HideInInspector]
    public GameObject boss;
    [HideInInspector]
    public float maxHealth;
    [HideInInspector]
    public float health;

    private Slider slider;


    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = 0;
    }

    void Update()
    {
        slider.value = Mathf.Lerp(slider.value, health / maxHealth, changeSpeed * Time.deltaTime);
    }
}
