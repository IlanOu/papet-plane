using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    [Header("Préfabs & Références")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    
    // [Header("Paramètres")]
    // [SerializeField] private int maxPlayers = 2;
    
    private List<PlayerInput> players = new List<PlayerInput>();
    private PlayerInputManager inputManager;
    
    private void Awake()
    {
        // Configurer le PlayerInputManager
        inputManager = FindObjectOfType<PlayerInputManager>();
        if (inputManager == null)
        {
            inputManager = gameObject.AddComponent<PlayerInputManager>();
        }
        
        inputManager.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        inputManager.playerPrefab = playerPrefab;
        // inputManager.maxPlayerCount = maxPlayers;
    }
    
    private void Start()
    {
        // Rechercher les manettes déjà connectées
        var gamepads = Gamepad.all;
        foreach (var gamepad in gamepads)
        {
            // Afficher les manettes disponibles dans la console
            Debug.Log($"Manette détectée: {gamepad.name} (ID: {gamepad.deviceId})");
        }
        
        // Instructions pour les joueurs
        Debug.Log("Pour rejoindre la partie, appuyez sur n'importe quelle touche sur votre manette");
    }
    
    // Appelé automatiquement quand un joueur rejoint la partie
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log($"Joueur {playerInput.playerIndex + 1} a rejoint avec le périphérique: {playerInput.devices[0].name}");
        
        players.Add(playerInput);
        
        // Positionner le joueur à son point d'apparition
        if (spawnPoints != null && spawnPoints.Length > playerInput.playerIndex)
        {
            playerInput.transform.position = spawnPoints[playerInput.playerIndex].position;
        }
        
        // Configurer le contrôleur du joueur
        var controller = playerInput.GetComponent<PlayerController>();
        // var controller = playerInput.GetComponent<SimpleMovement>();
        if (controller != null)
        {
            controller.InitPlayer(playerInput.playerIndex);
        }
    }
    
    // Appelé automatiquement quand un joueur quitte
    public void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log($"Joueur {playerInput.playerIndex + 1} a quitté");
        players.Remove(playerInput);
    }
}