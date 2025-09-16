using UnityEngine;

public class DoorAnchors : MonoBehaviour
{
    // Coloca estos empties justo en el centro del hueco de cada puerta
    public Transform top;     // puerta norte
    public Transform bottom;  // puerta sur
    public Transform left;    // puerta oeste
    public Transform right;   // puerta este

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        DrawAnchor(top, Color.cyan, "Top");
        DrawAnchor(bottom, Color.yellow, "Bottom");
        DrawAnchor(left, Color.green, "Left");
        DrawAnchor(right, Color.magenta, "Right");
    }
    void DrawAnchor(Transform t, Color c, string label)
    {
        if (!t) return;
        Gizmos.color = c;
        Gizmos.DrawSphere(t.position, 0.15f);
        UnityEditor.Handles.Label(t.position + Vector3.up * 0.2f, label);
    }
#endif
}
