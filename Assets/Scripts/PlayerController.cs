using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Composants")]
    public Animator animator;
    public CharacterController controller;
    public Transform spawnPoint;
    public BallShooter ballShooter;
    
    [Header("Paramètres visuels")]
    public Renderer spriteRenderer;
    public bool faceCamera = true;
    
    [Header("Paramètres de mouvement")]
    public float moveSpeed = 5f;
    [SerializeField] private int playerInversedIndex = 1;
    [SerializeField] private bool inverseControls = true;
    [SerializeField] private float reloadSpeedModifier = 0.8f; // Multiplicateur de vitesse pendant le rechargement
    
    [Header("Animation de rechargement")]
    [SerializeField] private string reloadAnimationTrigger = "Reload";
    
    // Constantes pour les noms des animations
    private const string ANIM_IDLE_DOWN = "Idle_Down";
    private const string ANIM_IDLE_UP = "Idle_Up";
    private const string ANIM_IDLE_LEFT = "Idle_Left";
    private const string ANIM_IDLE_RIGHT = "Idle_Right";
    private const string ANIM_WALK_DOWN = "Walk_Down";
    private const string ANIM_WALK_UP = "Walk_Up";
    private const string ANIM_WALK_LEFT = "Walk_Left";
    private const string ANIM_WALK_RIGHT = "Walk_Right";
    private const string ANIM_WALK_PLANE_RIGHT = "Walk_Right_Plane";
    private const string ANIM_WALK_PLANE_LEFT = "Walk_Left_Plane";
    private const string ANIM_WALK_PLANE_UP = "Walk_Up_Plane";
    private const string ANIM_WALK_PLANE_DOWN = "Walk_Down_Plane";
    private const string ANIM_IDLE_PLANE_RIGHT = "Idle_Right_Plane";
    private const string ANIM_IDLE_PLANE_LEFT = "Idle_Left_Plane";
    private const string ANIM_IDLE_PLANE_UP = "Idle_Up_Plane";
    private const string ANIM_IDLE_PLANE_DOWN = "Idle_Down_Plane";
    
    // Variables privées
    private Vector2 movementInput;
    private int playerIndex;
    private Camera mainCamera;
    private PlayerInput playerInput;
    private Vector3 lastMoveDirection;
    private string currentAnimationState;
    private float baseSpeed;
    private float speedModifier = 1.0f;
    private bool isCurrentlyReloading = false;
    private Coroutine reloadCoroutine;
    
    #region Initialisation
    
    private void Awake()
    {
        InitializeComponents();
        SetupSpawnPoint();
    }
    
    private void Start()
    {
        baseSpeed = moveSpeed;
        
        if (ballShooter != null)
        {
            ballShooter.reloading.AddListener(OnReloading);
        }
        else
        {
            Debug.LogWarning($"BallShooter non assigné sur le joueur {playerIndex + 1}");
        }
    }
    
    private void InitializeComponents()
    {
        // Récupérer les références nécessaires
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<Renderer>();
        
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        
        // Initialisation automatique
        if (playerInput != null)
        {
            InitPlayer(playerInput.playerIndex);
            Debug.Log($"Joueur {playerIndex + 1} initialisé");
        }
    }
    
    private void SetupSpawnPoint()
    {
        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnPoint = spawnObj.transform;
            spawnPoint.SetParent(transform);
            spawnPoint.localPosition = new Vector3(0, 0, 0.5f); // Légèrement devant le joueur
        }
    }
    
    public void InitPlayer(int index)
    {
        playerIndex = index;
        gameObject.name = $"Player_{index + 1}";
        
        if (index == playerInversedIndex)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
    
    #endregion
    
    #region Input & Movement
    
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        
        // Inverser les contrôles pour le joueur spécifié
        if (playerIndex == playerInversedIndex && inverseControls)
        {
            movementInput = new Vector2(-movementInput.x, -movementInput.y);
        }
    }
    
    private void Update()
    {
        HandleAnimation();
        HandleFaceCamera();
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        Vector3 moveVector = new Vector3(movementInput.x, 0, movementInput.y);
        
        if (moveVector.magnitude > 0.1f)
        {
            // Utiliser CharacterController si disponible
            if (controller != null)
            {
                controller.Move(moveVector * moveSpeed * Time.fixedDeltaTime);
            }
            // Sinon, utiliser Rigidbody
            else
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = gameObject.AddComponent<Rigidbody>();
                    rb.freezeRotation = true;
                    rb.useGravity = true;
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
                
                rb.MovePosition(rb.position + moveVector * moveSpeed * Time.fixedDeltaTime);
            }
        }
    }
    
    private void HandleFaceCamera()
    {
        if (faceCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            
            // Maintenir la rotation Y à 180 degrés pour le joueur inversé
            if (playerIndex == playerInversedIndex)
            {
                Vector3 currentRotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(currentRotation.x, 180, currentRotation.z);
            }
        }
    }
    
    #endregion
    
    #region Animation
    
    private void HandleAnimation()
    {
        Vector3 moveVector = new Vector3(movementInput.x, 0, movementInput.y);
        
        if (moveVector.magnitude > 0.1f)
        {
            // Mettre à jour la direction du mouvement
            lastMoveDirection = moveVector.normalized;
            
            // Mettre à jour la rotation du spawnPoint pour qu'il pointe dans la direction du mouvement
            spawnPoint.forward = new Vector3(lastMoveDirection.x, 0, lastMoveDirection.z);
            
            // Déterminer l'animation en fonction de la direction
            UpdateMovementAnimation(movementInput);
        }
        else
        {
            // Animation idle basée sur la dernière animation de mouvement
            UpdateIdleAnimation();
        }
    }
    
    private void UpdateMovementAnimation(Vector2 input)
    {
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        
        bool isInversedPlayer = (playerIndex == playerInversedIndex);
        
        if (isInversedPlayer)
        {
            // Animations inversées
            if (angle >= 22.5f && angle < 67.5f) 
                ChangeAnimationState(ANIM_WALK_LEFT);  // Droite devient gauche
            else if (angle >= 67.5f && angle < 112.5f) 
                ChangeAnimationState(ANIM_WALK_DOWN);  // Haut devient bas
            else if (angle >= 112.5f && angle < 202.5f) 
                ChangeAnimationState(ANIM_WALK_RIGHT); // Gauche devient droite
            else if (angle >= 202.5f && angle < 247.5f) 
                ChangeAnimationState(ANIM_WALK_RIGHT); // Gauche devient droite
            else if (angle >= 247.5f && angle < 292.5f) 
                ChangeAnimationState(ANIM_WALK_UP);    // Bas devient haut
            else
                ChangeAnimationState(ANIM_WALK_LEFT);  // Droite devient gauche
        }
        else
        {
            // Animation normale
            if (angle >= 22.5f && angle < 67.5f) 
                ChangeAnimationState(ANIM_WALK_RIGHT);
            else if (angle >= 67.5f && angle < 112.5f) 
                ChangeAnimationState(ANIM_WALK_UP);
            else if (angle >= 112.5f && angle < 202.5f) 
                ChangeAnimationState(ANIM_WALK_LEFT);
            else if (angle >= 202.5f && angle < 247.5f) 
                ChangeAnimationState(ANIM_WALK_LEFT);
            else if (angle >= 247.5f && angle < 292.5f) 
                ChangeAnimationState(ANIM_WALK_DOWN);
            else
                ChangeAnimationState(ANIM_WALK_RIGHT);
        }
    }
    
    private void UpdateIdleAnimation()
    {
        bool isInversedPlayer = (playerIndex == playerInversedIndex);
        
        if (isInversedPlayer)
        {
            // Inversion des animations idle
            if (currentAnimationState == ANIM_WALK_UP)
                ChangeAnimationState(ANIM_IDLE_DOWN);
            else if (currentAnimationState == ANIM_WALK_DOWN)
                ChangeAnimationState(ANIM_IDLE_UP);
            else if (currentAnimationState == ANIM_WALK_LEFT)
                ChangeAnimationState(ANIM_IDLE_RIGHT);
            else if (currentAnimationState == ANIM_WALK_RIGHT)
                ChangeAnimationState(ANIM_IDLE_LEFT);
            else if (currentAnimationState == null || !currentAnimationState.StartsWith("Idle"))
                ChangeAnimationState(ANIM_IDLE_UP); // Idle par défaut inversé
        }
        else
        {
            // Animation idle normale
            if (currentAnimationState == ANIM_WALK_UP)
                ChangeAnimationState(ANIM_IDLE_UP);
            else if (currentAnimationState == ANIM_WALK_DOWN)
                ChangeAnimationState(ANIM_IDLE_DOWN);
            else if (currentAnimationState == ANIM_WALK_LEFT)
                ChangeAnimationState(ANIM_IDLE_LEFT);
            else if (currentAnimationState == ANIM_WALK_RIGHT)
                ChangeAnimationState(ANIM_IDLE_RIGHT);
            else if (currentAnimationState == null || !currentAnimationState.StartsWith("Idle"))
                ChangeAnimationState(ANIM_IDLE_DOWN); // Idle par défaut
        }
    }
    
    private void ChangeAnimationState(string newState)
    {
        if (animator == null) return;
        if (currentAnimationState == newState) return;
        
        animator.Play(newState);
        currentAnimationState = newState;
    }
    
    #endregion
    
    #region Reloading
    
    private void OnReloading()
    {
        if (isCurrentlyReloading) return;
        
        isCurrentlyReloading = true;
        
        // Déclencher l'animation de rechargement
        if (animator != null)
        {
            animator.SetTrigger(reloadAnimationTrigger);
        }
        
        // Ralentir le joueur pendant le rechargement
        if (reloadSpeedModifier != 1.0f)
        {
            ApplySpeedModifier(reloadSpeedModifier);
        }
        
        // Annuler la coroutine existante si nécessaire
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }
        
        // Démarrer une nouvelle coroutine
        reloadCoroutine = StartCoroutine(ResetReloadingState(ballShooter.GetReloadTime()));
    }
    
    private IEnumerator ResetReloadingState(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        isCurrentlyReloading = false;
        
        // Restaurer la vitesse normale
        if (reloadSpeedModifier != 1.0f)
        {
            ResetSpeedModifier();
        }
        
        reloadCoroutine = null;
    }
    
    #endregion
    
    #region Utility Methods
    
    public void ApplySpeedModifier(float modifier)
    {
        speedModifier = modifier;
        moveSpeed = baseSpeed * speedModifier;
    }
    
    public void ResetSpeedModifier()
    {
        speedModifier = 1.0f;
        moveSpeed = baseSpeed;
    }
    
    // Méthode pour vérifier si le joueur est en rechargement
    public bool IsReloading()
    {
        return isCurrentlyReloading;
    }
    
    private void OnDisable()
    {
        // Nettoyer les événements lors de la désactivation
        if (ballShooter != null)
        {
            ballShooter.reloading.RemoveListener(OnReloading);
        }
        
        // Arrêter les coroutines en cours
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
    }
    
    #endregion
}