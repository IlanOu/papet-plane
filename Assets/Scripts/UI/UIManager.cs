using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject inGameMenu;
        
        public void ShowInGameMenu()
        {
            mainMenu.SetActive(false);
            inGameMenu.SetActive(true);
        }
        
        public void ShowMainMenu()
        {
            mainMenu.SetActive(true);
            inGameMenu.SetActive(false);
        }
    }
}