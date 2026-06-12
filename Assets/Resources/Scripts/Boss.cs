using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    public float fixedZPosition = 15f;
    public float moveAmplitudeX = 4f;
    public float moveAmplitudeY = 2f;
    public float moveFrequencyX = 1f;
    public float moveFrequencyY = 1.5f;
    public float bulletCooldown = 2f;
    public int ringsPerShot = 3;
    public int bulletsPerRing = 12;
    public float bulletSpeed = 5f;
    public float bulletLifetime = 5f;
    public int scoreTrigger = 20;
    public float timeTrigger = 30f;
    public bool useScoreTrigger = true;
    public bool useTimeTrigger;
    public int scoreValue = 10;
    public int maxHealth = 50;
    public Vector2 healthBarSize = new Vector2(3.2f, 0.4f);

    private Vector3 startPosition;
    private float nextShotTime;
    private int currentHealth;
    private Slider healthSlider;

    private void Start()
    {
        maxHealth = Mathf.Max(50, maxHealth);
        transform.position = new Vector3(transform.position.x, transform.position.y, fixedZPosition);
        startPosition = transform.position;
        currentHealth = maxHealth;
        CreateHealthBar();
    }

    private void Update()
    {
        MovePattern();
        TryShoot();
        DestroyIfOutOfView();
    }

    private void MovePattern()
    {
        float offsetX = Mathf.Sin(Time.time * moveFrequencyX) * moveAmplitudeX;
        float offsetY = Mathf.Cos(Time.time * moveFrequencyY) * moveAmplitudeY;
        transform.position = new Vector3(startPosition.x + offsetX, startPosition.y + offsetY, fixedZPosition);
    }

    private void TryShoot()
    {
        if (Time.time < nextShotTime)
        {
            return;
        }

        nextShotTime = Time.time + Mathf.Max(0.1f, bulletCooldown);
        FireCircularBurst();
    }

    private void FireCircularBurst()
    {
        for (int ring = 0; ring < Mathf.Max(1, ringsPerShot); ring++)
        {
            float speedMultiplier = 1f + (ring * 0.35f);
            float angleOffset = ring * (180f / Mathf.Max(1, bulletsPerRing));

            for (int i = 0; i < Mathf.Max(4, bulletsPerRing); i++)
            {
                float angle = angleOffset + (360f / Mathf.Max(4, bulletsPerRing)) * i;
                float radian = angle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), -1f).normalized;

                GameObject bulletObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bulletObject.name = "BossBullet";
                bulletObject.tag = "enemy";
                bulletObject.transform.position = transform.position;
                bulletObject.transform.localScale = Vector3.one * 0.35f;

                SphereCollider collider = bulletObject.GetComponent<SphereCollider>();
                collider.isTrigger = true;

                Rigidbody rigidbody = bulletObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;

                Renderer renderer = bulletObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial.color = Color.magenta;
                }

                BossBullet bossBullet = bulletObject.AddComponent<BossBullet>();
                bossBullet.direction = direction;
                bossBullet.speed = bulletSpeed * speedMultiplier;
                bossBullet.lifeTime = bulletLifetime;
            }
        }
    }

    private void DestroyIfOutOfView()
    {
        if (transform.position.z < -5f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("bullet"))
        {
            return;
        }

        Destroy(other.gameObject);
        TakeDamage(1);
    }

    private void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(1, damage));
        UpdateHealthBar();

        if (currentHealth > 0)
        {
            return;
        }

        if (GameLogic.Instance != null)
        {
            GameLogic.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }

    private void CreateHealthBar()
    {
        GameObject canvasObject = new GameObject("BossHealthCanvas", typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = new Vector3(0f, 1.8f, 0f);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(160f, 28f);
        canvasObject.transform.localScale = Vector3.one * 0.02f;

        GameObject sliderObject = new GameObject("BossHealthSlider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(canvasObject.transform, false);

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(healthBarSize.x * 50f, healthBarSize.y * 50f);

        healthSlider = sliderObject.GetComponent<Slider>();
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.value = 1f;
        healthSlider.direction = Slider.Direction.LeftToRight;
        healthSlider.transition = Selectable.Transition.None;

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(sliderObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image backgroundImage = backgroundObject.GetComponent<Image>();
        backgroundImage.color = new Color(0.15f, 0.1f, 0.1f, 0.9f);

        GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(3f, 3f);
        fillAreaRect.offsetMax = new Vector2(-3f, -3f);

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = new Color(0.8f, 0.15f, 0.15f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;

        healthSlider.targetGraphic = fillImage;
        healthSlider.fillRect = fillRect;
        healthSlider.handleRect = null;
    }

    private void UpdateHealthBar()
    {
        if (healthSlider == null || maxHealth <= 0)
        {
            return;
        }

        healthSlider.value = currentHealth / (float)maxHealth;
    }
}
