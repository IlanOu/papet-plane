using System.Collections;
using System.Collections.Generic;
using Multiplayer;
using UI;
using UnityEngine;

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
    
    public List<GameObject> instantiatedThings = new List<GameObject>();
    
    // Propriétés privées
    private GameState _currentState;
    private bool _isRestarting = false;
    
    // Propriétés publiques
    public GameState CurrentState => _currentState;
    
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
        
        // Configuration initiale
        SetGameState(GameState.MainMenu);
        StartGame();
    }
    
    private void Update()
    {
        // Mise à jour selon l'état du jeu
        switch (_currentState)
        {
            case GameState.MainMenu:
                uiManager.ShowMainMenu();
                break;
            case GameState.Playing:
                uiManager.ShowInGameMenu();
                break;
            case GameState.Paused:
                uiManager.ShowPauseMenu();
                break;
            case GameState.GameOver:
                uiManager.ShowGameOverMenu();
                break;
        }
    }
    
    [ContextMenu("Start Game")]
    // Méthodes principales de gestion du jeu
    public void StartGame()
    {
        SetupGame();
        SetGameState(GameState.Playing);
        
        // Informer l'UI
        if (uiManager != null)
        {
            uiManager.ShowInGameMenu();
        }
    }
    
    public void PauseGame()
    {
        if (_currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
            
            // Informer l'UI
            if (uiManager != null)
            {
                uiManager.ShowPauseMenu();
            }
        }
    }
    
    public void ResumeGame()
    {
        if (_currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
            Time.timeScale = 1f;
            
            // Informer l'UI
            if (uiManager != null)
            {
                uiManager.ShowInGameMenu();
            }
        }
    }
    
    public void EndGame()
    {
        SetGameState(GameState.GameOver);
        Time.timeScale = 1f;
        
        // Informer l'UI
        if (uiManager != null)
        {
            uiManager.ShowGameOverMenu();
        }
    }
    
    public void ReturnToMainMenu()
    {
        StartCoroutine(CleanupAndReturnToMenu());
    }
    
    private IEnumerator CleanupAndReturnToMenu()
    {
        yield return StartCoroutine(CleanupGameCoroutine());
        
        SetGameState(GameState.MainMenu);
        
        // Informer l'UI
        if (uiManager != null)
        {
            uiManager.ShowMainMenu();
        }
    }
    
    [ContextMenu("Restart Game")]
    public void RestartGame()
    {
        if (_isRestarting) return; // Éviter les redémarrages multiples
        
        _isRestarting = true;
        StartCoroutine(RestartGameCoroutine());
    }
    
    private IEnumerator RestartGameCoroutine()
    {
        // Nettoyer d'abord
        yield return StartCoroutine(CleanupGameCoroutine());
        
        // Attendre une frame pour s'assurer que tout est bien détruit
        yield return null;
        
        // Redémarrer
        SetupGame();
        SetGameState(GameState.Playing);
        
        // Informer l'UI
        if (uiManager != null)
        {
            uiManager.ShowInGameMenu();
        }
        
        _isRestarting = false;
    }
    
    // Méthodes internes
    private void SetupGame()
    {
        // S'assurer qu'il n'y a pas déjà un gestionnaire de multiplayer
        if (_multiplayerManager == null)
        {
            _multiplayerManager = Instantiate(multiplayerManagerPrefab);
            Debug.Log("Gestionnaire de multiplayer créé");
        }
    }
    
    [ContextMenu("Cleanup Game")]
    private void CleanupGame()
    {
        StartCoroutine(CleanupGameCoroutine());
    }
    
    private IEnumerator CleanupGameCoroutine()
    {
        Debug.Log("Début du nettoyage du jeu");
        
        // Désactiver le gestionnaire de multiplayer pour éviter qu'il ne crée de nouveaux joueurs
        if (_multiplayerManager != null)
        {
            MultiplayerSetup setup = _multiplayerManager.GetComponent<MultiplayerSetup>();
            if (setup != null)
            {
                // Désactiver le script pour éviter qu'il ne continue à fonctionner pendant la destruction
                setup.enabled = false;
                
                // Si vous avez ajouté la méthode CleanupAllPlayers, utilisez-la
                if (setup.GetType().GetMethod("CleanupAllPlayers") != null)
                {
                    setup.SendMessage("CleanupAllPlayers");
                }
            }
            
            // Désactiver l'objet avant de le détruire
            _multiplayerManager.SetActive(false);
            Destroy(_multiplayerManager);
            _multiplayerManager = null;
        }
        
        // Détruire tous les joueurs dans la scène par tag
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.SetActive(false); // Désactiver d'abord
            Destroy(player);
        }
        
        // Détruire toutes les choses instanciées
        foreach (GameObject thing in instantiatedThings)
        {
            if (thing != null)
            {
                thing.SetActive(false); // Désactiver d'abord
                Destroy(thing);
            }
        }
        instantiatedThings.Clear();
        
        // Attendre deux frames pour s'assurer que tout est bien détruit
        yield return null;
        yield return null;
        
        // Force une collection des objets détruits
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        
        Debug.Log("Nettoyage du jeu terminé");
    }
    
    private void SetGameState(GameState newState)
    {
        _currentState = newState;
        Debug.Log($"État du jeu changé pour : {newState}");
        
        // Déclencher l'événement de changement d'état
        OnGameStateChanged?.Invoke(newState);
    }
    
    // Méthode pour ajouter des objets instanciés à la liste de suivi
    public void RegisterInstantiatedObject(GameObject obj)
    {
        if (obj != null && !instantiatedThings.Contains(obj))
        {
            instantiatedThings.Add(obj);
        }
    }
    
    // Événement pour le changement d'état
    public delegate void GameStateChangedHandler(GameState newState);
    public event GameStateChangedHandler OnGameStateChanged;
}