using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LifeUI : MonoBehaviour
    {
        [Header("Player")]
        public int playerIndex; // Doit correspondre à l'index du joueur
        
        [Header("Sprites")]
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite emptyHeart;
        
        [Header("References")]
        [SerializeField] private Image heartPrefab;
        [SerializeField] private RectTransform heartsContainer;

        private Image[] _hearts;
        
        private void Awake()
        {
            if (heartsContainer == null)
                heartsContainer = transform as RectTransform;
        }
        
        public void Initialize(int currentLife, int maxLife)
        {
            Debug.Log($"LifeUI initialized for player {playerIndex} with {currentLife}/{maxLife} hearts");
            
            // Créer ou ajuster le tableau de cœurs
            if (_hearts == null || _hearts.Length != maxLife)
            {
                // Nettoyer les cœurs existants
                if (_hearts != null)
                {
                    foreach (var heart in _hearts)
                    {
                        if (heart != null)
                            Destroy(heart.gameObject);
                    }
                }
                
                _hearts = new Image[maxLife];
                
                // Créer de nouveaux cœurs
                for (int i = 0; i < maxLife; i++)
                {
                    _hearts[i] = Instantiate(heartPrefab, heartsContainer);
                }
            }
            
            // Mettre à jour l'affichage
            UpdateHearts(currentLife, maxLife);
        }
        
        public void UpdateHearts(int currentLife, int maxLife)
        {
            if (_hearts == null)
            {
                Debug.LogWarning($"Hearts array is null for player {playerIndex}. Initializing...");
                Initialize(currentLife, maxLife);
                return;
            }
            
            // S'assurer que nous avons le bon nombre de cœurs
            if (_hearts.Length != maxLife)
            {
                Debug.LogWarning($"Hearts count mismatch for player {playerIndex}. Reinitializing...");
                Initialize(currentLife, maxLife);
                return;
            }
            
            // Mettre à jour l'apparence de chaque cœur
            for (int i = 0; i < _hearts.Length; i++)
            {
                if (_hearts[i] != null)
                {
                    _hearts[i].sprite = i < currentLife ? fullHeart : emptyHeart;
                }
                else
                {
                    Debug.LogError($"Heart at index {i} is null for player {playerIndex}");
                }
            }
        }
    }
}