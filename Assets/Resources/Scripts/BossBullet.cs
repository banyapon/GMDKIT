using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public Vector3 direction = Vector3.down;
    public float speed = 5f;
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (transform.position.z < -5f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (GameLogic.Instance != null)
        {
            GameLogic.Instance.RegisterPlayerHit(1);
        }

        Destroy(gameObject);
    }
}
