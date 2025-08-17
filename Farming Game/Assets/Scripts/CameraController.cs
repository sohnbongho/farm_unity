using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target;
    public Transform clampMin;
    public Transform clampMax;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = FindAnyObjectByType<PlayerController>().transform;
        clampMin.SetParent(null);
        clampMax.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, clampMin.position.x, clampMax.position.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, clampMin.position.y, clampMax.position.y);

        transform.position = clampedPosition;

    }
}
