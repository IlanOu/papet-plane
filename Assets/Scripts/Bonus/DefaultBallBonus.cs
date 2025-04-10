using Projectile;
using Spawning;
using UnityEngine;

namespace Bonus
{
    public class DefaultBallBonus : BallBonus
    {
        [Tooltip("Durée de vie de l'avion en secondes")]
        public float lifeTime = 3f;
    
        public override void ApplyTo(GameObject ball) { }

        public override void UpdateBall(BallBehaviour ball)
        {
            if (ball.timer >= lifeTime)
            {
                GameObject.Destroy(ball.gameObject);
            }
        }
    
        public override void OnCollision(BallBehaviour ball, Collision collision)
        {
            // Détruire la balle à l'impact
            // if (collision.gameObject.GetComponent<PlayerController>())
            //     if (ball.ownerIndex == collision.gameObject.GetComponent<PlayerController>().playerIndex) return;
            // GameObject.Destroy(ball.gameObject);
        }

        public override void Initialize(Spawner spawner, int spawnPointIndex) { }

        public override void OnDespawn() { }

        public override Color GetAuraColor() => Color.clear;
    }
}