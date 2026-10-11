using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public OrbCollectorGame game;

    private void OnTriggerEnter(Collider other)
    {
        PickupItem item = other.GetComponent<PickupItem>();
        if (item != null) game.Collect(item);
    }
}
