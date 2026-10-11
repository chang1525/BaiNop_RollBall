using UnityEngine;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.25f;
    [SerializeField] private AudioClip deathSound;

    private Transform player;
    private GameManager gameManager;
    private float nextAttack;

    public void Setup(Transform target, GameManager manager)
    {
        player = target;
        gameManager = manager;
        nextAttack = Time.time + 2f;
    }

    private void Update()
    {
        if (player == null || !gameManager.CanPlay) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance > 1.35f)
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;

        if (distance > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);

        if (distance < 1.65f && Time.time >= nextAttack)
        {
            nextAttack = Time.time + 2f;
            gameManager.TakeDamage(3);
        }
    }

    public void Die()
    {
        if (gameManager.IsFinished) return;
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 0.5f);
        gameManager.ZombieKilled(this);
        Destroy(gameObject);
    }
}
