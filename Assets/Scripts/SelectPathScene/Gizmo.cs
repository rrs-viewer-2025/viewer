using UnityEngine;

public class Gizmo : MonoBehaviour
{
    public Vector3 center = Vector3.zero;
    public float radius = 30f;
    public Color gizmoColor = Color.green;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(center, radius);
    }
}
