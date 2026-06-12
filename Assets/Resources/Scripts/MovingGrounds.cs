using UnityEngine;

public class MovingGrounds : MonoBehaviour
{
    public float scrollSpeed = 0.35f;

    private Renderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (cachedRenderer == null || cachedRenderer.material == null)
        {
            return;
        }

        float offset = Time.time * scrollSpeed;
        cachedRenderer.material.mainTextureOffset = new Vector2(0f, -offset);
    }
}
