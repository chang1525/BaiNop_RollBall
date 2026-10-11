using UnityEngine;

public class SupplyPickup : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float pickupDistance = 1.2f;
    private PlayerController player;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * 90f * Time.deltaTime);

        if (!gameManager.CanPlay || player == null) return;

        Vector3 distance = player.transform.position - transform.position;
        distance.y = 0f;
        if (distance.sqrMagnitude <= pickupDistance * pickupDistance)
            Collect();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        Collect();
    }

    private void Collect()
    {
        if (!gameManager.CanPlay || !gameObject.activeSelf) return;
        gameManager.CollectSupply();
        gameObject.SetActive(false);
    }
}
