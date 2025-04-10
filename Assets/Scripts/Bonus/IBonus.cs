using Projectile;
using UnityEngine;

public interface IBonus
{
    void ApplyTo(GameObject target);
}

public abstract class BallBonus : IBonus
{
    public abstract void ApplyTo(GameObject ball);
    public abstract void UpdateBall(BallBehaviour ball);
    public abstract void OnCollision(BallBehaviour ball, Collision collision);
}
