using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ReloadIndicatorController : MonoBehaviour
    {
        [SerializeField] private int playerIndex; // Index du joueur associé à cet indicateur (0, 1, etc.)
        [SerializeField] private Sprite[] reloadSprites; // Sprites d'animation de rechargement
    
        private Image imageComponent;
    
        public int PlayerIndex => playerIndex; // Propriété en lecture seule pour l'index du joueur
    
        private void Awake()
        {
            imageComponent = GetComponent<Image>();
            if (imageComponent == null)
            {
                Debug.LogError("ReloadIndicatorController nécessite un composant Image");
            }
        }
    
        // Méthode appelée par BallShooter pour mettre à jour l'animation
        public void UpdateAnimation(float progress)
        {
            if (reloadSprites == null || reloadSprites.Length == 0)
                return;
            
            // Calculer l'index du sprite en fonction de la progression
            int frameIndex = Mathf.FloorToInt(progress * (reloadSprites.Length - 1));
            frameIndex = Mathf.Clamp(frameIndex, 0, reloadSprites.Length - 1);
        
            // Appliquer le sprite
            imageComponent.sprite = reloadSprites[frameIndex];
        }
        
        // Afficher la première frame
        public void ShowFirstFrame()
        {
            if (reloadSprites != null && reloadSprites.Length > 0)
            {
                imageComponent.sprite = reloadSprites[0];
            }
        }

        // Afficher la dernière frame
        public void ShowLastFrame()
        {
            if (reloadSprites != null && reloadSprites.Length > 0)
            {
                imageComponent.sprite = reloadSprites[reloadSprites.Length - 1];
            }
        }
    }
}