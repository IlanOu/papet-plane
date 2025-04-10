using UnityEngine;

namespace Projectile
{
    public class BallBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform quadTransform;
        [HideInInspector] public float timer = 0;
    
        private Rigidbody rb;
        private BallBonus bonus;
    
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        
            // Appliquer le bonus à la balle
            if (bonus != null)
            {
                bonus.ApplyTo(gameObject);
            }
        }
    
        public void SetBonus(BallBonus newBonus)
        {
            bonus = newBonus;
        }
    
        void Update()
        {
            // Mettre à jour le timer
            timer += Time.deltaTime;
        
            // Orienter le quad dans la direction du mouvement
            UpdateQuadOrientation();
        
            // Appliquer la logique de mise à jour du bonus
            if (bonus != null)
            {
                bonus.UpdateBall(this);
            }
        }
    
        private void UpdateQuadOrientation()
        {
            if (rb != null && rb.linearVelocity.magnitude > 0.1f && quadTransform != null)
            {
                Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            
                if (horizontalVelocity.magnitude > 0.01f)
                {
                    float angleY = Mathf.Atan2(horizontalVelocity.x, horizontalVelocity.z) * Mathf.Rad2Deg;
                    quadTransform.rotation = Quaternion.Euler(90, angleY, 0);
                }
            }
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            if (bonus != null)
            {
                bonus.OnCollision(this, collision);
            }
        }
    }
}