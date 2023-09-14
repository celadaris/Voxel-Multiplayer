using UnityEngine;

public class FreezeRotation : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    private void Awake()
    {
        rb.freezeRotation = true;
    }
}
