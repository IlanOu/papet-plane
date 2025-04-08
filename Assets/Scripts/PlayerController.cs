using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Composants")]
    public Animator animator;
    public CharacterController controller;
    
    [Header("Paramètres visuels")]
    public Color[] playerColors = { Color.red, Color.blue, Color.green, Color.yellow };
    public Renderer spriteRenderer;
    public bool faceCamera = true;
    
    [Header("Paramètres de mouvement")]
    public float moveSpeed = 5f;
    
    // Variables privées
    private Vector2 movementInput;
    private Vector3 movement;
    private string currentAnimationState;
    private int playerIndex;
    private Camera mainCamera;
    private PlayerInput playerInput;
    
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
    }
    
    public void InitPlayer(int index)
    {
        playerIndex = index;
        
        // Appliquer une couleur différente
        if (spriteRenderer != null && playerColors.Length > index)
        {
            spriteRenderer.material.color = playerColors[index];
        }
        
        gameObject.name = $"Player_{index + 1}";
    }
    
    // MÉTHODE CLÉ - Assurez-vous qu'elle est connectée dans l'Inspector
    public void OnMove(InputAction.CallbackContext context)
    {
        // Lire la valeur de l'input et l'afficher pour le débogage
        movementInput = context.ReadValue<Vector2>();
        Debug.Log($"Movement Input: {movementInput}");
    }
    
    private void Update()
    {
        // Simplification - Utiliser transform.Translate comme dans le script fonctionnel
        // tout en gardant la logique de conversion 2D/3D
        Vector3 moveVector = new Vector3(movementInput.x, 0, movementInput.y);
        
        // Déplacement et animation uniquement si on bouge
        if (moveVector.magnitude > 0.1f)
        {
            // CHANGEMENT CLÉ: Utiliser Translate au lieu de controller.Move
            // pour tester si le mouvement fonctionne
            transform.Translate(moveVector * moveSpeed * Time.deltaTime);
            
            // Logique d'animation existante
            float angle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;
            
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
            // Animation idle
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
        
        // Faire face à la caméra
        if (faceCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
    
    void ChangeAnimationState(string newState)
    {
        if (animator == null) return;
        if (currentAnimationState == newState) return;
        
        animator.Play(newState);
        currentAnimationState = newState;
    }
}