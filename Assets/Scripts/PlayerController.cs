using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioSource soundEffects;
    [SerializeField] private AudioClip gunshot;
    [SerializeField] private Material bulletMaterial;
    [SerializeField] private float moveSpeed = 6f;

    private CharacterController characterController;
    private float nextShot;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (gameManager.IsFinished) return;

        Move();
        AimAtMouse();

        if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) && Time.time >= nextShot)
        {
            Shoot();
        }
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical);
        characterController.Move(Vector3.ClampMagnitude(direction, 1f) * moveSpeed * Time.deltaTime);
    }

    private void AimAtMouse()
    {
        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        float distance;

        if (ground.Raycast(ray, out distance))
        {
            Vector3 direction = ray.GetPoint(distance) - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction), 15f * Time.deltaTime);
            }
        }
    }

    private void Shoot()
    {
        nextShot = Time.time + 0.28f;
        soundEffects.PlayOneShot(gunshot, 0.5f);

        Vector3 start = transform.position + Vector3.up * 1.1f + transform.forward * 0.65f;
        Vector3 end = start + transform.forward * 25f;
        RaycastHit hit;

        if (Physics.Raycast(start, transform.forward, out hit, 25f))
        {
            end = hit.point;
            ZombieController zombie = hit.collider.GetComponentInParent<ZombieController>();
            if (zombie != null) zombie.Die();
        }

        GameObject bulletLine = new GameObject("Bullet Line");
        LineRenderer line = bulletLine.AddComponent<LineRenderer>();
        line.material = bulletMaterial;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.09f;
        line.endWidth = 0.015f;
        Destroy(bulletLine, 0.12f);
    }
}
