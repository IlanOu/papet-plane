using UI;
using UnityEngine;

namespace Game
{
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
            _uiManager.ShowGameOverMenu(winnerIndex: _gameManager.winnerIndex);
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
}