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
    private bool isReloading = false; // Indique si le processus de recharge (charge du tir) est en cours
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
        // L'arme démarre en "charged" et aucun tir n'a été effectué
        isCharged = true;
        shotsSinceLastReload = 0;

        // Récupération (ou ajout) du BonusManager
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
        // Recherche de tous les indicateurs dans la scène portant le tag spécifié
        GameObject[] indicators = GameObject.FindGameObjectsWithTag(reloadIndicatorTag);
        
        foreach (GameObject indicator in indicators)
        {
            // Vérifier si cet indicateur correspond au joueur courant
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
            // On invoque l'événement pour signaler le début du processus de recharge (charge du tir)
            reloading.Invoke();
            reloadIndicator.gameObject.SetActive(true);
            reloadIndicator.fillAmount = 0f; // Réinitialise l'indicateur
        }
    }
    
    // Méthode appelée par l'Input pour tirer
    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Si une recharge (animation de charge) est en cours, on ignore l'action
            if (isReloading)
                return;
            
            // Si le chargeur est vide, le joueur ne peut plus tirer
            if (currentAmmo <= 0)
            {
                Debug.Log("Chargeur vide, impossible de tirer !");
                return;
            }
            
            // Si le tir n'est pas chargé, il faut lancer la recharge (animation de charge)
            if (!isCharged)
            {
                Debug.Log("Le tir n'est pas chargé, recharge nécessaire.");
                StartReload();
                return;
            }
            
            // Le tir est chargé : procéder au tir
            FireBall();
            
            // Incrémenter le compteur et décrémenter le chargeur
            shotsSinceLastReload++;
            currentAmmo--;
            Debug.Log($"Tir effectué ! Munitions restantes : {currentAmmo}");
            
            // Si le nombre de tirs consécutifs atteint le seuil, l'arme perd son état chargé
            if (shotsSinceLastReload >= shotsBeforeReload)
            {
                isCharged = false;
                Debug.Log("Nombre maximum de tirs atteint, recharge nécessaire pour préparer le tir suivant.");
            }
        }
    }
    
    // Instanciation et lancement de la balle
    private void FireBall()
    {
        GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
            
        // Appliquer le bonus actuel
        BallBehaviour ballBehaviour = ball.GetComponent<BallBehaviour>();
        if (ballBehaviour != null)
        {
            ballBehaviour.SetBonus(bonusManager.GetCurrentBonus());
        }
            
        // Déterminer la direction du tir d'après la caméra
        Vector3 shootDirection = playerCamera.transform.forward;
            
        // Appliquer la force au Rigidbody
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
            
            // Orienter le quad pour l'effet visuel
            Transform quadTransform = ball.transform.Find("DirectionQuad");
            if (quadTransform != null)
            {
                OrientQuad(quadTransform, shootDirection);
            }
        }
    }
    
    // Démarre la procédure de recharge (charge du tir)
    private void StartReload()
    {
        // Ne pas lancer la recharge si le chargeur est vide
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
            
            // Mise à jour de l'indicateur visuel
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
        
        // Fin de l'animation de charge : le tir est désormais prêt
        isCharged = true;
        isReloading = false;
        // Réinitialiser le compteur pour permettre une suite de tirs (tant que le chargeur n'est pas vidé)
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
        // Si le chargeur n'est pas vide, on met à jour l'état chargé
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
        // Conserver certaines rotations initiales pour X et Z
        Vector3 eulerAngles = quadTransform.eulerAngles;
        quadTransform.eulerAngles = new Vector3(originalRotation.eulerAngles.x, eulerAngles.y, originalRotation.eulerAngles.z);
    }
    
    public float GetReloadTime()
    {
        return reloadTime;
    }
    
    // Retourne true si le tir est préparé (chargé)
    public bool IsLoaded()
    {
        return isCharged;
    }
    
    public void FillAmmo(int amount)
    {
        currentAmmo = amount;
        isCharged = false;
    }
}
