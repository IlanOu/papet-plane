using UnityEngine;

public class BallOrientationHandler : MonoBehaviour
{
    [SerializeField] private Transform quadTransform;
    [SerializeField] private float lifeTime = 5f; // Durée de vie en secondes
    [SerializeField] [Range(0, 1)] private float bounciness = 0.8f; // Facteur de rebond
    
    private Rigidbody rb;
    private float timer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (quadTransform == null)
        {
            quadTransform = transform.Find("DirectionQuad");
        }
        
        // Créer et appliquer un Physic Material pour le rebond
        ConfigureBounciness();
    }
    
    void ConfigureBounciness()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            PhysicsMaterial physicMaterial = new PhysicsMaterial("Bouncy");
            physicMaterial.bounciness = bounciness;
            physicMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            physicMaterial.bounceCombine = PhysicsMaterialCombine.Maximum;
            collider.material = physicMaterial;
        }
    }
    
    void Update()
    {
        // Gestion de l'orientation
        if (rb != null && rb.linearVelocity.magnitude > 0.1f && quadTransform != null)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            
            if (horizontalVelocity.magnitude > 0.01f)
            {
                float angleY = Mathf.Atan2(horizontalVelocity.x, horizontalVelocity.z) * Mathf.Rad2Deg;
                quadTransform.rotation = Quaternion.Euler(90, angleY-90, 0);
            }
        }
        
        // Gestion du timer d'autodestruction
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}