using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    
    // Variables pour l'animation
    private Vector2 moveInput;
    private string currentAnimState;
    private PlayerInput playerInput;
    
    // Constantes pour les noms des animations
    private const string IDLE_DOWN = "Idle_Down";
    private const string IDLE_UP = "Idle_Up";
    private const string IDLE_LEFT = "Idle_Left";
    private const string IDLE_RIGHT = "Idle_Right";
    private const string WALK_DOWN = "Walk_Down";
    private const string WALK_UP = "Walk_Up";
    private const string WALK_LEFT = "Walk_Left";
    private const string WALK_RIGHT = "Walk_Right";
    
    void Awake()
    {
        // Auto-référencement si non assigné
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        playerInput = GetComponent<PlayerInput>();
        
        // Message de débogage
        Debug.Log($"SimpleMovement initialisé pour joueur {playerInput?.playerIndex}");
    }
    
    // IMPORTANT: Cette méthode doit être référencée dans l'inspecteur du composant PlayerInput
    public void OnMove(InputAction.CallbackContext context)
    {
        // Récupérer la valeur d'input
        moveInput = context.ReadValue<Vector2>();
        
        // Message de débogage
        Debug.Log($"Input de mouvement: {moveInput} - Joueur: {playerInput?.playerIndex}");
    }
    
    void Update()
    {
        // Vérifier les erreurs
        if (controller == null)
        {
            Debug.LogError("Pas de CharacterController assigné!");
            return;
        }
        
        // Déplacement simple
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
        
        // Appliquer le mouvement
        if (movement.magnitude > 0.1f)
        {
            controller.Move(movement * moveSpeed * Time.deltaTime);
            Debug.Log($"Déplacement: {movement * moveSpeed * Time.deltaTime}");
            
            // Gérer les animations
            UpdateAnimation(true);
        }
        else
        {
            // Animation idle
            UpdateAnimation(false);
        }
    }
    
    void UpdateAnimation(bool isMoving)
    {
        if (animator == null) return;
        
        if (isMoving)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            
            if (angle >= 22.5f && angle < 67.5f) 
                ChangeAnimation(WALK_RIGHT);
            else if (angle >= 67.5f && angle < 112.5f) 
                ChangeAnimation(WALK_UP);
            else if (angle >= 112.5f && angle < 157.5f) 
                ChangeAnimation(WALK_LEFT);
            else if (angle >= 157.5f && angle < 202.5f) 
                ChangeAnimation(WALK_LEFT);
            else if (angle >= 202.5f && angle < 247.5f) 
                ChangeAnimation(WALK_LEFT);
            else if (angle >= 247.5f && angle < 292.5f) 
                ChangeAnimation(WALK_DOWN);
            else if (angle >= 292.5f && angle < 337.5f) 
                ChangeAnimation(WALK_RIGHT);
            else 
                ChangeAnimation(WALK_RIGHT);
        }
        else
        {
            // Idle animations
            if (currentAnimState == WALK_UP)
                ChangeAnimation(IDLE_UP);
            else if (currentAnimState == WALK_DOWN)
                ChangeAnimation(IDLE_DOWN);
            else if (currentAnimState == WALK_LEFT)
                ChangeAnimation(IDLE_LEFT);
            else if (currentAnimState == WALK_RIGHT)
                ChangeAnimation(IDLE_RIGHT);
            else
                ChangeAnimation(IDLE_DOWN);
        }
    }
    
    void ChangeAnimation(string newState)
    {
        if (currentAnimState == newState) return;
        
        animator.Play(newState);
        currentAnimState = newState;
    }
}