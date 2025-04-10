using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game;  // Assurez-vous que le namespace Game est correctement référencé

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
        
        [Header("Configs")]
        [Tooltip("Images qui auront la couleur du joueur 1")]
        [SerializeField] private Image[] imagesPlayer1;
        [Tooltip("Images qui auront la couleur du joueur 2")]
        [SerializeField] private Image[] imagePlayer2;

        // Référence au menu actuellement affiché
        private GameObject currentMenu;

        private void Start()
        {
            // Configuration des couleurs d'équipe sur les images
            foreach (var img in imagesPlayer1)
            {
                img.color = GameManager.Instance.teamColors[0];
            }
            foreach (var img in imagePlayer2)
            {
                img.color = GameManager.Instance.teamColors[1];
            }
            
            // Optionnel : vous pouvez définir un menu par défaut au démarrage
            ShowMainMenu(false);
        }

        #region Méthodes d'affichage avec Transition (Fade Out / Fade In)

        /// <summary>
        /// Transition entre le menu actuellement affiché et un nouveau menu.
        /// Si aucun menu n'est actif, seule l'apparition du nouveau menu sera animée.
        /// </summary>
        /// <param name="newMenu">Le nouveau menu à afficher</param>
        /// <param name="fadeOutDuration">Durée du fade out du menu actuel</param>
        /// <param name="fadeInDuration">Durée du fade in du nouveau menu</param>
        public IEnumerator TransitionBetweenMenus(GameObject newMenu, float fadeOutDuration, float fadeInDuration)
        {
            // Si un menu est déjà affiché, réaliser un fade out
            if(currentMenu != null)
            {
                CanvasGroup currentCG = currentMenu.GetComponent<CanvasGroup>();
                if (currentCG != null)
                {
                    float timer = 0f;
                    while (timer < fadeOutDuration)
                    {
                        timer += Time.deltaTime;
                        currentCG.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
                        yield return null;
                    }
                    currentCG.alpha = 0f;
                }
                // Désactiver le menu après le fade out
                currentMenu.SetActive(false);
            }

            // Activer le nouveau menu et l'assigner comme menu courant
            newMenu.SetActive(true);
            currentMenu = newMenu;
            
            // Réaliser le fade in du nouveau menu
            CanvasGroup newCG = newMenu.GetComponent<CanvasGroup>();
            if (newCG != null)
            {
                // On initialise l'opacité à 0
                newCG.alpha = 0f;
                float timer = 0f;
                while (timer < fadeInDuration)
                {
                    timer += Time.deltaTime;
                    newCG.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
                    yield return null;
                }
                newCG.alpha = 1f;
            }
        }

        #endregion

        #region Méthodes d'affichage des menus

        /// <summary>
        /// Affiche l'intro. Possibilité de transition ou affichage instantané.
        /// </summary>
        public void ShowIntroMenu(bool useTransition=false, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f)
        {
            if (useTransition)
            {
                StartCoroutine(TransitionBetweenMenus(introMenu, fadeOutDuration, fadeInDuration));
            }
            else
            {
                HideAllMenus();
                introMenu.SetActive(true);
                currentMenu = introMenu;
            }
        }

        /// <summary>
        /// Affiche le menu principal.
        /// </summary>
        public void ShowMainMenu(bool useTransition=false, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f)
        {
            if (useTransition)
            {
                StartCoroutine(TransitionBetweenMenus(mainMenu, fadeOutDuration, fadeInDuration));
            }
            else
            {
                HideAllMenus();
                mainMenu.SetActive(true);
                currentMenu = mainMenu;
            }
        }

        /// <summary>
        /// Affiche le menu en jeu.
        /// </summary>
        public void ShowInGameMenu(bool useTransition=false, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f)
        {
            if (useTransition)
            {
                StartCoroutine(TransitionBetweenMenus(inGameMenu, fadeOutDuration, fadeInDuration));
            }
            else
            {
                HideAllMenus();
                inGameMenu.SetActive(true);
                currentMenu = inGameMenu;
            }
        }

        /// <summary>
        /// Affiche le menu de pause (exemple sans transition, mais adaptable).
        /// </summary>
        public void ShowPauseMenu(bool useTransition = false, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f)
        {
            if (useTransition)
            {
                StartCoroutine(TransitionBetweenMenus(pauseMenu, fadeOutDuration, fadeInDuration));
            }
            else
            {
                HideAllMenus();
                pauseMenu.SetActive(true);
                currentMenu = pauseMenu;
            }
        }

        /// <summary>
        /// Affiche le menu Game Over.
        /// </summary>
        public void ShowGameOverMenu(bool useTransition=false, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f)
        {
            if (useTransition)
            {
                StartCoroutine(TransitionBetweenMenus(gameOverMenu, fadeOutDuration, fadeInDuration));
            }
            else
            {
                HideAllMenus();
                gameOverMenu.SetActive(true);
                currentMenu = gameOverMenu;
            }
        }

        #endregion

        #region Méthodes pour les boutons

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

        #endregion

        #region Méthode utilitaire pour masquer tous les menus

        /// <summary>
        /// Désactive tous les menus connus dans le script.
        /// </summary>
        private void HideAllMenus()
        {
            if (introMenu != null)
                introMenu.SetActive(false);
            if (mainMenu != null)
                mainMenu.SetActive(false);
            if (inGameMenu != null)
                inGameMenu.SetActive(false);
            if (pauseMenu != null)
                pauseMenu.SetActive(false);
            if (gameOverMenu != null)
                gameOverMenu.SetActive(false);
        }

        #endregion
    }
}
