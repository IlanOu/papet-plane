using Spawning;
using UnityEngine;

public class MagazineTracker : MonoBehaviour, ISpawnable
{
    private Spawner _spawner;
    private int _spawnPointIndex;
    
    // Implémentation de la méthode Initialize de l'interface ISpawnable
    public void Initialize(Spawner spawner, int spawnPointIndex)
    {
        _spawner = spawner;
        _spawnPointIndex = spawnPointIndex;
    }
    
    // Implémentation de la méthode OnDespawn de l'interface ISpawnable
    public void OnDespawn()
    {
        if (_spawner != null)
        {
            _spawner.ObjectDespawned(_spawnPointIndex, gameObject);
        }
    }
    
    // Appel de OnDespawn quand l'objet est détruit
    private void OnDestroy()
    {
        OnDespawn();
    }
}