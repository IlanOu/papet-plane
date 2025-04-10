using UI;
using UnityEngine;

namespace Game
{
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
        
        
            // Régler le timeScale
            Time.timeScale = 1f;
        
            _uiManager.ShowInGameMenu(true, onComplete: () => _gameManager.SetupGame());
            
            // // Afficher l'UI de jeu
            // _uiManager.ShowInGameMenu(true);
            //
            // // Configurer le jeu
            // _gameManager.SetupGame();
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
}