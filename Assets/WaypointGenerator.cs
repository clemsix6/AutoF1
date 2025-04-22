using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(MeshCollider))]
public class WaypointGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int numPoints = 100;
    public GameObject waypointPrefab;
    [Tooltip("Hauteur max au‑dessus de la route pour les rayons verticaux")]
    public float rayHeight = 10f;
    [Tooltip("Nb d’échantillons radiaux (qualité du scan)")]
    public int radialSamples = 50;
    public Transform waypointsParent;

    MeshCollider trackCollider;
    Vector3 center;
    float maxRadius;

    void OnValidate()
    {
        trackCollider = GetComponent<MeshCollider>();
        if (trackCollider != null)
        {
            center = trackCollider.bounds.center;
            maxRadius = Mathf.Max(
                trackCollider.bounds.extents.x,
                trackCollider.bounds.extents.z
            ) + 1f;
        }
    }

    [ContextMenu("Generate Waypoints")]
    public void GenerateWaypoints()
    {
        // Prérequis
        if (trackCollider == null || waypointPrefab == null || waypointsParent == null)
        {
            Debug.LogError("[WaypointGenerator] Vérifie MeshCollider, prefab et parent.");
            return;
        }

        Debug.Log($"[WaypointGenerator] Début génération ({numPoints} pts)…");

        // Clear anciens
        for (int i = waypointsParent.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(waypointsParent.GetChild(i).gameObject);
#else
            DestroyImmediate(waypointsParent.GetChild(i).gameObject);
#endif
        }

        center = trackCollider.bounds.center;
        int created = 0;

        for (int i = 0; i < numPoints; i++)
        {
            float angle = 2 * Mathf.PI * i / numPoints;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            // Liste des indices validés
            int firstIdx = -1, lastIdx = -1;
            for (int j = 0; j <= radialSamples; j++)
            {
                float r = maxRadius * j / radialSamples;
                Vector3 samplePos = center + dir * r + Vector3.up * rayHeight;

                if (Physics.Raycast(
                    origin: samplePos,
                    direction: Vector3.down,
                    out RaycastHit hit,
                    maxDistance: rayHeight * 2f
                ) && hit.collider == trackCollider)
                {
                    if (firstIdx < 0) firstIdx = j;
                    lastIdx = j;
                }
            }

            // Si on a bien une bande de hits
            if (firstIdx >= 0 && lastIdx >= firstIdx)
            {
                float midIdx = (firstIdx + lastIdx) * 0.5f;
                float rMid   = maxRadius * midIdx / radialSamples;
                // Re-raycast pour la hauteur exacte
                Vector3 aboveMid = center + dir * rMid + Vector3.up * rayHeight;
                if (Physics.Raycast(
                    origin: aboveMid,
                    direction: Vector3.down,
                    out RaycastHit midHit,
                    maxDistance: rayHeight * 2f
                ) && midHit.collider == trackCollider)
                {
                    // Instanciation
#if UNITY_EDITOR
                    GameObject wp = (GameObject)PrefabUtility.InstantiatePrefab(waypointPrefab, waypointsParent);
                    Undo.RegisterCreatedObjectUndo(wp, "Create Waypoint");
                    wp.transform.position = midHit.point;
                    wp.transform.rotation = Quaternion.LookRotation(dir);
#else
                    Instantiate(waypointPrefab, midHit.point, Quaternion.LookRotation(dir), waypointsParent);
#endif
                    created++;
                }
            }
        }

        Debug.Log($"[WaypointGenerator] Créés {created}/{numPoints} waypoints.");
#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
    }
}
