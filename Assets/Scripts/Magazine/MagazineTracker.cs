using Magazine;
using UnityEngine;

public class MagazineTracker : MonoBehaviour
{
    private MagazineSpawner _spawner;
    private int _spawnPointIndex;
    
    public void SetSpawner(MagazineSpawner spawner, int spawnPointIndex)
    {
        _spawner = spawner;
        _spawnPointIndex = spawnPointIndex;
    }
    
    private void OnDestroy()
    {
        if (_spawner != null)
        {
            _spawner.MagazineDestroyed(_spawnPointIndex);
        }
    }
}