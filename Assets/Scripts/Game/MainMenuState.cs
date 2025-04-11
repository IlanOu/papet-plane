using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
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
            
            UpdatePlayerCountMessage();
        }
    
        public void Update()
        {
            if (_gameStarting)
                return;
                
            // Vérifier si on a assez de joueurs
            int playerCount = _gameManager.GetPlayerCount();
            
            // Mettre à jour le message avec le nombre de joueurs
            UpdatePlayerCountMessage();
            
            // Vérifier si on a assez de joueurs pour démarrer automatiquement
            if (playerCount >= _gameManager.RequiredPlayerCount)
            {
                _uiManager.HideMessagePanel();
                StartGame();
            }
        }
        
        private void UpdatePlayerCountMessage()
        {
            int playerCount = _gameManager.GetPlayerCount();
            
            if (playerCount < _gameManager.RequiredPlayerCount)
            {
                _uiManager.SetMenuMessage($"En attente de joueurs... ({playerCount}/{_gameManager.RequiredPlayerCount})");
            }
        }
        
        private void StartGame()
        {
            _gameStarting = true;
            Debug.Log("Démarrage du jeu...");
            
            _uiManager.SetMenuMessage("C'est parti !");
            
            _gameManager.Invoke("StartGame", 1.0f);
            _uiManager.HideMessagePanel();
        }
    
        public void Exit()
        {
            Debug.Log("Sortie de l'état MainMenu");
        }
    }
}