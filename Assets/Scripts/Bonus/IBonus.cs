using Projectile;
using Spawning;
using UnityEngine;

public interface IBonus
{
    void ApplyTo(GameObject target);
}

public abstract class BallBonus : IBonus, ISpawnable
{
    public float duration = 5f; 
    public abstract Color GetAuraColor();
    public abstract void ApplyTo(GameObject ball);
    public abstract void UpdateBall(BallBehaviour ball);
    public abstract void OnCollision(BallBehaviour ball, Collision collision);

    public abstract void Initialize(Spawner spawner, int spawnPointIndex);
    public abstract void OnDespawn();
}
