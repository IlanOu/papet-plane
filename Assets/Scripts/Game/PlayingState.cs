using UI;
using UnityEngine;

namespace Game
{
    public class PlayingState : IState
    {
        private GameManager _gameManager;
        private UIManager _uiManager;
        
        // Constante pour le délai d'inactivité (2 minutes en secondes)
        private const float INACTIVITY_TIMEOUT = 30;
        
        // Variables pour gérer l'avertissement d'inactivité
        private bool _inactivityWarningShown = false;
        private float _warningThreshold = 15f; // Afficher un avertissement 30 secondes avant le redémarrage
    
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
            
            // Réinitialiser les variables d'inactivité
            _inactivityWarningShown = false;
            
            // Mettre à jour le lastTimeInput au moment d'entrer dans l'état Playing
            _gameManager.lastTimeInput = Time.time;
        
            _uiManager.ShowInGameMenu(true, onComplete: () => _gameManager.SetupGame());
        }
    
        public void Update()
        {
            // Calculer le temps écoulé depuis la dernière interaction
            float timeSinceLastInput = Time.time - _gameManager.lastTimeInput;
            
            // Vérifier si on approche du délai d'inactivité
            if (timeSinceLastInput > (INACTIVITY_TIMEOUT - _warningThreshold) && !_inactivityWarningShown)
            {
                // Afficher un avertissement d'inactivité
                _uiManager.SetMenuMessage($"Inactivité détectée! Redémarrage dans {_warningThreshold} secondes...");
                _inactivityWarningShown = true;
            }
            
            // Vérifier si le délai d'inactivité est dépassé
            if (timeSinceLastInput >= INACTIVITY_TIMEOUT)
            {
                Debug.Log("Inactivité détectée pendant 2 minutes. Redémarrage du jeu...");
                _uiManager.SetMenuMessage("Redémarrage pour inactivité...");
                
                // Attendre un court instant pour que le message soit visible
                _gameManager.Invoke("RestartGame", 2f);
            }
            
            // Si une interaction a lieu après l'avertissement, cacher le message
            if (_inactivityWarningShown && timeSinceLastInput < (INACTIVITY_TIMEOUT - _warningThreshold))
            {
                _uiManager.HideMessagePanel();
                _inactivityWarningShown = false;
            }
            
            // Autres logiques de mise à jour du jeu...
        }
    
        public void Exit()
        {
            Debug.Log("Sortie de l'état Playing");
            
            // Cacher le message d'inactivité si présent
            if (_inactivityWarningShown)
            {
                _uiManager.HideMessagePanel();
                _inactivityWarningShown = false;
            }
        }
    }
}