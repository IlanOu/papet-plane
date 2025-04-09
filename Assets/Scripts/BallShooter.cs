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
    
    private BonusManager bonusManager;
    
    private void Awake()
    {
        // Important: obtenez le BonusManager du même GameObject (pas un singleton)
        bonusManager = GetComponent<BonusManager>();
        if (bonusManager == null)
        {
            bonusManager = gameObject.AddComponent<BonusManager>();
        }
    }
    
    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Instantiate ball at spawn point
            GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
            
            // Appliquer le bonus actuel
            BallBehaviour ballBehaviour = ball.GetComponent<BallBehaviour>();
            if (ballBehaviour != null)
            {
                ballBehaviour.SetBonus(bonusManager.GetCurrentBonus());
            }
            
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
                    OrientQuad(quadTransform, shootDirection);
                }
            }
        }
    }
    
    private void OrientQuad(Transform quadTransform, Vector3 direction)
    {
        Quaternion originalRotation = quadTransform.rotation;
        quadTransform.rotation = Quaternion.LookRotation(direction);
        
        // Reset X and Z rotation, keep only Y rotation
        Vector3 eulerAngles = quadTransform.eulerAngles;
        quadTransform.eulerAngles = new Vector3(originalRotation.eulerAngles.x, 
            eulerAngles.y, 
            originalRotation.eulerAngles.z);
    }
}