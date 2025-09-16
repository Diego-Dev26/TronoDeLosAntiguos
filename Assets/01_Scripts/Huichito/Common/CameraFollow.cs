using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 8, -6);
    public float smooth = 10f;
    public float lookDownAngle = 20f;

    void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smooth);
        var rot = Quaternion.Euler(lookDownAngle, target.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * smooth);
    }
}
