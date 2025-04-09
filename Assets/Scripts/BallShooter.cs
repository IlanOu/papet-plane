using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class BallShooter : MonoBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float shootForce = 10f;
    
    [Header("Camera")]
    [SerializeField] private GameObject playerCamera;
    
    [Header("Reload Settings")]
    [SerializeField] private float reloadTime = 3f; // Temps de rechargement en secondes
    
    [Header("UI References")]
    [SerializeField] private string reloadIndicatorTag = "ReloadIndicator"; // Tag pour trouver les indicateurs
    
    private BonusManager bonusManager;
    private bool isLoaded = true; // Indique si l'arme est chargée
    private bool isReloading = false; // Indique si le rechargement est en cours
    private Coroutine reloadCoroutine;
    private int playerIndex; // Index du joueur (0 pour joueur 1, 1 pour joueur 2, etc.)
    private PlayerInput playerInput;
    private Image reloadIndicator; // Référence à l'indicateur de ce joueur
    private ReloadIndicatorController indicatorController; // Nouveau contrôleur pour l'animation

    public UnityEvent reloading;
    
    private void Awake()
    {
        // Important: obtenez le BonusManager du même GameObject (pas un singleton)
        bonusManager = GetComponent<BonusManager>();
        if (bonusManager == null)
        {
            bonusManager = gameObject.AddComponent<BonusManager>();
        }
        
        // Récupérer l'index du joueur
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerIndex = playerInput.playerIndex;
            Debug.Log($"BallShooter initialisé pour le joueur {playerIndex + 1}");
        }
        
        // Trouver l'indicateur de rechargement correspondant au joueur
        FindReloadIndicator();
        
        // Cacher l'indicateur de rechargement au démarrage
        HideReloadIndicator();
    }
    
    private void FindReloadIndicator()
    {
        // Trouver tous les indicateurs de rechargement dans la scène
        GameObject[] indicators = GameObject.FindGameObjectsWithTag(reloadIndicatorTag);
        
        foreach (GameObject indicator in indicators)
        {
            // Vérifier si cet indicateur correspond à ce joueur
            ReloadIndicatorController controller = indicator.GetComponent<ReloadIndicatorController>();
            if (controller != null && controller.PlayerIndex == playerIndex)
            {
                reloadIndicator = indicator.GetComponent<Image>();
                indicatorController = controller;
                break;
            }
        }
        
        if (reloadIndicator == null)
        {
            Debug.LogWarning($"Aucun indicateur de rechargement trouvé pour le joueur {playerIndex}");
        }
    }
    
    private void HideReloadIndicator()
    {
        if (reloadIndicator != null)
        {
            reloadIndicator.gameObject.SetActive(false);
        }
    }
    
    private void ShowReloadIndicator()
    {
        if (reloadIndicator != null)
        {
            reloading.Invoke();
            reloadIndicator.gameObject.SetActive(true);
            reloadIndicator.fillAmount = 0f; // Réinitialiser à 0
        }
    }
    
    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Si on est en train de recharger, ne rien faire
            if (isReloading)
                return;
                
            // Si l'arme n'est pas chargée, lancer le rechargement
            if (!isLoaded)
            {
                StartReload();
                return;
            }
            
            // Instantiate ball at spawn point
            GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
            
            // Appliquer le bonus actuel
            BallBehaviour ballBehaviour = ball.GetComponent<BallBehaviour>();
            if (ballBehaviour != null)
            {
                ballBehaviour.SetBonus(bonusManager.GetCurrentBonus());
            }
            
            // Get shooting direction from camera
            Vector3 shootDirection = playerCamera.transform.forward;
            
            // Get rigidbody and apply force
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
                
                // Orient the quad on the ball to face the direction of travel
                Transform quadTransform = ball.transform.Find("DirectionQuad");
                if (quadTransform != null)
                {
                    OrientQuad(quadTransform, shootDirection);
                }
            }
            
            // Après avoir tiré, l'arme n'est plus chargée
            isLoaded = false;
            
            // Lancer le rechargement automatique
            StartReload();
        }
    }
    
    private void StartReload()
    {
        if (!isReloading)
        {
            isReloading = true;
            
            // Afficher l'UI de rechargement
            ShowReloadIndicator();
            
            // Lancer la coroutine de rechargement
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }
    }
    
    private IEnumerator ReloadCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / reloadTime;
            
            // Mettre à jour l'indicateur de progression
            if (reloadIndicator != null)
            {
                reloadIndicator.fillAmount = progress;
                
                // Utiliser le contrôleur d'animation pour l'animation
                if (indicatorController != null)
                {
                    indicatorController.UpdateAnimation(progress);
                }
            }
            
            yield return null;
        }
        
        // Fin du rechargement
        isLoaded = true;
        isReloading = false;
        
        // Cacher l'UI de rechargement
        HideReloadIndicator();
    }
    
    // Méthode pour annuler le rechargement si nécessaire
    public void CancelReload()
    {
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            
            // Cacher l'UI de rechargement
            HideReloadIndicator();
        }
    }
    
    // Méthode pour recharger immédiatement (peut être utilisée pour les bonus)
    public void InstantReload()
    {
        CancelReload();
        isLoaded = true;
    }
    
    private void OrientQuad(Transform quadTransform, Vector3 direction)
    {
        Quaternion originalRotation = quadTransform.rotation;
        quadTransform.rotation = Quaternion.LookRotation(direction);
        
        // Reset X and Z rotation, keep only Y rotation
        Vector3 eulerAngles = quadTransform.eulerAngles;
        quadTransform.eulerAngles = new Vector3(originalRotation.eulerAngles.x, 
            eulerAngles.y, 
            originalRotation.eulerAngles.z);
    }
    
    public float GetReloadTime()
    {
        return reloadTime;
    }
}