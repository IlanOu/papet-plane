using System.Collections.Generic;
using Game;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerLifeSystem : MonoBehaviour
    {
        [Header("Composants")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHit playerHit;

        [Header("Paramètres")]
        public int maxLife = 3;
        public int hitDamage = 1;
        
        [Header("Respawn")]
        [Tooltip("Tag des points de réapparition")]
        [SerializeField] private string respawnPointTag = "RespawnPoint";
        [Tooltip("Tag des joueurs pour éviter les spawnkills")]
        [SerializeField] private string playerTag = "Player";
        [Tooltip("Délai avant réapparition (en secondes)")]
        [SerializeField] private float respawnDelay = 0.5f;
        [Tooltip("Effet visuel lors de la réapparition")]
        [SerializeField] private GameObject respawnEffectPrefab;
        [Tooltip("Durée d'invincibilité après réapparition (en secondes)")]
        [SerializeField] private float invincibilityDuration = 1.5f;
        [Tooltip("Nombre de meilleurs points de spawn à considérer")]
        [SerializeField] private int topSpawnPointsToConsider = 3;
        
        [Header("Game Over")]
        [Tooltip("Nombre de vies perdues avant Game Over")]
        [SerializeField] private int livesBeforeGameOver = 3;
        [Tooltip("Délai avant de passer à l'écran Game Over (en secondes)")]
        [SerializeField] private float gameOverDelay = 2f;
        
        private int _currentLife;
        private int _livesLost = 0; // Compteur de vies perdues
        private bool _isInvincible = false;
        private List<Transform> _respawnPoints = new List<Transform>();
        
        [Header("Events")]
        public UnityEvent<int, int> onLifeChanged; // Envoie currentLife, maxLife
        public UnityEvent onRespawn; // Déclenché quand le joueur réapparaît
        public UnityEvent onGameOver; // Déclenché quand le joueur n'a plus de vies
        
        [Header("Indicator")]
        public string lifeUITag = "LifeUI";
        
        private LifeUI _lifeUI;
        private bool _gameOverTriggered = false;
        
        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            
            if (playerHit == null)
                playerHit = GetComponent<PlayerHit>();
            
            // Trouver tous les points de respawn
            FindRespawnPoints();
            
            ResetLife();
            _livesLost = 0;
            _gameOverTriggered = false;
        }
        
        private void Start()
        {
            // Trouver l'UI correspondante
            _lifeUI = FindLifeUI();
            
            if (_lifeUI != null)
            {
                // S'abonner à l'événement APRÈS avoir trouvé l'UI
                onLifeChanged.AddListener(_lifeUI.UpdateHearts);
                
                // Initialiser l'UI au démarrage
                _lifeUI.Initialize(_currentLife, maxLife);
            }
            else
            {
                Debug.LogError($"LifeUI non trouvée pour le joueur {playerController.playerIndex}");
            }   
            
            if (playerHit != null)
            {
                playerHit.onHit.AddListener(() => TakeDamage(hitDamage));
            }
        }
        
        private void OnDestroy()
        {
            // Se désabonner de l'événement
            if (_lifeUI != null)
            {
                onLifeChanged.RemoveListener(_lifeUI.UpdateHearts);
            }
        }
        
        private void FindRespawnPoints()
        {
            _respawnPoints.Clear();
            GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag(respawnPointTag);
            
            foreach (GameObject respawnObj in respawnObjects)
            {
                _respawnPoints.Add(respawnObj.transform);
            }
            
            if (_respawnPoints.Count == 0)
            {
                Debug.LogWarning($"Aucun point de respawn trouvé avec le tag {respawnPointTag}");
            }
            else
            {
                Debug.Log($"Trouvé {_respawnPoints.Count} points de respawn");
            }
        }
        
        public void TakeDamage(int damage)
        {
            // Ignorer les dégâts si invincible ou si GameOver déjà déclenché
            if (_isInvincible || _gameOverTriggered)
                return;
                
            _currentLife -= damage;
            onLifeChanged?.Invoke(_currentLife, maxLife);
            
            if (_currentLife <= 0)
            {
                _livesLost++;
                Debug.Log($"Joueur {playerController.playerIndex} a perdu une vie. Vies perdues: {_livesLost}/{livesBeforeGameOver}");
                
                if (_livesLost >= livesBeforeGameOver)
                {
                    // Plus de vies, déclencher le Game Over
                    TriggerGameOver();
                }
                else
                {
                    // Il reste des vies, réinitialiser la vie et réapparaître
                    ResetLife();
                    RespawnAtSafestPoint();
                }
            }
            else
            {
                // Encore de la vie, juste réapparaître
                RespawnAtSafestPoint();
            }
        }
        
        private void TriggerGameOver()
        {
            if (_gameOverTriggered)
                return;
                
            _gameOverTriggered = true;
            
            // Désactiver les contrôles du joueur
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            
            // Jouer une animation de mort si disponible
            // ...
            
            // Déclencher le Game Over après un délai
            StartCoroutine(GameOverCoroutine());
        }
        
        private System.Collections.IEnumerator GameOverCoroutine()
        {
            // Attendre le délai avant de passer à l'écran Game Over
            yield return new WaitForSeconds(gameOverDelay);
            
            // Déclencher l'événement onGameOver
            onGameOver?.Invoke();
            
            // Transition vers l'état GameOver du GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame();
            }
        }
        
        private void RespawnAtSafestPoint()
        {
            // Vérifier s'il y a des points de respawn disponibles
            if (_respawnPoints.Count == 0)
            {
                Debug.LogWarning("Tentative de respawn mais aucun point n'est disponible");
                return;
            }
            
            // Désactiver temporairement les contrôles du joueur
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            
            // Commencer le processus de réapparition
            StartCoroutine(RespawnCoroutine());
        }
        
        private System.Collections.IEnumerator RespawnCoroutine()
        {
            // Rendre le joueur invisible ou jouer une animation de disparition
            if (playerController.spriteRenderer != null)
            {
                playerController.spriteRenderer.enabled = false;
            }
            
            // Désactiver les collisions pendant la réapparition
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }
            
            // Attendre le délai de réapparition
            yield return new WaitForSeconds(respawnDelay);
            
            // Trouver le point de respawn le plus sûr
            Transform respawnPoint = FindSafestRespawnPoint();
            
            // Déplacer le joueur au point de respawn
            transform.position = respawnPoint.position;
            
            // Rendre le joueur à nouveau visible
            if (playerController.spriteRenderer != null)
            {
                playerController.spriteRenderer.enabled = true;
            }
            
            // Réactiver les collisions
            if (playerCollider != null)
            {
                playerCollider.enabled = true;
            }
            
            // Jouer un effet de réapparition si disponible
            if (respawnEffectPrefab != null)
            {
                Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Réactiver les contrôles du joueur
            if (playerController != null)
            {
                playerController.enabled = true;
            }
            
            // Déclencher l'événement de réapparition
            onRespawn?.Invoke();
            
            // Activer l'invincibilité temporaire
            StartCoroutine(TemporaryInvincibility());
        }
        
        private Transform FindSafestRespawnPoint()
        {
            // Trouver tous les autres joueurs
            GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag(playerTag);
            List<GameObject> players = new List<GameObject>();
            
            // Exclure ce joueur de la liste
            foreach (GameObject player in otherPlayers)
            {
                if (player != this.gameObject)
                {
                    players.Add(player);
                }
            }
            
            // S'il n'y a pas d'autres joueurs, choisir un point aléatoire
            if (players.Count == 0)
            {
                return _respawnPoints[Random.Range(0, _respawnPoints.Count)];
            }
            
            // Calculer le score de sécurité pour chaque point de spawn
            Dictionary<Transform, float> spawnPointScores = new Dictionary<Transform, float>();
            
            foreach (Transform spawnPoint in _respawnPoints)
            {
                float totalDistance = 0f;
                
                foreach (GameObject player in players)
                {
                    float distance = Vector3.Distance(spawnPoint.position, player.transform.position);
                    totalDistance += distance;
                }
                
                spawnPointScores[spawnPoint] = totalDistance;
            }
            
            // Trier les points de spawn par score (du plus élevé au plus bas)
            List<KeyValuePair<Transform, float>> sortedSpawnPoints = new List<KeyValuePair<Transform, float>>(spawnPointScores);
            sortedSpawnPoints.Sort((a, b) => b.Value.CompareTo(a.Value));
            
            // Sélectionner aléatoirement un des meilleurs points de spawn
            int maxIndex = Mathf.Min(topSpawnPointsToConsider, sortedSpawnPoints.Count);
            int randomIndex = Random.Range(0, maxIndex);
            
            Transform selectedSpawnPoint = sortedSpawnPoints[randomIndex].Key;
            
            Debug.Log($"Sélectionné le point de spawn {selectedSpawnPoint.name} avec un score de sécurité de {sortedSpawnPoints[randomIndex].Value}");
            
            return selectedSpawnPoint;
        }
        
        private System.Collections.IEnumerator TemporaryInvincibility()
        {
            _isInvincible = true;
            
            // Effet visuel d'invincibilité (clignotement)
            if (playerController.spriteRenderer != null)
            {
                float endTime = Time.time + invincibilityDuration;
                while (Time.time < endTime)
                {
                    playerController.spriteRenderer.enabled = !playerController.spriteRenderer.enabled;
                    yield return new WaitForSeconds(0.1f);
                }
                playerController.spriteRenderer.enabled = true;
            }
            else
            {
                // Si pas de sprite renderer, juste attendre
                yield return new WaitForSeconds(invincibilityDuration);
            }
            
            _isInvincible = false;
        }
        
        public void Heal(int amount)
        {
            _currentLife = Mathf.Min(_currentLife + amount, maxLife);
            onLifeChanged?.Invoke(_currentLife, maxLife);
        }
        
        public int GetCurrentLife()
        {
            return _currentLife;
        }
        
        public int GetMaxLife()
        {
            return maxLife;
        }
        
        public void ResetLife()
        {
            _currentLife = maxLife;
            onLifeChanged?.Invoke(_currentLife, maxLife);
        }
        
        public void SetLife(int life)
        {
            _currentLife = life;
            onLifeChanged?.Invoke(_currentLife, maxLife);
        }
        
        public void SetMaxLife(int life)
        {
            maxLife = life;
            onLifeChanged?.Invoke(_currentLife, maxLife);
        }
        
        // Réinitialiser complètement le joueur (pour un nouveau jeu)
        public void ResetPlayer()
        {
            ResetLife();
            _livesLost = 0;
            _gameOverTriggered = false;
            
            // Réactiver les contrôles du joueur
            if (playerController != null)
            {
                playerController.enabled = true;
            }
            
            // S'assurer que le joueur est visible
            if (playerController.spriteRenderer != null)
            {
                playerController.spriteRenderer.enabled = true;
            }
        }
        
        private LifeUI FindLifeUI()
        {
            GameObject[] lifeUIs = GameObject.FindGameObjectsWithTag(lifeUITag);

            foreach (GameObject lifeUIObj in lifeUIs)
            {
                LifeUI lifeUI = lifeUIObj.GetComponent<LifeUI>();
                if (lifeUI != null && lifeUI.playerIndex == playerController.playerIndex)
                {
                    return lifeUI;
                }
            }
            
            return null;
        }
    }
}