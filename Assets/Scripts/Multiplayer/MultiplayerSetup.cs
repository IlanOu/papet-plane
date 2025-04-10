using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Multiplayer
{
    public class MultiplayerSetup : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform[] spawnPoints;
    
        [Header("Configuration des équipes")]
        [SerializeField] private int numberOfTeams = 2;
        [SerializeField] private int playersPerTeam = 1;
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private Color[] teamColors = { Color.red, Color.blue };
    
        private PlayerInputManager _inputManager;
        private Dictionary<int, int> _playerTeams = new Dictionary<int, int>(); // playerIndex -> teamIndex
        private int _currentPlayerCount = 0;
        private List<GameObject> _spawnedPlayers = new List<GameObject>(); // Garde une référence aux joueurs créés
    
        private void Awake()
        {
            // Initialiser les collections
            _playerTeams = new Dictionary<int, int>();
            _spawnedPlayers = new List<GameObject>();
        }
    
        void Start()
        {
            InitializeInputManager();
        }
        
        private void InitializeInputManager()
        {
            // Créer le PlayerInputManager s'il n'existe pas
            _inputManager = FindObjectOfType<PlayerInputManager>();
            if (_inputManager == null)
            {
                _inputManager = gameObject.AddComponent<PlayerInputManager>();
                Debug.Log("PlayerInputManager ajouté automatiquement");
            }
        
            // Configuration basique
            _inputManager.playerPrefab = playerPrefab;
            _inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        
            // Connecter les événements
            _inputManager.onPlayerJoined += OnPlayerJoined;
        
            Debug.Log($"MultiplayerSetup initialisé - Maximum {maxPlayers} joueurs - Appuyez sur une touche sur chaque manette pour rejoindre");
        }
    
        private void OnPlayerJoined(PlayerInput newPlayerInput)
        {
            GameManager.Instance.DisablePlayerScripts();
            
            _currentPlayerCount++;
            _spawnedPlayers.Add(newPlayerInput.gameObject); // Ajouter à la liste des joueurs
        
            // Vérifier si le nombre maximum de joueurs est atteint
            if (_currentPlayerCount > maxPlayers)
            {
                Debug.LogWarning($"Nombre maximum de joueurs atteint ({maxPlayers}). Joueur rejeté.");
                _spawnedPlayers.Remove(newPlayerInput.gameObject); // Retirer de la liste avant destruction
                Destroy(newPlayerInput.gameObject);
                _currentPlayerCount--;
            
                // Désactiver temporairement la possibilité de rejoindre
                StartCoroutine(TemporarilyDisableJoining());
                return;
            }
        
            int playerIndex = newPlayerInput.playerIndex;
            Debug.Log($"Joueur {playerIndex+1} a rejoint avec {newPlayerInput.devices[0].name}");
        
            // Vérifier si le nombre maximum de joueurs par équipe est atteint
            int teamIndex = playerIndex % numberOfTeams;
            int playersInTeam = CountPlayersInTeam(teamIndex);
            if (playersInTeam >= playersPerTeam)
            {
                // Essayer de trouver une équipe avec de la place
                bool foundTeam = false;
                for (int i = 0; i < numberOfTeams; i++)
                {
                    if (CountPlayersInTeam(i) < playersPerTeam)
                    {
                        teamIndex = i;
                        foundTeam = true;
                        break;
                    }
                }
            
                if (!foundTeam)
                {
                    Debug.LogWarning($"Toutes les équipes sont complètes. Joueur {playerIndex+1} rejeté.");
                    _spawnedPlayers.Remove(newPlayerInput.gameObject); // Retirer de la liste avant destruction
                    Destroy(newPlayerInput.gameObject);
                    _currentPlayerCount--;
                    return;
                }
            }
        
            // Assigner une équipe
            _playerTeams[playerIndex] = teamIndex;
        
            // Configurer le PlayerController
            PlayerController controller = newPlayerInput.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.InitPlayer(playerIndex);
            
                // Appliquer la couleur de l'équipe
                if (controller.spriteRenderer != null && teamIndex < teamColors.Length)
                {
                    controller.spriteRenderer.material.color = teamColors[teamIndex];
                    Debug.Log($"Joueur {playerIndex+1} assigné à l'équipe {teamIndex+1} (couleur: {teamColors[teamIndex]})");
                }
            }
        
            // Positionner au spawn point
            if (spawnPoints != null && playerIndex < spawnPoints.Length)
            {
                newPlayerInput.transform.position = spawnPoints[playerIndex].position;
            }
            else
            {
                Debug.LogError($"Pas de spawn point pour le joueur {playerIndex+1}!");
            }
        
            // Si le nombre max est atteint, désactiver le joining
            if (_currentPlayerCount >= maxPlayers)
            {
                _inputManager.DisableJoining();
                Debug.Log("Nombre maximum de joueurs atteint, joining désactivé");
            }
        }
    
        // Coroutine pour réactiver temporairement le joining après rejet d'un joueur
        private System.Collections.IEnumerator TemporarilyDisableJoining()
        {
            _inputManager.DisableJoining();
            yield return new WaitForSeconds(1f);
        
            // Réactiver le joining si on n'a pas atteint le max
            if (_currentPlayerCount < maxPlayers && this != null && _inputManager != null)
            {
                _inputManager.EnableJoining();
            }
        }
    
        // Méthode pour compter le nombre de joueurs dans une équipe
        private int CountPlayersInTeam(int teamIndex)
        {
            int count = 0;
            foreach (var kvp in _playerTeams)
            {
                if (kvp.Value == teamIndex)
                    count++;
            }
            return count;
        }
    
        // Méthode utilitaire pour obtenir l'équipe d'un joueur
        public int GetPlayerTeam(int playerIndex)
        {
            if (_playerTeams.TryGetValue(playerIndex, out int teamIndex))
            {
                return teamIndex;
            }
            return -1; // Joueur non trouvé ou pas encore assigné à une équipe
        }
    
        // Méthode pour vérifier si deux joueurs sont dans la même équipe
        public bool AreSameTeam(int playerIndex1, int playerIndex2)
        {
            int team1 = GetPlayerTeam(playerIndex1);
            int team2 = GetPlayerTeam(playerIndex2);
        
            return team1 != -1 && team2 != -1 && team1 == team2;
        }
    
        // Méthode pour gérer le départ d'un joueur (peut être appelée depuis ailleurs)
        public void OnPlayerLeft(PlayerInput player)
        {
            int playerIndex = player.playerIndex;
        
            // Supprimer de la liste des équipes
            if (_playerTeams.ContainsKey(playerIndex))
            {
                _playerTeams.Remove(playerIndex);
            }
            
            // Supprimer de la liste des joueurs
            _spawnedPlayers.Remove(player.gameObject);
            
            _currentPlayerCount--;
        
            // Réactiver le joining si nécessaire
            if (_currentPlayerCount < maxPlayers && _inputManager != null && !_inputManager.joiningEnabled)
            {
                _inputManager.EnableJoining();
                Debug.Log("Place disponible, joining réactivé");
            }
        
            Debug.Log($"Joueur {playerIndex+1} a quitté. Nombre de joueurs: {_currentPlayerCount}");
        }
        
        // Méthode pour nettoyer tous les joueurs
        public void CleanupAllPlayers()
        {
            // Arrêter toutes les coroutines
            StopAllCoroutines();
            
            // Désenregistrer l'événement pour éviter les callbacks après destruction
            if (_inputManager != null)
            {
                _inputManager.onPlayerJoined -= OnPlayerJoined;
            }
            
            // Détruire tous les joueurs
            foreach (GameObject player in _spawnedPlayers.ToArray()) // Utiliser une copie pour éviter les problèmes de modification pendant l'itération
            {
                if (player != null)
                {
                    Destroy(player);
                }
            }
            
            // Vider les collections
            _spawnedPlayers.Clear();
            _playerTeams.Clear();
            _currentPlayerCount = 0;
            
            Debug.Log("Tous les joueurs ont été nettoyés");
        }
        
        // Appelé quand l'objet est détruit
        private void OnDestroy()
        {
            // Désenregistrer l'événement pour éviter les fuites mémoire
            if (_inputManager != null)
            {
                _inputManager.onPlayerJoined -= OnPlayerJoined;
            }
            
            // Nettoyer les joueurs si ce n'est pas déjà fait
            CleanupAllPlayers();
        }
        
        // Méthode publique pour réinitialiser le système
        public void Reset()
        {
            CleanupAllPlayers();
            InitializeInputManager();
        }
        
        public void DisableJoining()
        {
            if (_inputManager != null)
            {
                _inputManager.DisableJoining();
                Debug.Log("Joining désactivé");
            }
        }
    }
}