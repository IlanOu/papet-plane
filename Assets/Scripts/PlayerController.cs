using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Composants")]
    public Animator animator;
    public CharacterController controller;
    public Transform spawnPoint;
    
    [Header("Paramètres visuels")]
    public Renderer spriteRenderer;
    public bool faceCamera = true;
    
    [Header("Paramètres de mouvement")]
    public float moveSpeed = 5f;
    
    [SerializeField] private int playerInversedIndex = 1;
    [SerializeField] private bool inverseControls = true;
    
    private float speedModifier = 1.0f;
    private float baseSpeed;
    
    // Variables privées
    private Vector2 movementInput;
    private int playerIndex;
    private Camera mainCamera;
    private PlayerInput playerInput;
    private Vector3 lastMoveDirection;
    
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
    
    private void Awake()
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
            Debug.Log($"Joueur {playerIndex+1} initialisé");
        }
        
        // Créer le spawnPoint s'il n'existe pas
        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnPoint = spawnObj.transform;
            spawnPoint.SetParent(transform);
            spawnPoint.localPosition = new Vector3(0, 0, 0.5f); // Légèrement devant le joueur
        }
    }
    
    private void Start()
    {
        baseSpeed = moveSpeed;
    }
    
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
    
    public void InitPlayer(int index)
    {
        playerIndex = index;
        gameObject.name = $"Player_{index + 1}";
        
        if (index == playerInversedIndex)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
    
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
        Vector3 moveVector = new Vector3(movementInput.x, 0, movementInput.y);
        
        // Déplacement et animation uniquement si on bouge
        if (moveVector.magnitude > 0.1f)
        {
            // transform.Translate(moveVector * moveSpeed * Time.deltaTime);
            lastMoveDirection = moveVector.normalized;
            
            // Mettre à jour la rotation du spawnPoint pour qu'il pointe dans la direction du mouvement
            spawnPoint.forward = new Vector3(lastMoveDirection.x, 0, lastMoveDirection.z);
            
            // Logique d'animation
            float angle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            
            // Pour le joueur inversé, on inverse les animations
            if (playerIndex == playerInversedIndex)
            {
                // On inverse les directions pour les animations
                if (angle >= 22.5f && angle < 67.5f) 
                    ChangeAnimationState(ANIM_WALK_LEFT);  // Droite devient gauche
                else if (angle >= 67.5f && angle < 112.5f) 
                    ChangeAnimationState(ANIM_WALK_DOWN);  // Haut devient bas
                else if (angle >= 112.5f && angle < 157.5f) 
                    ChangeAnimationState(ANIM_WALK_RIGHT); // Gauche devient droite
                else if (angle >= 157.5f && angle < 202.5f) 
                    ChangeAnimationState(ANIM_WALK_RIGHT); // Gauche devient droite
                else if (angle >= 202.5f && angle < 247.5f) 
                    ChangeAnimationState(ANIM_WALK_RIGHT); // Gauche devient droite
                else if (angle >= 247.5f && angle < 292.5f) 
                    ChangeAnimationState(ANIM_WALK_UP);    // Bas devient haut
                else if (angle >= 292.5f && angle < 337.5f) 
                    ChangeAnimationState(ANIM_WALK_LEFT);  // Droite devient gauche
                else 
                    ChangeAnimationState(ANIM_WALK_LEFT);  // Droite devient gauche
            }
            else
            {
                // Animation normale pour le joueur non-inversé
                if (angle >= 22.5f && angle < 67.5f) 
                    ChangeAnimationState(ANIM_WALK_RIGHT);
                else if (angle >= 67.5f && angle < 112.5f) 
                    ChangeAnimationState(ANIM_WALK_UP);
                else if (angle >= 112.5f && angle < 157.5f) 
                    ChangeAnimationState(ANIM_WALK_LEFT);
                else if (angle >= 157.5f && angle < 202.5f) 
                    ChangeAnimationState(ANIM_WALK_LEFT);
                else if (angle >= 202.5f && angle < 247.5f) 
                    ChangeAnimationState(ANIM_WALK_LEFT);
                else if (angle >= 247.5f && angle < 292.5f) 
                    ChangeAnimationState(ANIM_WALK_DOWN);
                else if (angle >= 292.5f && angle < 337.5f) 
                    ChangeAnimationState(ANIM_WALK_RIGHT);
                else 
                    ChangeAnimationState(ANIM_WALK_RIGHT);
            }
        }
        else
        {
            // Animation idle
            if (playerIndex == playerInversedIndex)
            {
                // Inversion des animations idle pour le joueur inversé
                if (currentAnimationState == ANIM_WALK_UP)
                    ChangeAnimationState(ANIM_IDLE_DOWN);
                else if (currentAnimationState == ANIM_WALK_DOWN)
                    ChangeAnimationState(ANIM_IDLE_UP);
                else if (currentAnimationState == ANIM_WALK_LEFT)
                    ChangeAnimationState(ANIM_IDLE_RIGHT);
                else if (currentAnimationState == ANIM_WALK_RIGHT)
                    ChangeAnimationState(ANIM_IDLE_LEFT);
                else if (currentAnimationState == null)
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
                else if (currentAnimationState == null)
                    ChangeAnimationState(ANIM_IDLE_DOWN);
            }
        }
        
        // Faire face à la caméra
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
    
    private void FixedUpdate()
    {
        Vector3 moveVector = new Vector3(movementInput.x, 0, movementInput.y);
    
        if (moveVector.magnitude > 0.1f)
        {
            // Utiliser CharacterController si disponible
            if (controller != null)
            {
                controller.Move(moveVector * moveSpeed * Time.fixedDeltaTime);
            }
            // Sinon, ajouter un Rigidbody et l'utiliser
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
            
                // Utiliser MovePosition au lieu de Translate
                rb.MovePosition(rb.position + moveVector * moveSpeed * Time.fixedDeltaTime);
            }
        }
    }
    
    private string currentAnimationState;
    
    void ChangeAnimationState(string newState)
    {
        if (animator == null) return;
        if (currentAnimationState == newState) return;
        
        animator.Play(newState);
        currentAnimationState = newState;
    }
}