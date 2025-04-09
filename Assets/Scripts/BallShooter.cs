using UnityEngine;
using UnityEngine.InputSystem;

public class BallShooter : MonoBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float shootForce = 10f;
    
    [Header("Camera")]
    [SerializeField] private GameObject playerCamera;
    
    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Instantiate ball at spawn point
            GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
            
            // Get shooting direction from camera
            Vector3 shootDirection = playerCamera.transform.forward;
            
            // Get rigidbody and apply force
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
                
                // Orient the quad on the ball to face the direction of travel
                Transform quadTransform = ball.transform.Find("DirectionQuad");
                if (quadTransform != null)
                {
                    // Set the quad to look in the direction of travel
                    // but only rotate on Y axis
                    Quaternion originalRotation = quadTransform.rotation;
                    quadTransform.rotation = Quaternion.LookRotation(shootDirection);
                    
                    // Reset X and Z rotation, keep only Y rotation
                    Vector3 eulerAngles = quadTransform.eulerAngles;
                    quadTransform.eulerAngles = new Vector3(originalRotation.eulerAngles.x, 
                        eulerAngles.y, 
                        originalRotation.eulerAngles.z);
                }
            }
        }
    }
}