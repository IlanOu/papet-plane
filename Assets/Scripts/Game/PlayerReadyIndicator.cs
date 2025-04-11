using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace UI
{
    public class PlayerReadyIndicator : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI playerLabel;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image controllerIcon;
        
        [Header("États")]
        [SerializeField] private Color joinedColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color readyColor = new Color(0, 1, 0);
        
        [Header("Icônes")]
        [SerializeField] private Sprite keyboardIcon;
        [SerializeField] private Sprite gamepadIcon;
        
        private int _playerIndex;
        
        public void Initialize(int playerIndex, InputDevice device)
        {
            _playerIndex = playerIndex;
            playerLabel.text = $"JOUEUR {playerIndex + 1}";
            
            // Définir l'icône appropriée selon le type de périphérique
            if (device is Keyboard)
                controllerIcon.sprite = keyboardIcon;
            else
                controllerIcon.sprite = gamepadIcon;
        }
        
        public void SetJoinedState()
        {
            backgroundImage.color = joinedColor;
            statusText.text = "APPUYEZ SUR START";
        }
        
        public void SetReadyState()
        {
            backgroundImage.color = readyColor;
            statusText.text = "PRÊT !";
        }
    }
}