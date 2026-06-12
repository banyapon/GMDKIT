using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool followPlayer = true;
    public float minMoveSpeed = 3f;
    public float maxMoveSpeed = 7f;
    public int damage = 1;

    private Transform playerTarget;
    private float moveSpeed;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }

        moveSpeed = Random.Range(minMoveSpeed, Mathf.Max(minMoveSpeed, maxMoveSpeed));
    }

    private void Update()
    {
        Vector3 direction = followPlayer && playerTarget != null
            ? (playerTarget.position - transform.position).normalized
            : Vector3.back;

        transform.position += direction * moveSpeed * Time.deltaTime;
        DestroyIfOutOfView();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bullet"))
        {
            if (GameLogic.Instance != null)
            {
                GameLogic.Instance.AddScore();
            }

            Destroy(other.gameObject);
            SpawnDeathParticles();
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    private void DestroyIfOutOfView()
    {
        if (transform.position.z < -5f)
        {
            Destroy(gameObject);
        }
    }

    private void SpawnDeathParticles()
    {
        GameObject particleObject = new GameObject("EnemyDeathParticles");
        particleObject.transform.position = transform.position;

        ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startLifetime = 0.45f;
        main.startSpeed = 4f;
        main.startSize = 0.18f;
        main.startColor = Color.white;
        main.maxParticles = 24;
        main.loop = false;
        main.playOnAwake = false;

        var emission = particleSystem.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        var renderer = particleObject.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));

        particleSystem.Play();
        Destroy(particleObject, 1.5f);
    }
}
