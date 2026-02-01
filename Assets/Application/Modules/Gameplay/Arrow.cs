using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float maxDistance = 4f; // 2 tiles * gridSize(2)
    [SerializeField] private float fallSpeed = 5f;

    private Vector3 direction;
    private float distanceTraveled;
    private bool isFalling;

    public void Initialize(Vector3 flyDirection, float tileSize, int maxTiles)
    {
        direction = flyDirection.normalized;
        maxDistance = tileSize * maxTiles;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Update()
    {
        Vector3 velocity;

        if (isFalling)
        {
            velocity = direction * speed + Vector3.down * fallSpeed;
            transform.position += velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(velocity);
            if (transform.position.y < -0.2f)
                Destroy(gameObject);
            return;
        }

        velocity = direction * speed;
        float step = speed * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(velocity);
        distanceTraveled += step;

        if (distanceTraveled >= maxDistance)
        {
            isFalling = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.Kill(DeathReason.Arrow);
            Destroy(gameObject);
        }
    }
}