using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Magazine
{
    public class MagazineSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject magazinePrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private int timeBetweenSpawns = 5;
        private int _maxSpawnedMagazines;
    
        // Randomiser le spawn des magazines toutes les 5 secondes
        private float _nextSpawnTime = 0f;
    
        // Liste des points de spawn occupés
        private List<int> _occupiedSpawnPoints = new List<int>();

        private void Start()
        {
            _maxSpawnedMagazines = spawnPoints.Length;
        }

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
            if (GameObject.FindGameObjectsWithTag("Magazine").Length < _maxSpawnedMagazines)
            {
                // Créer une liste des indices disponibles
                List<int> availableIndices = new List<int>();
            
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    if (!_occupiedSpawnPoints.Contains(i))
                    {
                        availableIndices.Add(i);
                    }
                }
            
                // S'il y a des points disponibles
                if (availableIndices.Count > 0)
                {
                    int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
                    GameObject magazine = Instantiate(magazinePrefab, spawnPoints[randomIndex].position, Quaternion.identity);
                    _occupiedSpawnPoints.Add(randomIndex);
                
                    // Configurer le magazine pour qu'il informe le spawner quand il est détruit
                    MagazineTracker tracker = magazine.AddComponent<MagazineTracker>();
                    tracker.SetSpawner(this, randomIndex);
                }
            }
        }
    
        // Méthode appelée quand un magazine est détruit
        public void MagazineDestroyed(int spawnPointIndex)
        {
            _occupiedSpawnPoints.Remove(spawnPointIndex);
        }
    }
}
