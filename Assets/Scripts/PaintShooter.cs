using UnityEngine;
using UnityEngine.InputSystem;

public class PaintShooter : MonoBehaviour
{
    public GameObject planePrefab;
    public Transform shootPoint;
    public float shootDistance = 10f;
    public int team = 1;

    public void OnShoot(InputAction.CallbackContext context)
    {
        Debug.Log("On Shoot");
        if (context.started)
        {
            Vector3 target = shootPoint.position + shootPoint.forward * shootDistance;
            GameObject plane = Instantiate(planePrefab, shootPoint.position, Quaternion.identity);
            plane.GetComponent<PaperPlane>().Init(shootPoint.position, target, team);
        }
    }
}