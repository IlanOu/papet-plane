using System.Collections;
using Bonus;
using Game;
using Player;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Projectile
{
    public class BallShooter : MonoBehaviour
    {
        [Header("Ball Settings")]
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float shootForce = 10f;
    
        [Header("Camera")]
        [SerializeField] private GameObject playerCamera;
    
        [Header("Reload Settings")]
        public float reloadTime = 3f; // Durée du rechargement (chargement du tir)
    
        [Header("Ammo Settings")]
        public int magazineCapacity = 5; // Nombre maximal de munitions dans le chargeur
        private int _currentAmmo;                          // Munitions actuellement disponibles dans le chargeur
    
        // Nombre de tirs autorisés avant de devoir recharger l'arme (même si le chargeur n'est pas vide)
        [SerializeField] private int shotsBeforeReload = 1;
        // Compteur de tirs effectués depuis la dernière recharge (état "chargé")
        private int _shotsSinceLastReload = 0;
    
        // État indiquant si le tir est préparé (après l'animation de recharge)
        private bool _isCharged = false;
    
        [Header("UI References")]
        [SerializeField] private string reloadIndicatorTag = "ReloadIndicator"; // Tag utilisé pour trouver l'indicateur de rechargement
    
        private BonusManager _bonusManager;
        private bool _isReloading = false; // Indique si le processus de recharge est en cours
        private Coroutine _reloadCoroutine;

        private Image _reloadIndicator; // Référence à l'indicateur pour ce joueur
        private ReloadIndicatorController _indicatorController; // Contrôleur pour l'animation de l'indicateur

        [SerializeField] PlayerController playerController;
        
        [HideInInspector] public UnityEvent reloading;
    
        private void Awake()
        {
            _bonusManager = GetComponent<BonusManager>();
            if (_bonusManager == null)
            {
                _bonusManager = gameObject.AddComponent<BonusManager>();
            }
            
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }
        
        private void Start()
        {   
            FillAmmo(magazineCapacity);
            _isCharged = true;
            _shotsSinceLastReload = 0;
    
            FindReloadIndicator();
    
            // Initialiser la pile de munitions
            if (_indicatorController != null)
            {
                _indicatorController.InitializeAmmoStack(magazineCapacity);
                _indicatorController.UpdateAmmoDisplay(_currentAmmo, false, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
            }
    
            HideReloadIndicator();
        }

        private void FindReloadIndicator()
        {
            GameObject[] indicators = GameObject.FindGameObjectsWithTag(reloadIndicatorTag);
        
            foreach (GameObject indicator in indicators)
            {
                ReloadIndicatorController controller = indicator.GetComponent<ReloadIndicatorController>();
                if (controller != null && controller.PlayerIndex == playerController.playerIndex)
                {
                    _reloadIndicator = indicator.GetComponent<Image>();
                    _indicatorController = controller;
                    break;
                }
            }
        
            if (_reloadIndicator == null)
            {
                Debug.LogWarning($"Aucun indicateur de rechargement trouvé pour le joueur {playerController.playerIndex}");
            }
        }
    
        private void HideReloadIndicator()
        {
            if (_reloadIndicator != null)
            {
                if (_currentAmmo <= 0)
                {
                    // Si plus de munitions, cacher complètement l'indicateur
                    _reloadIndicator.gameObject.SetActive(false);
            
                    // Mettre à jour l'affichage des munitions (toutes cachées)
                    if (_indicatorController != null)
                    {
                        _indicatorController.UpdateAmmoDisplay(0, false, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
                    }
                }
                else if (_isCharged)
                {
                    // Si l'arme est chargée, afficher la dernière frame
                    _reloadIndicator.gameObject.SetActive(true);
                    _reloadIndicator.fillAmount = 1f;
                    if (_indicatorController != null)
                    {
                        _indicatorController.ShowLastFrame();
                        _indicatorController.UpdateAmmoDisplay(_currentAmmo, false, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
                    }
                }
                else
                {
                    // Si l'arme n'est pas chargée, afficher la première frame
                    _reloadIndicator.gameObject.SetActive(true);
                    _reloadIndicator.fillAmount = 0f;
                    if (_indicatorController != null)
                    {
                        _indicatorController.ShowFirstFrame();
                        _indicatorController.UpdateAmmoDisplay(_currentAmmo, false, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
                    }
                }
            }
        }
        
        private void ShowReloadIndicator()
        {
            if (_reloadIndicator != null)
            {
                reloading.Invoke();
                _reloadIndicator.gameObject.SetActive(true);
                _reloadIndicator.fillAmount = 0f;
            }
        }
    
        // Méthode appelée par l'Input pour tirer
        public void Shoot(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (_isReloading)
                    return;
        
                if (_currentAmmo <= 0)
                {
                    Debug.Log("Chargeur vide, impossible de tirer !");
                    return;
                }
        
                if (!_isCharged)
                {
                    Debug.Log("Le tir n'est pas chargé, recharge nécessaire.");
                    StartReload();
                    return;
                }
        
                FireBall();
        
                _shotsSinceLastReload++;
                _currentAmmo--;
                Debug.Log($"Tir effectué ! Munitions restantes : {_currentAmmo}");
        
                // Mettre à jour l'affichage des munitions après le tir
                if (_indicatorController != null)
                {
                    _indicatorController.UpdateAmmoDisplay(_currentAmmo, false, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
                }
        
                if (_currentAmmo <= 0)
                {
                    HideReloadIndicator(); // Cela cachera complètement l'indicateur
                }
        
                if (_shotsSinceLastReload >= shotsBeforeReload)
                {
                    _isCharged = false;
                    Debug.Log("Nombre maximum de tirs atteint, recharge nécessaire pour préparer le tir suivant.");
                    HideReloadIndicator(); // Mettre à jour l'affichage pour montrer la première frame
                }
            }
        }
    
        private void FireBall()
        {
            GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
            GameManager.Instance.instantiatedThings.Add(ball);
            
            BallBehaviour ballBehaviour = ball.GetComponent<BallBehaviour>();
            if (ballBehaviour != null)
            {
                ballBehaviour.SetBonus(_bonusManager.GetCurrentBonus());
                ballBehaviour.ownerIndex = playerController.playerIndex;
            }
            
            Vector3 shootDirection = playerCamera.transform.forward;
            
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
            
                Transform quadTransform = ball.transform.Find("DirectionQuad");
                if (quadTransform != null)
                {
                    OrientQuad(quadTransform, shootDirection);
                }
            }
        }
    
        private void StartReload()
        {
            if (_currentAmmo <= 0)
            {
                Debug.Log("Impossible de recharger, chargeur vide.");
                return;
            }
        
            if (!_isReloading)
            {
                _isReloading = true;
                ShowReloadIndicator();
                _reloadCoroutine = StartCoroutine(ReloadCoroutine());
            }
        }

        private IEnumerator ReloadCoroutine()
        {
            float elapsedTime = 0f;

            // Mettre à jour l'affichage des munitions pour le mode rechargement
            if (_indicatorController != null)
            {
                _indicatorController.UpdateAmmoDisplay(_currentAmmo, true, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
            }
    
            while (elapsedTime < reloadTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / reloadTime;
        
                if (_reloadIndicator != null)
                {
                    _reloadIndicator.fillAmount = progress;
                    if (_indicatorController != null)
                    {
                        _indicatorController.UpdateAnimation(progress);
                    }
                }
        
                yield return null;
            }
    
            // Fin du rechargement : le tir est prêt
            _isCharged = true;
            _isReloading = false;
            _shotsSinceLastReload = 0;
            Debug.Log("Tir chargé, prêt à tirer.");
    
            // Remettre l'affichage des munitions en mode normal
            if (_indicatorController != null)
            {
                _indicatorController.UpdateAmmoDisplay(_currentAmmo, false, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
            }
    
            HideReloadIndicator();
        }
    
        public void CancelReload()
        {
            if (_isReloading && _reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                _isReloading = false;
        
                // Remettre l'affichage des munitions en mode normal
                if (_indicatorController != null)
                {
                    _indicatorController.UpdateAmmoDisplay(_currentAmmo, false, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
                }
        
                HideReloadIndicator();
            }
        }
    
        public void InstantReload()
        {
            CancelReload();
            if (_currentAmmo > 0)
            {
                _isCharged = true;
                _shotsSinceLastReload = 0;
                Debug.Log("Recharge instantanée effectuée, tir chargé.");
        
                // Mettre à jour l'affichage des munitions
                if (_indicatorController != null)
                {
                    _indicatorController.UpdateAmmoDisplay(_currentAmmo, false, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
                }
            }
        }
    
        private void OrientQuad(Transform quadTransform, Vector3 direction)
        {
            Quaternion originalRotation = quadTransform.rotation;
            quadTransform.rotation = Quaternion.LookRotation(direction);
            Vector3 eulerAngles = quadTransform.eulerAngles;
            quadTransform.eulerAngles = new Vector3(originalRotation.eulerAngles.x, eulerAngles.y, originalRotation.eulerAngles.z);
        }
    
        public float GetReloadTime() => reloadTime;
    
        // Retourne true si le tir est préparé (chargé)
        public bool IsLoaded() => _isCharged;
    
        public bool IsReloading() => _isReloading;
    
        public void FillAmmo(int amount)
        {
            bool wasEmpty = _currentAmmo <= 0;
            _currentAmmo = amount;
    
            // Mettre à jour l'affichage des munitions
            if (_indicatorController != null)
            {
                _indicatorController.UpdateAmmoDisplay(_currentAmmo, _isReloading, shotsBeforeReload, _isCharged, _shotsSinceLastReload);
            }
    
            // Si on avait 0 munitions avant et qu'on en a maintenant, mettre à jour l'indicateur
            if (wasEmpty && _currentAmmo > 0)
            {
                HideReloadIndicator(); // Cette méthode affichera maintenant l'indicateur avec la frame appropriée
            }
        }
    }
}
