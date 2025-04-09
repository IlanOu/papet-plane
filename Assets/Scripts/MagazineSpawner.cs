using UnityEngine;

public class MagazineSpawner : MonoBehaviour
{
    [SerializeField] private GameObject magazinePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int timeBetweenSpawns = 5;
    [SerializeField] private int maxSpawnedMagazines = 5;
    
    // Randomiser le spawn des magazines toutes les 5 secondes
    private float _nextSpawnTime = 0f;

    private void Update()
    {
        if (Time.time >= _nextSpawnTime)
        {
            _nextSpawnTime = Time.time + timeBetweenSpawns;
            SpawnMagazine();
        }
    }

    private void SpawnMagazine()
    {
        if (GameObject.FindGameObjectsWithTag("Magazine").Length < maxSpawnedMagazines)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Instantiate(magazinePrefab, spawnPoints[randomIndex].position, Quaternion.identity);
        }
    }
}