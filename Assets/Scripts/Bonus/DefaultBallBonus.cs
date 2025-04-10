using UnityEngine;

public class DefaultBallBonus : BallBonus
{
    public override void ApplyTo(GameObject ball) { }
    
    public override void UpdateBall(BallBehaviour ball) { }
    
    public override void OnCollision(BallBehaviour ball, Collision collision)
    {
        // Détruire la balle à l'impact
        GameObject.Destroy(ball.gameObject);
    }
}