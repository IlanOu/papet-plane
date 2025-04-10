using UI;
using UnityEngine;

namespace Game
{
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
}