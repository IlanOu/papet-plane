using UI;
using UnityEngine;
using System.Collections;

namespace Game
{
    public class GameOverState : IState
    {
        private GameManager _gameManager;
        private UIManager _uiManager;
        private bool _restartScheduled = false;
        private float _restartDelay = 5f;
        private float _restartTimer;
    
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
            _uiManager.ShowGameOverMenu(winnerIndex: _gameManager.winnerIndex);
            
            // Initialiser le timer de redémarrage
            _restartTimer = _restartDelay;
            _restartScheduled = true;
            
            // Afficher le message initial
            UpdateRestartMessage();
        }
    
        public void Update()
        {
            if (_restartScheduled)
            {
                // Décrémenter le timer
                _restartTimer -= Time.deltaTime;
                
                // Mettre à jour le message avec le temps restant
                UpdateRestartMessage();
                
                // Vérifier si le temps est écoulé
                if (_restartTimer <= 0)
                {
                    _restartScheduled = false;
                    RestartGame();
                }
            }
        }
        
        private void UpdateRestartMessage()
        {
            // Arrondir au nombre entier le plus proche pour l'affichage
            int secondsRemaining = Mathf.CeilToInt(_restartTimer);
            
            // Mettre à jour le texte
            if (secondsRemaining > 0)
            {
                _uiManager.SetMenuMessage($"La partie va redemarrer dans {secondsRemaining} seconde{(secondsRemaining > 1 ? "s" : "")}...");
            }
            else
            {
                _uiManager.SetMenuMessage("Redemarrage...");
            }
        }
        
        private void RestartGame()
        {
            Debug.Log("Redémarrage de la partie");
            _gameManager.RestartGame();
        }
    
        public void Exit()
        {
            Debug.Log("Sortie de l'état GameOver");
            
            // Cacher le message de redémarrage
            _uiManager.HideMessagePanel();
        }
    }
}