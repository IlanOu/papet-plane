using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ReloadIndicatorController : MonoBehaviour
    {
        [SerializeField] private int playerIndex;
        [SerializeField] private Sprite[] reloadSprites;
        
        [Header("Stack Position")]
        [SerializeField] private Vector2 stackBasePosition = new Vector2(-20f, 0f); // Position de base de la pile par rapport à l'indicateur
        
        [Header("Normal Ammo")]
        [SerializeField] private float normalOffsetX = 10f; // Espacement horizontal entre les sprites normaux
        [SerializeField] private float normalOffsetY = 0f;  // Espacement vertical entre les sprites normaux
        
        [Header("Charged Ammo")]
        [SerializeField] private float chargedOffsetX = 10f; // Espacement horizontal pour munitions chargées
        [SerializeField] private float chargedOffsetY = 5f;  // Espacement vertical pour munitions chargées
        
        [Header("Spacing Between Types")]
        [SerializeField] private float typeSeparationX = 15f; // Espace horizontal supplémentaire entre les types de munitions
        [SerializeField] private float typeSeparationY = 5f;  // Espace vertical supplémentaire entre les types de munitions
        
        [Header("Reloading Shift")]
        [SerializeField] private Vector2 reloadingShift = new Vector2(30f, 0f); // Décalage supplémentaire pendant le rechargement
        
        private Image imageComponent;
        private Image[] ammoStackImages; // Tableau pour stocker les images de la pile de munitions
        private RectTransform rectTransform;
        private Vector2 originalPosition;
        private Transform stackParent; // Parent pour tous les sprites de la pile
        
        public int PlayerIndex => playerIndex;
    
        private void Awake()
        {
            imageComponent = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            originalPosition = rectTransform.anchoredPosition;
            
            if (imageComponent == null)
            {
                Debug.LogError("ReloadIndicatorController nécessite un composant Image");
            }
        }
        
        // Initialise la pile de munitions
        public void InitializeAmmoStack(int magazineCapacity)
        {
            // Supprimer l'ancien parent de pile s'il existe
            if (stackParent != null)
            {
                Destroy(stackParent.gameObject);
            }
            
            // Créer un nouveau parent pour la pile
            GameObject stackParentObj = new GameObject("AmmoStack_Parent");
            stackParentObj.transform.SetParent(transform.parent);
            stackParentObj.transform.SetSiblingIndex(transform.GetSiblingIndex()); // Placer derrière l'indicateur principal
            
            // Ajouter un RectTransform au parent
            RectTransform stackParentRT = stackParentObj.AddComponent<RectTransform>();
            stackParentRT.anchorMin = rectTransform.anchorMin;
            stackParentRT.anchorMax = rectTransform.anchorMax;
            stackParentRT.pivot = rectTransform.pivot;
            stackParentRT.sizeDelta = rectTransform.sizeDelta;
            
            // Positionner le parent par rapport à l'indicateur principal
            stackParentRT.anchoredPosition = originalPosition + stackBasePosition;
            
            stackParent = stackParentObj.transform;
            
            // Supprimer les anciennes images de pile si elles existent
            if (ammoStackImages != null)
            {
                foreach (var img in ammoStackImages)
                {
                    if (img != null && img.gameObject != null)
                    {
                        Destroy(img.gameObject);
                    }
                }
            }
            
            // Créer un nouveau tableau pour les images de munitions
            // On crée magazineCapacity-1 images car l'indicateur principal compte déjà comme une munition
            ammoStackImages = new Image[magazineCapacity > 0 ? magazineCapacity - 1 : 0];
            
            // Créer des images pour chaque munition supplémentaire
            for (int i = 0; i < ammoStackImages.Length; i++)
            {
                GameObject ammoObj = new GameObject($"AmmoStack_{i}");
                ammoObj.transform.SetParent(stackParent);
                
                RectTransform rt = ammoObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = rectTransform.sizeDelta;
                
                // Position initiale (sera mise à jour par UpdateAmmoDisplay)
                rt.anchoredPosition = Vector2.zero; // Sera positionné relativement au parent
                
                Image img = ammoObj.AddComponent<Image>();
                img.sprite = reloadSprites[0]; // Sprite initial
                img.color = new Color(1, 1, 1, 0.5f); // Semi-transparent
                
                ammoStackImages[i] = img;
            }
        }
        
        // Met à jour l'affichage des munitions
        public void UpdateAmmoDisplay(int currentAmmo, bool isReloading, int shotsBeforeReload, bool isCharged, int shotsSinceLastReload)
        {
            if (ammoStackImages == null || stackParent == null) return;
            
            // Déplacer le parent de la pile pendant le rechargement
            RectTransform stackParentRT = stackParent.GetComponent<RectTransform>();
            if (isReloading)
            {
                stackParentRT.anchoredPosition = originalPosition + stackBasePosition + reloadingShift;
            }
            else
            {
                stackParentRT.anchoredPosition = originalPosition + stackBasePosition;
            }
            
            // Afficher uniquement les images correspondant aux munitions restantes, moins 1 car l'indicateur principal compte
            int stackAmmoToShow = Mathf.Max(0, currentAmmo - 1);
            
            // Calculer combien de munitions chargées restent
            int remainingChargedShots = isCharged ? Mathf.Max(0, shotsBeforeReload - shotsSinceLastReload - 1) : 0;
            
            for (int i = 0; i < ammoStackImages.Length; i++)
            {
                if (ammoStackImages[i] != null)
                {
                    bool shouldShow = i < stackAmmoToShow;
                    ammoStackImages[i].gameObject.SetActive(shouldShow);
                    
                    if (shouldShow)
                    {
                        RectTransform rt = ammoStackImages[i].GetComponent<RectTransform>();
                        
                        // Les premières munitions dans la pile (jusqu'à remainingChargedShots) sont chargées
                        bool isAmmoCharged = i < remainingChargedShots;
                        
                        if (isAmmoCharged)
                        {
                            // Utiliser le dernier sprite pour les munitions chargées
                            ammoStackImages[i].sprite = reloadSprites[reloadSprites.Length - 1];
                            
                            // Positionner les munitions chargées à partir du début
                            rt.anchoredPosition = new Vector2(
                                chargedOffsetX * i,
                                chargedOffsetY * i);
                        }
                        else
                        {
                            // Utiliser le premier sprite pour les munitions non chargées
                            ammoStackImages[i].sprite = reloadSprites[0];
                            
                            // Ajouter l'espace entre les types et calculer la position relative
                            int indexAfterCharged = i - remainingChargedShots;
                            
                            // Si c'est la première munition non chargée, ajouter l'espace de séparation
                            float separationX = (indexAfterCharged == 0 && remainingChargedShots > 0) ? typeSeparationX : 0;
                            float separationY = (indexAfterCharged == 0 && remainingChargedShots > 0) ? typeSeparationY : 0;
                            
                            // Position de base des munitions chargées, plus l'espace entre types, plus l'offset pour cette munition
                            rt.anchoredPosition = new Vector2(
                                chargedOffsetX * remainingChargedShots + separationX + normalOffsetX * indexAfterCharged,
                                chargedOffsetY * remainingChargedShots + separationY + normalOffsetY * indexAfterCharged);
                        }
                    }
                }
            }
        }
        
        public void UpdateAnimation(float progress)
        {
            if (reloadSprites == null || reloadSprites.Length == 0)
                return;
            
            int frameIndex = Mathf.FloorToInt(progress * (reloadSprites.Length - 1));
            frameIndex = Mathf.Clamp(frameIndex, 0, reloadSprites.Length - 1);
            
            imageComponent.sprite = reloadSprites[frameIndex];
        }
        
        public void ShowFirstFrame()
        {
            if (reloadSprites != null && reloadSprites.Length > 0)
            {
                imageComponent.sprite = reloadSprites[0];
            }
        }

        public void ShowLastFrame()
        {
            if (reloadSprites != null && reloadSprites.Length > 0)
            {
                imageComponent.sprite = reloadSprites[reloadSprites.Length - 1];
            }
        }
    }
}