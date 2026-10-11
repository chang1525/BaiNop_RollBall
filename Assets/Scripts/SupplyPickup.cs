using UnityEngine;

public class SupplyPickup : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void Update()
    {
        transform.Rotate(Vector3.up * 90f * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        if (!gameManager.CanPlay) return;

        gameManager.CollectSupply();
        gameObject.SetActive(false);
    }
}
