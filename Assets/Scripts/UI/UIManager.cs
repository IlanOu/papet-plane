using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game;  // Assurez-vous que le namespace Game est correctement référencé

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Écrans de menu")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject inGameMenu;
        [SerializeField] private GameObject gameOverMenu;
        
        [Header("Configs")]
        [Tooltip("Images qui auront la couleur du joueur 1")]
        [SerializeField] private Image[] imagesPlayer1;
        [Tooltip("Images qui auront la couleur du joueur 2")]
        [SerializeField] private Image[] imagePlayer2;

        [Header("Win Screen Configs")]
        [SerializeField] private List<GameObject> displayForP1Winner;
        [SerializeField] private List<GameObject> displayForP2Winner;
        [SerializeField] private List<GameObject> displayForP1Loser;
        [SerializeField] private List<GameObject> displayForP2Loser;
        
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
            
            GameManager.Instance.uiManager = this;
        }

        #region Méthodes d'affichage avec Transition (Fade Out / Fade In)

        /// <summary>
        /// Transition entre le menu actuellement affiché et un nouveau menu.
        /// Si aucun menu n'est actif, seule l'apparition du nouveau menu sera animée.
        /// </summary>
        /// <param name="newMenu">Le nouveau menu à afficher</param>
        /// <param name="fadeOutDuration">Durée du fade out du menu actuel</param>
        /// <param name="fadeInDuration">Durée du fade in du nouveau menu</param>
        public IEnumerator TransitionBetweenMenus(GameObject newMenu, float fadeOutDuration, float fadeInDuration, System.Action onComplete = null)
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
            
            onComplete?.Invoke();
        }

        #endregion

        #region Méthodes d'affichage des menus

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
        public Coroutine ShowInGameMenu(bool useTransition=false, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f, System.Action onComplete = null)
        {
            if (useTransition)
            {
                return StartCoroutine(TransitionBetweenMenus(inGameMenu, fadeOutDuration, fadeInDuration, onComplete));
            }
            else
            {
                HideAllMenus();
                inGameMenu.SetActive(true);
                currentMenu = inGameMenu;
                onComplete?.Invoke();
                return null;
            }
        }
        

        /// <summary>
        /// Affiche le menu Game Over.
        /// </summary>
        public void ShowGameOverMenu(bool useTransition=false, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f, int winnerIndex = -1)
        {
            if (winnerIndex == -1)
            {
                Debug.LogError("Le joueur gagnant doit avoir un index.");
            }
            
            foreach (var displayP1W in displayForP1Winner)
            {
                displayP1W.SetActive(winnerIndex == 0);
            }
            foreach (var displayP2W in displayForP2Winner)
            {
                displayP2W.SetActive(winnerIndex == 1);
            }
            foreach (var displayP1L in displayForP1Loser)
            {
                displayP1L.SetActive(winnerIndex == 1);
            }
            foreach (var displayP2L in displayForP2Loser)
            {
                displayP2L.SetActive(winnerIndex == 0);
            }
            
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

        #region Messages

        [Header("Messages")]
        [SerializeField] private TextMeshProUGUI menuMessageText;
        [SerializeField] private GameObject messagePanel; // Le panneau contenant le texte

        /// <summary>
        /// Définit le message affiché dans le menu principal.
        /// Si le message est vide ou null, le panneau sera caché.
        /// </summary>
        /// <param name="message">Le message à afficher</param>
        public void SetMenuMessage(string message)
        {
            if (menuMessageText != null)
            {
                // Vérifier si le message est vide
                bool hasMessage = !string.IsNullOrEmpty(message);
        
                // Mettre à jour le texte si nécessaire
                if (hasMessage)
                {
                    menuMessageText.text = message;
                }
        
                // Afficher ou cacher le panneau
                if (messagePanel != null)
                {
                    messagePanel.SetActive(hasMessage);
                }
                else
                {
                    // Si pas de panneau défini, gérer juste la visibilité du texte
                    menuMessageText.gameObject.SetActive(hasMessage);
                }
            }
            else
            {
                Debug.LogWarning("menuMessageText n'est pas assigné dans l'inspecteur.");
            }
        }

        /// <summary>
        /// Cache le panneau de message.
        /// </summary>
        public void HideMessagePanel()
        {
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }
            else if (menuMessageText != null)
            {
                menuMessageText.gameObject.SetActive(false);
            }
        }

        #endregion
        
        #region Méthode utilitaire pour masquer tous les menus

        /// <summary>
        /// Désactive tous les menus connus dans le script.
        /// </summary>
        private void HideAllMenus()
        {
            if (mainMenu != null)
                mainMenu.SetActive(false);
            if (inGameMenu != null)
                inGameMenu.SetActive(false);
            if (gameOverMenu != null)
                gameOverMenu.SetActive(false);
        }

        #endregion
    }
}
