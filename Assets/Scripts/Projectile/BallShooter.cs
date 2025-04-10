using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BallShooter : MonoBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float shootForce = 10f;
    
    [Header("Camera")]
    [SerializeField] private GameObject playerCamera;
    
    [Header("Reload Settings")]
    [SerializeField] private float reloadTime = 3f; // Durée du rechargement (chargement du tir)
    
    [Header("Ammo Settings")]
    public int magazineCapacity = 5; // Nombre maximal de munitions dans le chargeur
    private int currentAmmo;                          // Munitions actuellement disponibles dans le chargeur
    
    // Nombre de tirs autorisés avant de devoir recharger l'arme (même si le chargeur n'est pas vide)
    [SerializeField] private int shotsBeforeReload = 1;
    // Compteur de tirs effectués depuis la dernière recharge (état "chargé")
    private int shotsSinceLastReload = 0;
    
    // État indiquant si le tir est préparé (après l'animation de recharge)
    private bool isCharged = false;
    
    [Header("UI References")]
    [SerializeField] private string reloadIndicatorTag = "ReloadIndicator"; // Tag utilisé pour trouver l'indicateur de rechargement
    
    private BonusManager bonusManager;
    private bool isReloading = false; // Indique si le processus de recharge est en cours
    private Coroutine reloadCoroutine;
    private int playerIndex; // Index du joueur (0 pour joueur 1, 1 pour joueur 2, etc.)
    private PlayerInput playerInput;
    private Image reloadIndicator; // Référence à l'indicateur pour ce joueur
    private ReloadIndicatorController indicatorController; // Contrôleur pour l'animation de l'indicateur

    public UnityEvent reloading;
    
    private void Awake()
    {
        // Au démarrage, on remplit le chargeur et on considère que l'arme est chargée
        FillAmmo(magazineCapacity);
        isCharged = true;
        shotsSinceLastReload = 0;

        bonusManager = GetComponent<BonusManager>();
        if (bonusManager == null)
        {
            bonusManager = gameObject.AddComponent<BonusManager>();
        }
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerIndex = playerInput.playerIndex;
            Debug.Log($"BallShooter initialisé pour le joueur {playerIndex + 1}");
        }
        FindReloadIndicator();
        HideReloadIndicator();
    }

    private void FindReloadIndicator()
    {
        GameObject[] indicators = GameObject.FindGameObjectsWithTag(reloadIndicatorTag);
        
        foreach (GameObject indicator in indicators)
        {
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
            reloadIndicator.fillAmount = 0f;
        }
    }
    
    // Méthode appelée par l'Input pour tirer
    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isReloading)
                return;
            
            if (currentAmmo <= 0)
            {
                Debug.Log("Chargeur vide, impossible de tirer !");
                return;
            }
            
            if (!isCharged)
            {
                Debug.Log("Le tir n'est pas chargé, recharge nécessaire.");
                StartReload();
                return;
            }
            
            FireBall();
            
            shotsSinceLastReload++;
            currentAmmo--;
            Debug.Log($"Tir effectué ! Munitions restantes : {currentAmmo}");
            
            if (shotsSinceLastReload >= shotsBeforeReload)
            {
                isCharged = false;
                Debug.Log("Nombre maximum de tirs atteint, recharge nécessaire pour préparer le tir suivant.");
            }
        }
    }
    
    private void FireBall()
    {
        GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
            
        BallBehaviour ballBehaviour = ball.GetComponent<BallBehaviour>();
        if (ballBehaviour != null)
        {
            ballBehaviour.SetBonus(bonusManager.GetCurrentBonus());
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
        if (currentAmmo <= 0)
        {
            Debug.Log("Impossible de recharger, chargeur vide.");
            return;
        }
        
        if (!isReloading)
        {
            isReloading = true;
            ShowReloadIndicator();
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
            
            if (reloadIndicator != null)
            {
                reloadIndicator.fillAmount = progress;
                if (indicatorController != null)
                {
                    indicatorController.UpdateAnimation(progress);
                }
            }
            
            yield return null;
        }
        
        // Fin du rechargement : le tir est prêt
        isCharged = true;
        isReloading = false;
        shotsSinceLastReload = 0;
        Debug.Log("Tir chargé, prêt à tirer.");
        
        HideReloadIndicator();
    }
    
    public void CancelReload()
    {
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            HideReloadIndicator();
        }
    }
    
    public void InstantReload()
    {
        CancelReload();
        if (currentAmmo > 0)
        {
            isCharged = true;
            shotsSinceLastReload = 0;
            Debug.Log("Recharge instantanée effectuée, tir chargé.");
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
    public bool IsLoaded() => isCharged;
    
    public bool IsReloading() => isReloading;
    
    public void FillAmmo(int amount)
    {
        currentAmmo = amount;
    }
}
