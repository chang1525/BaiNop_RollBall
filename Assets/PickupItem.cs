using UnityEngine;

public class PickupItem : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
    }
}
