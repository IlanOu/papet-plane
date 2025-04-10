using UI;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerLifeSystem : MonoBehaviour
    {
        [Header("Composants")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHit playerHit;

        [Header("Paramètres")]
        public int maxLife = 3;
        public int hitDamage = 1;
        
        private int _currentLife;
        
        
        [Header("Events")]
        public UnityEvent<int, int> onLifeChanged; // Envoie currentLife, maxLife
        
        [Header("Indicator")]
        public string lifeUITag = "LifeUI";
        
        private LifeUI _lifeUI;
        
        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            
            if (playerHit == null)
                playerHit = GetComponent<PlayerHit>();
            
            ResetLife();
        }
        
        private void Start()
        {
            // Trouver l'UI correspondante
            _lifeUI = FindLifeUI();
            
            if (_lifeUI != null)
            {
                // S'abonner à l'événement APRÈS avoir trouvé l'UI
                onLifeChanged.AddListener(_lifeUI.UpdateHearts);
                
                // Initialiser l'UI au démarrage
                _lifeUI.Initialize(_currentLife, maxLife);
            }
            else
            {
                Debug.LogError($"LifeUI non trouvée pour le joueur {playerController.playerIndex}");
            }   
            
            if (playerHit != null)
            {
                playerHit.onHit.AddListener(() => TakeDamage(hitDamage));
            }
        }
        
        private void OnDestroy()
        {
            // Se désabonner de l'événement
            if (_lifeUI != null)
            {
                onLifeChanged.RemoveListener(_lifeUI.UpdateHearts);
            }
        }
        
        public void TakeDamage(int damage)
        {
            _currentLife -= damage;
            onLifeChanged?.Invoke(_currentLife, maxLife);
            
            if (_currentLife <= 0)
            {
                // Kill player
                // playerController.KillPlayer();
            }
        }
        
        public void Heal(int amount)
        {
            _currentLife = Mathf.Min(_currentLife + amount, maxLife);
            onLifeChanged?.Invoke(_currentLife, maxLife);
        }
        
        public int GetCurrentLife()
        {
            return _currentLife;
        }
        
        public int GetMaxLife()
        {
            return maxLife;
        }
        
        public void ResetLife()
        {
            _currentLife = maxLife;
            onLifeChanged?.Invoke(_currentLife, maxLife);
        }
        
        public void SetLife(int life)
        {
            _currentLife = life;
            onLifeChanged?.Invoke(_currentLife, maxLife);
        }
        
        public void SetMaxLife(int life)
        {
            maxLife = life;
            onLifeChanged?.Invoke(_currentLife, maxLife);
        }
        
        private LifeUI FindLifeUI()
        {
            GameObject[] lifeUIs = GameObject.FindGameObjectsWithTag(lifeUITag);

            foreach (GameObject lifeUIObj in lifeUIs)
            {
                LifeUI lifeUI = lifeUIObj.GetComponent<LifeUI>();
                if (lifeUI != null && lifeUI.playerIndex == playerController.playerIndex)
                {
                    return lifeUI;
                }
            }
            
            return null;
        }
    }
}