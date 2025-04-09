using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MultiplayerSetup : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("Configuration des équipes")]
    [SerializeField] private int numberOfTeams = 2;
    [SerializeField] private Color[] teamColors = { Color.red, Color.blue };
    
    private PlayerInputManager inputManager;
    private Dictionary<int, int> playerTeams = new Dictionary<int, int>(); // playerIndex -> teamIndex
    
    void Awake()
    {
        // Créer le PlayerInputManager s'il n'existe pas
        inputManager = FindObjectOfType<PlayerInputManager>();
        if (inputManager == null)
        {
            inputManager = gameObject.AddComponent<PlayerInputManager>();
            Debug.Log("PlayerInputManager ajouté automatiquement");
        }
        
        // Configuration basique
        inputManager.playerPrefab = playerPrefab;
        inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        
        // Connecter les événements
        inputManager.onPlayerJoined += OnPlayerJoined;
        
        Debug.Log("MultiplayerSetup initialisé - Appuyez sur une touche sur chaque manette pour rejoindre");
    }
    
    private void OnPlayerJoined(PlayerInput newPlayerInput)
    {
        int playerIndex = newPlayerInput.playerIndex;
        Debug.Log($"Joueur {playerIndex+1} a rejoint avec {newPlayerInput.devices[0].name}");
        
        // Assigner une équipe (alternance simple)
        int teamIndex = playerIndex % numberOfTeams;
        playerTeams[playerIndex] = teamIndex;
        
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
            Debug.Log($"Joueur {playerIndex+1} placé à la position {spawnPoints[playerIndex].position}");
        }
        else
        {
            Debug.LogError($"Pas de spawn point pour le joueur {playerIndex+1}!");
        }
    }
    
    // Méthode utilitaire pour obtenir l'équipe d'un joueur
    public int GetPlayerTeam(int playerIndex)
    {
        if (playerTeams.TryGetValue(playerIndex, out int teamIndex))
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
}