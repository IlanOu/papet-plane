using UI;
using UnityEngine;

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
}