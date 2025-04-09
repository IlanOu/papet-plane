using UnityEngine;

public class PaperPlane : MonoBehaviour
{
    [Header("Flight Settings")]
    public float speed = 5f;
    public float arcHeight = 2f;
    public float paintInterval = 0.2f;
    public int team = 1;

    private Vector3 startPoint;
    private Vector3 targetPoint;
    private float flightProgress = 0f;
    private float nextPaintTime;

    private bool isFlying = true;

    public GameObject impactPaintZonePrefab;

    public void Init(Vector3 start, Vector3 target, int teamId)
    {
        startPoint = start;
        targetPoint = target;
        team = teamId;
        transform.position = start;
    }

    void Update()
    {
        if (!isFlying) return;

        flightProgress += Time.deltaTime * speed;
        float t = Mathf.Clamp01(flightProgress);

        // Mouvement en arc
        Vector3 currentPos = Vector3.Lerp(startPoint, targetPoint, t);
        float height = Mathf.Sin(t * Mathf.PI) * arcHeight;
        transform.position = new Vector3(currentPos.x, currentPos.y + height, currentPos.z);

        // Peinture pendant le vol
        if (Time.time >= nextPaintTime)
        {
            nextPaintTime = Time.time + paintInterval;
            // PaintGrid.Instance.PaintCellAtWorldPos(transform.position, team);
        }

        // Fin du vol
        if (t >= 1f)
        {
            isFlying = false;
            OnImpact();
        }
    }

    void OnImpact()
    {
        if (impactPaintZonePrefab)
        {
            // Instantiate(impactPaintZonePrefab, transform.position, Quaternion.identity)
            //     .GetComponent<PaintZone>()
            //     .Paint(team);
        }

        Destroy(gameObject);
    }
}