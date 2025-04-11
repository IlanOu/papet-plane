using System.Collections;
using System.Collections.Generic;
using Multiplayer;
using Player;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
    
        [Header("References")]
        public UIManager uiManager;
        
        [Tooltip("Prefab du gestionnaire de multiplayer")]
        [SerializeField] private GameObject multiplayerManagerPrefab;
        private GameObject _multiplayerManager;
    
        [SerializeField] private int requiredPlayerCount = 2;
        
        public List<GameObject> instantiatedThings = new List<GameObject>();
    
        public int winnerIndex = -1;
        
        [Header("Parameters")]
        public Color[] teamColors = { Color.red, Color.blue };
        
        // State Machine
        private StateMachine _stateMachine;
    
        // States
        private MainMenuState _mainMenuState;
        private PlayingState _playingState;
        private GameOverState _gameOverState;

        [HideInInspector] public float lastTimeInput;
        
        private void Awake()
        {
            // Pattern Singleton
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject);
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
    
            // Sauvegarder temporairement l'instance actuelle
            GameManager currentInstance = Instance;
    
            // Réinitialiser la référence statique avant de charger la nouvelle scène
            Instance = null;
    
            // Charger la scène asynchrone
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

            // Attendre que la scène soit prête
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
    
            // Maintenant que la scène est chargée, détruire l'ancien GameManager
            if (currentInstance != null)
            {
                Destroy(currentInstance.gameObject);
            }
        }
    
        // Méthodes internes
        public void SetupGame()
        {
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
}