using System;
using System.Collections;
using System.Collections.Generic;
using Multiplayer;
using Player;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    Intro,
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private UIManager uiManager;
    
    [Tooltip("Prefab du gestionnaire de multiplayer")]
    [SerializeField] private GameObject multiplayerManagerPrefab;
    private GameObject _multiplayerManager;
    
    [SerializeField] private int requiredPlayerCount = 2;
    
    public List<GameObject> instantiatedThings = new List<GameObject>();
    
    // State Machine
    private StateMachine _stateMachine;
    
    // States
    private MainMenuState _mainMenuState;
    private PlayingState _playingState;
    private PausedState _pausedState;
    private GameOverState _gameOverState;
    
    private void Awake()
    {
        // Pattern Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialiser la state machine
        InitializeStateMachine();
    }
    
    private void Start()
    {
        // Créer le gestionnaire de multiplayer dès le départ
        CreateMultiplayerManager();
        
        // Démarrer avec l'état MainMenu
        _stateMachine.ChangeState(_mainMenuState);
    }
    
    private void Update()
    {
        // Mettre à jour l'état actuel
        _stateMachine.Update();
    }
    
    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine();
        
        // Créer les états
        _mainMenuState = new MainMenuState(this, uiManager);
        _playingState = new PlayingState(this, uiManager);
        _pausedState = new PausedState(this, uiManager);
        _gameOverState = new GameOverState(this, uiManager);
    }
    
    // Méthode pour créer le gestionnaire de multiplayer
    private void CreateMultiplayerManager()
    {
        if (_multiplayerManager == null)
        {
            _multiplayerManager = Instantiate(multiplayerManagerPrefab);
            RegisterInstantiatedObject(_multiplayerManager);
            Debug.Log("Gestionnaire de multiplayer créé");
        }
    }
    
    // Méthode pour obtenir le nombre de joueurs
    public int GetPlayerCount()
    {
        return GameObject.FindGameObjectsWithTag("Player").Length;
    }
    
    // Méthode pour désactiver les scripts des joueurs (pour le menu)
    public void DisablePlayerScripts()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            // Désactiver les contrôles de joueur mais garder le PlayerInput actif pour rejoindre
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            // Désactiver d'autres scripts de gameplay
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                // Ne pas désactiver PlayerInput ni les scripts essentiels
                if (!(script is PlayerInput) && !(script is MultiplayerSetup) && script.enabled)
                {
                    script.enabled = false;
                }
            }
            
            // Optionnel : Mettre les joueurs dans une position "d'attente" dans le menu
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 10f);
            
            Debug.Log($"Scripts désactivés pour le joueur: {player.name}");
        }
    }
    
    // Méthode pour activer les scripts des joueurs (pour le jeu)
    public void EnablePlayerScripts()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            // Activer les contrôles de joueur
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = true;
            }
            
            // Activer d'autres scripts de gameplay
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                // Ne pas toucher aux scripts qui devraient rester désactivés
                string scriptName = script.GetType().Name;
                if (!(scriptName.Contains("Editor") || scriptName.Contains("Debug")))
                {
                    script.enabled = true;
                }
            }
            
            Debug.Log($"Scripts activés pour le joueur: {player.name}");
        }
    }
    
    // Transitions d'état
    public void StartGame()
    {
        _stateMachine.ChangeState(_playingState);
    }
    
    public void PauseGame()
    {
        _stateMachine.ChangeState(_pausedState);
    }
    
    public void ResumeGame()
    {
        _stateMachine.ChangeState(_playingState);
    }
    
    public void EndGame()
    {
        _stateMachine.ChangeState(_gameOverState);
    }
    
    public void ReturnToMainMenu()
    {
        StartCoroutine(CleanupAndReturnToMenu());
    }
    
    private IEnumerator CleanupAndReturnToMenu()
    {
        yield return StartCoroutine(CleanupGameCoroutine());
        _stateMachine.ChangeState(_mainMenuState);
    }
    
    [ContextMenu("Restart Game")]
    public void RestartGame()
    {
        StartCoroutine(RestartGameCoroutine());
    }
    
    private IEnumerator RestartGameCoroutine()
    {
        // Nettoyer d'abord
        yield return StartCoroutine(CleanupGameCoroutine());
        
        // Attendre une frame pour s'assurer que tout est bien détruit
        yield return null;
        
        // Redémarrer
        _stateMachine.ChangeState(_playingState);
    }
    
    // Méthodes internes
    public void SetupGame()
    {
        // Le MultiplayerManager est déjà créé dans le menu principal
        // Vous pouvez ajouter ici d'autres initialisations spécifiques au jeu
        
        // Désactiver le joining dans MultiplayerSetup
        if (_multiplayerManager != null)
        {
            MultiplayerSetup setup = _multiplayerManager.GetComponent<MultiplayerSetup>();
            if (setup != null)
            {
                // Désactiver le joining pour ne plus accepter de nouveaux joueurs
                setup.DisableJoining();
            }
        }
        
        // Activer les scripts des joueurs pour le gameplay
        EnablePlayerScripts();
    }
    
    [ContextMenu("Cleanup Game")]
    public void CleanupGame()
    {
        StartCoroutine(CleanupGameCoroutine());
    }
    
    private IEnumerator CleanupGameCoroutine()
    {
        Debug.Log("Début du nettoyage du jeu");
        
        // Désactiver les scripts des joueurs pour le menu
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.SetActive(false);
            Destroy(player);
        }
        
        // Désactiver le gestionnaire de multiplayer pour éviter qu'il ne crée de nouveaux joueurs
        if (_multiplayerManager != null)
        {
            MultiplayerSetup setup = _multiplayerManager.GetComponent<MultiplayerSetup>();
            if (setup != null)
            {
                // Désactiver le script pour éviter qu'il ne continue à fonctionner pendant la destruction
                setup.enabled = false;
            }
            
            // Désactiver l'objet avant de le détruire
            _multiplayerManager.SetActive(false);
            Destroy(_multiplayerManager);
            _multiplayerManager = null;
        }
        
        // Détruire tous les objets de jeu (mais pas les joueurs)
        foreach (GameObject thing in instantiatedThings)
        {
            if (thing != null && !thing.CompareTag("Player"))
            {
                thing.SetActive(false); // Désactiver d'abord
                Destroy(thing);
            }
        }
        
        // Nettoyer la liste, en gardant les joueurs
        instantiatedThings.RemoveAll(item => !item.CompareTag("Player") && item != null);
        
        // Attendre deux frames pour s'assurer que tout est bien détruit
        yield return null;
        yield return null;
        
        // Force une collection des objets détruits
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        
        Debug.Log("Nettoyage du jeu terminé");
        
        // Recréer le gestionnaire de multiplayer pour le menu principal
        CreateMultiplayerManager();
    }
    
    // Méthode pour ajouter des objets instanciés à la liste de suivi
    public void RegisterInstantiatedObject(GameObject obj)
    {
        if (obj != null && !instantiatedThings.Contains(obj))
        {
            instantiatedThings.Add(obj);
        }
    }
    
    // Getters
    public int RequiredPlayerCount => requiredPlayerCount;
    public MultiplayerSetup GetMultiplayerSetup()
    {
        if (_multiplayerManager != null)
        {
            return _multiplayerManager.GetComponent<MultiplayerSetup>();
        }
        return null;
    }
}

// State Machine de base
public class StateMachine
{
    private IState _currentState;
    
    public void ChangeState(IState newState)
    {
        if (_currentState != null)
        {
            _currentState.Exit();
        }
        
        _currentState = newState;
        _currentState.Enter();
    }
    
    public void Update()
    {
        if (_currentState != null)
        {
            _currentState.Update();
        }
    }
}

// Interface pour les états
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}

// Dans la classe MainMenuState, modifions la méthode Enter pour ne pas désactiver les scripts

public class MainMenuState : IState
{
    private GameManager _gameManager;
    private UIManager _uiManager;
    private bool _gameStarting = false;
    
    public MainMenuState(GameManager gameManager, UIManager uiManager)
    {
        _gameManager = gameManager;
        _uiManager = uiManager;
    }
    
    public void Enter()
    {
        Debug.Log("Entrée dans l'état MainMenu");
        _uiManager.ShowMainMenu();
        _gameStarting = false;
        
        // Ne pas désactiver les scripts des joueurs ici
        // Les joueurs n'ont pas encore rejoint le jeu
    }
    
    public void Update()
    {
        // Vérifier si on a assez de joueurs pour démarrer automatiquement
        if (!_gameStarting && _gameManager.GetPlayerCount() >= _gameManager.RequiredPlayerCount)
        {
            _gameStarting = true;
            Debug.Log($"Nombre requis de joueurs atteint ({_gameManager.RequiredPlayerCount}). Démarrage du jeu...");
            
            // Démarrer le jeu après un court délai
            _gameManager.Invoke("StartGame", 1.5f);
        }
    }
    
    public void Exit()
    {
        Debug.Log("Sortie de l'état MainMenu");
    }
}

// État de jeu
public class PlayingState : IState
{
    private GameManager _gameManager;
    private UIManager _uiManager;
    
    public PlayingState(GameManager gameManager, UIManager uiManager)
    {
        _gameManager = gameManager;
        _uiManager = uiManager;
    }
    
    public void Enter()
    {
        Debug.Log("Entrée dans l'état Playing");
        
        // Configurer le jeu
        _gameManager.SetupGame();
        
        // Régler le timeScale
        Time.timeScale = 1f;
        
        // Afficher l'UI de jeu
        _uiManager.ShowInGameMenu();
    }
    
    public void Update()
    {
        // Logique de mise à jour du jeu
        // Par exemple, vérifier les conditions de fin de jeu
    }
    
    public void Exit()
    {
        Debug.Log("Sortie de l'état Playing");
    }
}

// État de pause
public class PausedState : IState
{
    private GameManager _gameManager;
    private UIManager _uiManager;
    
    public PausedState(GameManager gameManager, UIManager uiManager)
    {
        _gameManager = gameManager;
        _uiManager = uiManager;
    }
    
    public void Enter()
    {
        Debug.Log("Entrée dans l'état Paused");
        
        // Mettre le jeu en pause
        Time.timeScale = 0f;
        
        // Afficher le menu de pause
        _uiManager.ShowPauseMenu();
    }
    
    public void Update()
    {
        // Logique de mise à jour de la pause
    }
    
    public void Exit()
    {
        Debug.Log("Sortie de l'état Paused");
    }
}

// État de fin de jeu
public class GameOverState : IState
{
    private GameManager _gameManager;
    private UIManager _uiManager;
    
    public GameOverState(GameManager gameManager, UIManager uiManager)
    {
        _gameManager = gameManager;
        _uiManager = uiManager;
    }
    
    public void Enter()
    {
        Debug.Log("Entrée dans l'état GameOver");
        
        // S'assurer que le temps est normal
        Time.timeScale = 1f;
        
        // Afficher l'écran de fin de jeu
        _uiManager.ShowGameOverMenu();
    }
    
    public void Update()
    {
        // Logique de mise à jour de l'écran de fin
    }
    
    public void Exit()
    {
        Debug.Log("Sortie de l'état GameOver");
    }
}