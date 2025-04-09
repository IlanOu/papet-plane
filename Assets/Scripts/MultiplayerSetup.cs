using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MultiplayerSetup : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("Configuration des équipes")]
    [SerializeField] private int numberOfTeams = 2;
    [SerializeField] private int playersPerTeam = 1;
    [SerializeField] private int maxPlayers = 4; // Paramètre pour le nombre max de joueurs
    [SerializeField] private Color[] teamColors = { Color.red, Color.blue };
    
    private PlayerInputManager inputManager;
    private Dictionary<int, int> playerTeams = new Dictionary<int, int>(); // playerIndex -> teamIndex
    private int currentPlayerCount = 0;
    
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
        
        Debug.Log($"MultiplayerSetup initialisé - Maximum {maxPlayers} joueurs - Appuyez sur une touche sur chaque manette pour rejoindre");
    }
    
    private void OnPlayerJoined(PlayerInput newPlayerInput)
    {
        currentPlayerCount++;
        
        // Vérifier si le nombre maximum de joueurs est atteint
        if (currentPlayerCount > maxPlayers)
        {
            Debug.LogWarning($"Nombre maximum de joueurs atteint ({maxPlayers}). Joueur rejeté.");
            Destroy(newPlayerInput.gameObject);
            currentPlayerCount--;
            
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
                Destroy(newPlayerInput.gameObject);
                currentPlayerCount--;
                return;
            }
        }
        
        // Assigner une équipe
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
        
        // Si le nombre max est atteint, désactiver le joining
        if (currentPlayerCount >= maxPlayers)
        {
            inputManager.DisableJoining();
            Debug.Log("Nombre maximum de joueurs atteint, joining désactivé");
        }
    }
    
    // Coroutine pour réactiver temporairement le joining après rejet d'un joueur
    private System.Collections.IEnumerator TemporarilyDisableJoining()
    {
        inputManager.DisableJoining();
        yield return new WaitForSeconds(1f);
        
        // Réactiver le joining si on n'a pas atteint le max
        if (currentPlayerCount < maxPlayers)
        {
            inputManager.EnableJoining();
        }
    }
    
    // Méthode pour compter le nombre de joueurs dans une équipe
    private int CountPlayersInTeam(int teamIndex)
    {
        int count = 0;
        foreach (var kvp in playerTeams)
        {
            if (kvp.Value == teamIndex)
                count++;
        }
        return count;
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
    
    // Méthode pour gérer le départ d'un joueur (peut être appelée depuis ailleurs)
    public void OnPlayerLeft(PlayerInput player)
    {
        int playerIndex = player.playerIndex;
        
        // Supprimer de la liste des équipes
        if (playerTeams.ContainsKey(playerIndex))
        {
            playerTeams.Remove(playerIndex);
        }
        
        currentPlayerCount--;
        
        // Réactiver le joining si nécessaire
        if (currentPlayerCount < maxPlayers && !inputManager.joiningEnabled)
        {
            inputManager.EnableJoining();
            Debug.Log("Place disponible, joining réactivé");
        }
        
        Debug.Log($"Joueur {playerIndex+1} a quitté. Nombre de joueurs: {currentPlayerCount}");
    }
    
    // Pour connecter cet événement dans Start
    private void Start()
    {
        // Si le PlayerInputManager expose un événement onPlayerLeft, vous pouvez l'utiliser
        // Sinon, vous devrez appeler OnPlayerLeft manuellement quand un joueur quitte
        // inputManager.onPlayerLeft += OnPlayerLeft;
    }
}