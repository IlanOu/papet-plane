using System.Collections.Generic;
using UnityEngine;

namespace Spawning
{
    // Interface pour tout objet pouvant être suivi par le spawner
    public interface ISpawnable
    {
        void Initialize(Spawner spawner, int spawnPointIndex);
        void OnDespawn();
    }

    public class Spawner : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private int timeBetweenSpawns = 5;
        [SerializeField] private int maxActiveObjects = -1; // -1 signifie utiliser spawnPoints.Length
        
        // Randomiser le spawn des objets
        private float _nextSpawnTime = 0f;
        
        // Suivi des objets actifs et points de spawn occupés
        private List<int> _occupiedSpawnPoints = new List<int>();
        private List<GameObject> _activeObjects = new List<GameObject>();

        private void Start()
        {
            if (maxActiveObjects < 0)
            {
                maxActiveObjects = spawnPoints.Length;
            }
        }

        private void Update()
        {
            // Nettoyage des objets détruits
            CleanupDestroyedObjects();
            
            if (Time.time >= _nextSpawnTime)
            {
                _nextSpawnTime = Time.time + timeBetweenSpawns;
                SpawnObject();
            }
        }

        private void CleanupDestroyedObjects()
        {
            // Supprime les références aux objets qui ont été détruits en dehors du système
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                if (_activeObjects[i] == null)
                {
                    // L'objet a été détruit sans utiliser notre système
                    // On ne peut pas savoir quel point de spawn il occupait
                    // On pourrait améliorer cela avec un dictionnaire
                    _activeObjects.RemoveAt(i);
                }
            }
        }

        private void SpawnObject()
        {
            if (_activeObjects.Count < maxActiveObjects)
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
                    GameObject spawnedObject = Instantiate(prefab, spawnPoints[randomIndex].position, Quaternion.identity);
                    _occupiedSpawnPoints.Add(randomIndex);
                    _activeObjects.Add(spawnedObject);
                    
                    // Si l'objet implémente ISpawnable, l'initialiser
                    ISpawnable spawnable = spawnedObject.GetComponent<ISpawnable>();
                    if (spawnable != null)
                    {
                        spawnable.Initialize(this, randomIndex);
                    }
                    else
                    {
                        // Sinon, ajouter un tracker générique
                        SpawnTracker tracker = spawnedObject.AddComponent<SpawnTracker>();
                        tracker.Initialize(this, randomIndex);
                    }
                }
            }
        }
        
        // Méthode appelée quand un objet est détruit/despawné
        public void ObjectDespawned(int spawnPointIndex, GameObject obj)
        {
            _occupiedSpawnPoints.Remove(spawnPointIndex);
            _activeObjects.Remove(obj);
        }
        
        // Classe utilitaire pour suivre les objets qui n'implémentent pas ISpawnable
        private class SpawnTracker : MonoBehaviour, ISpawnable
        {
            private Spawner _spawner;
            private int _spawnPointIndex;
            
            public void Initialize(Spawner spawner, int spawnPointIndex)
            {
                _spawner = spawner;
                _spawnPointIndex = spawnPointIndex;
            }
            
            public void OnDespawn()
            {
                if (_spawner != null)
                {
                    _spawner.ObjectDespawned(_spawnPointIndex, gameObject);
                }
            }
            
            private void OnDestroy()
            {
                OnDespawn();
            }
        }
    }
}