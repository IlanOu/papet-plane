using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerSetup : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    
    private PlayerInputManager inputManager;
    
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
}