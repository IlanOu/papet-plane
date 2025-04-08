using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementAnimation3D : MonoBehaviour
{
    [Header("Composants")]
    public Animator animator;
    public CharacterController controller; // Ou Rigidbody si vous préférez

    [Header("Paramètres de mouvement")]
    public float moveSpeed = 5f;
    public bool faceCamera = true; // Si le quad doit toujours faire face à la caméra
    
    // Variables privées
    private Vector2 movementInput;
    private Vector3 movement;
    private string currentAnimationState;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private Camera mainCamera;
    
    // Constantes pour les noms des animations
    private const string ANIM_IDLE_DOWN = "Idle_Down";
    private const string ANIM_IDLE_UP = "Idle_Up";
    private const string ANIM_IDLE_LEFT = "Idle_Left";
    private const string ANIM_IDLE_RIGHT = "Idle_Right";
    private const string ANIM_WALK_DOWN = "Walk_Down";
    private const string ANIM_WALK_UP = "Walk_Up";
    private const string ANIM_WALK_LEFT = "Walk_Left";
    private const string ANIM_WALK_RIGHT = "Walk_Right";
    
    private void Awake()
    {
        // Récupérer le PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }
        
        // Récupérer l'action de mouvement
        moveAction = playerInput.actions.FindAction("Move");
        
        // Récupérer la caméra principale
        mainCamera = Camera.main;
    }
    
    private void OnEnable()
    {
        moveAction.Enable();
    }
    
    private void OnDisable()
    {
        moveAction.Disable();
    }
    
    void Update()
    {
        // Récupérer l'input de mouvement
        movementInput = moveAction.ReadValue<Vector2>();
        
        // Convertir le mouvement 2D en 3D (en supposant que y -> z pour un mouvement au sol)
        movement = new Vector3(movementInput.x, 0, movementInput.y);
        
        // Si on utilise une caméra non-orthographique, transformer le mouvement en fonction de la caméra
        if (mainCamera != null && !mainCamera.orthographic)
        {
            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;
            
            // Supprimer la composante y pour rester sur le plan du sol
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // Convertir le mouvement en fonction de l'orientation de la caméra
            movement = right * movementInput.x + forward * movementInput.y;
        }
        
        // Déterminer si le personnage est en mouvement
        bool isMoving = movementInput.magnitude > 0.1f;
        
        // Orienter le quad vers la caméra si nécessaire
        if (faceCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
        
        // Choisir l'animation appropriée
        if (isMoving)
        {
            // Convertir les 8 directions possibles en 4 animations
            float angle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
            
            // Normaliser l'angle entre 0 et 360
            if (angle < 0) angle += 360f;
            
            // Choisir l'animation en fonction de l'angle
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
        else
        {
            // Animation d'idle basée sur la dernière direction
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
    
    void FixedUpdate()
    {
        // Déplacer le personnage avec le CharacterController
        if (controller != null)
        {
            controller.Move(movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
    
    void ChangeAnimationState(string newState)
    {
        // Éviter de rejouer la même animation
        if (currentAnimationState == newState) return;
        
        // Jouer la nouvelle animation
        animator.Play(newState);
        
        // Mettre à jour l'état d'animation actuel
        currentAnimationState = newState;
    }
}