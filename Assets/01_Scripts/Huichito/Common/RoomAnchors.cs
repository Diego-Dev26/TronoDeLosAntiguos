using UnityEngine;

[ExecuteAlways]
public class RoomAnchors : MonoBehaviour
{
    public Transform top, bottom, left, right;

    // Entrada de la nueva sala según el lado del SpawnPoint actual
    // 1=Bottom, 2=Top, 3=Left, 4=Right
    public Transform GetEntryForOpenSide(int openSide)
    {
        switch (openSide)
        {
            case 1: return top;    // spawn en bottom => pega TOP de la nueva
            case 2: return bottom; // spawn en top    => pega BOTTOM de la nueva
            case 3: return right;  // spawn en left   => pega RIGHT de la nueva
            case 4: return left;   // spawn en right  => pega LEFT de la nueva
            default: return null;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawAnchor(top, Color.cyan, "Top");
        DrawAnchor(bottom, Color.magenta, "Bottom");
        DrawAnchor(left, Color.yellow, "Left");
        DrawAnchor(right, Color.green, "Right");
    }

    void DrawAnchor(Transform t, Color c, string label)
    {
        if (!t) return;
        Gizmos.color = c;
        Gizmos.DrawSphere(t.position, 0.12f);
        // flecha en forward (+Z) -> DEBE salir hacia afuera del cuarto
        var f = t.forward; f.y = 0; f.Normalize();
        Gizmos.DrawLine(t.position, t.position + f * 0.9f);
#if UNITY_EDITOR
        UnityEditor.Handles.color = c;
        UnityEditor.Handles.Label(t.position + Vector3.up * 0.2f, label);
#endif
    }
#endif
}
