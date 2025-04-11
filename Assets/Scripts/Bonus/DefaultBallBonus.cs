using Player;
using Projectile;
using Spawning;
using UnityEngine;

namespace Bonus
{
    public class DefaultBallBonus : BallBonus
    {
        [Header("Paramètres de base")]
        [Tooltip("Durée de vie de l'avion en secondes")]
        public float lifeTime = 5f;
        
        [Header("Aim Assist")]
        [Tooltip("Activer l'aide à la visée")]
        public bool aimAssistEnabled = true;
        [Tooltip("Rayon de détection des cibles potentielles")]
        public float targetDetectionRadius = 50f;
        [Tooltip("Force de l'aide à la visée (0-1, où 1 est une correction complète)")]
        [Range(0, 1)]
        public float aimAssistStrength = 1f;
        [Tooltip("Vitesse à laquelle l'aide à la visée ajuste la trajectoire")]
        public float aimAssistSpeed = 4f;
        [Tooltip("Tag des objets pouvant être ciblés")]
        public string targetTag = "Player";
        [Tooltip("Temps minimum avant que l'aim assist ne s'active (en secondes)")]
        public float aimAssistDelay = 0.2f;
        [Tooltip("Angle maximum (en degrés) pour que l'aim assist considère une cible")]
        public float maxAimAssistAngle = 60f;
        
        private Transform _currentTarget;
        private float _timeSinceSpawn;
        
        public override void ApplyTo(GameObject ball) 
        {
            // Réinitialiser les variables au moment où le bonus est appliqué
            _currentTarget = null;
            _timeSinceSpawn = 0f;
            
            Debug.Log("Aim Assist initialized. Enabled: " + aimAssistEnabled);
        }

        public override void UpdateBall(BallBehaviour ball)
        {
            // Mettre à jour le timer
            _timeSinceSpawn += Time.deltaTime;
            
            // Détruire la balle si sa durée de vie est écoulée
            if (ball.timer >= lifeTime)
            {
                GameObject.Destroy(ball.gameObject);
                return;
            }
            
            // Appliquer l'aide à la visée si activée et après le délai initial
            if (aimAssistEnabled && _timeSinceSpawn > aimAssistDelay)
            {
                ApplyAimAssist(ball);
            }
        }
        
        private void ApplyAimAssist(BallBehaviour ball)
        {
            // Ne pas cibler le propriétaire de la balle
            int ownerIndex = ball.ownerIndex;
            
            // Si pas de cible actuelle ou si elle est trop loin, chercher une nouvelle cible
            if (_currentTarget == null || Vector3.Distance(ball.transform.position, _currentTarget.position) > targetDetectionRadius)
            {
                _currentTarget = FindBestTarget(ball, ownerIndex);
            }
            
            // Si une cible est trouvée, ajuster la trajectoire
            if (_currentTarget != null)
            {
                AdjustTrajectory(ball, _currentTarget);
            }
        }
        
        private Transform FindBestTarget(BallBehaviour ball, int ownerIndex)
        {
            // Trouver tous les objets avec le tag cible dans le rayon de détection
            Collider[] potentialTargets = Physics.OverlapSphere(ball.transform.position, targetDetectionRadius);
            
            Transform bestTarget = null;
            float bestScore = float.MinValue;
            
            foreach (Collider targetCollider in potentialTargets)
            {
                // Vérifier si c'est une cible valide (tag correct)
                if (targetCollider.CompareTag(targetTag))
                {
                    // Vérifier si ce n'est pas le propriétaire de la balle
                    PlayerController playerController = targetCollider.GetComponent<PlayerController>();
                    if (playerController != null && playerController.playerIndex == ownerIndex)
                    {
                        continue; // Ignorer le propriétaire
                    }
                    
                    // Calculer la direction vers la cible
                    Vector3 directionToTarget = (targetCollider.transform.position - ball.transform.position).normalized;
                    
                    // Obtenir la direction actuelle de la balle
                    Rigidbody rb = ball.GetComponent<Rigidbody>();
                    if (rb == null) continue;
                    
                    Vector3 currentDirection = rb.linearVelocity.normalized;
                    
                    // Calculer l'angle entre la direction actuelle de la balle et la direction vers la cible
                    float angle = Vector3.Angle(currentDirection, directionToTarget);
                    
                    // Ignorer les cibles qui sont trop loin de la trajectoire actuelle
                    if (angle > maxAimAssistAngle)
                    {
                        continue;
                    }
                    
                    // Calculer la distance à la cible
                    float distance = Vector3.Distance(ball.transform.position, targetCollider.transform.position);
                    
                    // Calculer un score basé sur l'angle et la distance (préférer les cibles proches et dans la direction)
                    float score = (targetDetectionRadius - distance) * (maxAimAssistAngle - angle);
                    
                    // Mettre à jour la meilleure cible si ce score est meilleur
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = targetCollider.transform;
                        
                        Debug.Log($"Found potential target: {targetCollider.name}, Score: {score}, Angle: {angle}, Distance: {distance}");
                    }
                }
            }
            
            if (bestTarget != null)
            {
                Debug.Log($"Selected best target: {bestTarget.name}");
            }
            else
            {
                Debug.Log("No suitable target found");
            }
            
            return bestTarget;
        }
        
        private void AdjustTrajectory(BallBehaviour ball, Transform target)
        {
            // Obtenir le Rigidbody de la balle
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb == null) return;
            
            // Calculer la direction vers la cible
            Vector3 directionToTarget = (target.position - ball.transform.position).normalized;
            
            // Calculer la direction actuelle de la balle
            Vector3 currentDirection = rb.linearVelocity.normalized;
            
            // Interpoler entre la direction actuelle et la direction vers la cible
            Vector3 newDirection = Vector3.Lerp(currentDirection, directionToTarget, aimAssistStrength * Time.deltaTime * aimAssistSpeed);
            newDirection.Normalize();
            
            // Appliquer la nouvelle direction tout en conservant la vitesse
            float currentSpeed = rb.linearVelocity.magnitude;
            rb.linearVelocity = newDirection * currentSpeed;
            
            // Visualiser l'aide à la visée (pour le débogage)
            Debug.DrawLine(ball.transform.position, ball.transform.position + currentDirection * 2f, Color.blue, 0.1f);
            Debug.DrawLine(ball.transform.position, ball.transform.position + directionToTarget * 2f, Color.red, 0.1f);
            Debug.DrawLine(ball.transform.position, ball.transform.position + newDirection * 2f, Color.green, 0.1f);
            
            Debug.Log($"Adjusting trajectory: Current speed: {currentSpeed}, Target: {target.name}");
        }
    
        public override void OnCollision(BallBehaviour ball, Collision collision)
        {
            // Détruire la balle à l'impact avec quelque chose d'autre que son propriétaire
            PlayerController hitPlayer = collision.gameObject.GetComponent<PlayerController>();
            if (hitPlayer != null && ball.ownerIndex == hitPlayer.playerIndex) 
                return; // Ne pas détruire si c'est le propriétaire
                
            GameObject.Destroy(ball.gameObject);
        }

        public override void Initialize(Spawner spawner, int spawnPointIndex) { }

        public override void OnDespawn() { }

        public override Color GetAuraColor() => Color.clear;
    }
}