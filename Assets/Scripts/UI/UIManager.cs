using Game;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Écrans de menu")]
        [SerializeField] private GameObject introMenu;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject inGameMenu;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject gameOverMenu;
        
        // Méthodes d'affichage des menus
        public void ShowIntroMenu()
        {
            HideAllMenus();
            introMenu.SetActive(true);
        }
        
        public void ShowMainMenu()
        {
            HideAllMenus();
            mainMenu.SetActive(true);
        }
        
        public void ShowInGameMenu()
        {
            HideAllMenus();
            inGameMenu.SetActive(true);
        }
        
        public void ShowPauseMenu()
        {
            pauseMenu.SetActive(true);
        }
        
        public void ShowGameOverMenu()
        {
            HideAllMenus();
            gameOverMenu.SetActive(true);
        }
        
        // Méthodes pour les boutons
        public void OnStartButtonClicked()
        {
            GameManager.Instance.StartGame();
        }
        
        public void OnPauseButtonClicked()
        {
            GameManager.Instance.PauseGame();
        }
        
        public void OnResumeButtonClicked()
        {
            GameManager.Instance.ResumeGame();
        }
        
        public void OnRestartButtonClicked()
        {
            GameManager.Instance.RestartGame();
        }
        
        public void OnMainMenuButtonClicked()
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        
        public void OnQuitButtonClicked()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        // Méthode utilitaire
        private void HideAllMenus()
        {
            // introMenu.SetActive(false);
            mainMenu.SetActive(false);
            inGameMenu.SetActive(false);
            // pauseMenu.SetActive(false);
            // gameOverMenu.SetActive(false);
        }
    }
}