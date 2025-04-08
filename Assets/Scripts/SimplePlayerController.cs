using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerController : MonoBehaviour
{
    // Références
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private MeshRenderer meshRenderer;
    
    // Paramètres
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Color[] playerColors = { Color.red, Color.blue, Color.green, Color.yellow };
    
    // Variables privées
    private Vector2 moveInput;
    private int playerIndex;
    private PlayerInput playerInput;
    
    // Animation states
    private string currentAnimState;
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
        // Obtenir les références automatiquement si non assignées
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (meshRenderer == null) meshRenderer = GetComponentInChildren<MeshRenderer>();
        
        playerInput = GetComponent<PlayerInput>();
        playerIndex = playerInput.playerIndex;
        
        // Définir la couleur du joueur
        if (meshRenderer != null && playerIndex < playerColors.Length)
        {
            meshRenderer.material.color = playerColors[playerIndex];
        }
        
        Debug.Log($"Joueur {playerIndex+1}: Contrôleur initialisé");
    }
    
    // Cette fonction est appelée par le système d'input
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"Joueur {playerIndex+1}: Input = {moveInput}");
    }
    
    void Update()
    {
        // Gérer les animations
        HandleAnimations();
    }
    
    void FixedUpdate()
    {
        // Déplacer le personnage
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
        characterController.Move(movement * moveSpeed * Time.fixedDeltaTime);
    }
    
    void HandleAnimations()
    {
        bool isMoving = moveInput.magnitude > 0.1f;
        
        if (isMoving)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            
            if (angle >= 22.5f && angle < 67.5f) 
                ChangeAnimState(WALK_RIGHT);
            else if (angle >= 67.5f && angle < 112.5f) 
                ChangeAnimState(WALK_UP);
            else if (angle >= 112.5f && angle < 157.5f) 
                ChangeAnimState(WALK_LEFT);
            else if (angle >= 157.5f && angle < 202.5f) 
                ChangeAnimState(WALK_LEFT);
            else if (angle >= 202.5f && angle < 247.5f) 
                ChangeAnimState(WALK_LEFT);
            else if (angle >= 247.5f && angle < 292.5f) 
                ChangeAnimState(WALK_DOWN);
            else if (angle >= 292.5f && angle < 337.5f) 
                ChangeAnimState(WALK_RIGHT);
            else 
                ChangeAnimState(WALK_RIGHT);
        }
        else
        {
            if (currentAnimState == WALK_UP)
                ChangeAnimState(IDLE_UP);
            else if (currentAnimState == WALK_DOWN)
                ChangeAnimState(IDLE_DOWN);
            else if (currentAnimState == WALK_LEFT)
                ChangeAnimState(IDLE_LEFT);
            else if (currentAnimState == WALK_RIGHT)
                ChangeAnimState(IDLE_RIGHT);
            else
                ChangeAnimState(IDLE_DOWN);
        }
    }
    
    void ChangeAnimState(string newState)
    {
        if (animator == null) return;
        if (currentAnimState == newState) return;
        
        animator.Play(newState);
        currentAnimState = newState;
    }
}