using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraIsometricFollow : MonoBehaviour
{
    public Transform target;

    [Header("Vista isométrica")]
    [Range(0f, 89f)] public float pitch = 35f; // inclinación
    [Range(0f, 360f)] public float yaw = 45f;  // giro horizontal
    public float distance = 18f;               // distancia al objetivo
    public Vector3 worldOffset = Vector3.zero; // ajuste fino (si tu player es alto, etc.)

    [Header("Seguimiento")]
    [Range(0.01f, 1f)] public float smoothTime = 0.15f;
    public bool lockYToTarget = true; // si el player sube/baja, ¿acompañar Y?

    [Header("Proyección")]
    public bool useOrthographic = false;
    public float orthographicSize = 8f;

    Vector3 velocity;

    void Awake()
    {
        var cam = GetComponent<Camera>();
        cam.orthographic = useOrthographic;
        if (useOrthographic) cam.orthographicSize = orthographicSize;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + worldOffset;

        // Coloca la cámara "detrás" del pivot según el ángulo
        Vector3 desiredPos = pivot + rot * (Vector3.back * distance);

        if (lockYToTarget)
            desiredPos.y = Mathf.Max(desiredPos.y, pivot.y + 0.1f);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothTime);
        transform.rotation = rot;
    }
}