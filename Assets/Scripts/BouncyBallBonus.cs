using UnityEngine;

public class BouncyBallBonus : BallBonus
{
    public float bounciness = 0.8f;
    public float lifeTime = 5f;
    
    public override void ApplyTo(GameObject ball)
    {
        Collider collider = ball.GetComponent<Collider>();
        if (collider != null)
        {
            PhysicsMaterial physicMaterial = new PhysicsMaterial("Bouncy");
            physicMaterial.bounciness = bounciness;
            physicMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            physicMaterial.bounceCombine = PhysicsMaterialCombine.Maximum;
            collider.material = physicMaterial;
        }
    }
    
    public override void UpdateBall(BallBehaviour ball)
    {
        // La balle se détruit après sa durée de vie
        if (ball.timer >= lifeTime)
        {
            GameObject.Destroy(ball.gameObject);
        }
    }
    
    public override void OnCollision(BallBehaviour ball, Collision collision)
    {
        // Ne rien faire, la balle continue d'exister après collision
    }
}