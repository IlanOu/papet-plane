using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 5f;
    [SerializeField] private bool inverseControls = true;
    [SerializeField] private int playerInversedIndex = 1;

    private CharacterController controller;
    private Vector2 movementInput;
    private Camera mainCamera;
    private PlayerInput playerInput;
    private PlayerController playerController;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        playerController = GetComponent<PlayerController>();
    }
    
    // Méthode appelée par l'Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        // Bloquer l'input pendant le reload
        if (playerController.ballShooter.IsReloading())
        {
            movementInput = Vector2.zero;
            return;
        }
    
        movementInput = context.ReadValue<Vector2>();
    
        // Utiliser la propriété playerIndex du PlayerController pour vérifier l'inversion
        if (playerController.playerIndex == playerInversedIndex && inverseControls)
        {
            movementInput = new Vector2(-movementInput.x, -movementInput.y);
        }
    }

    
    private void FixedUpdate()
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        if (playerController.playerReloadController.isCurrentlyReloading) 
            return;
        
        if (movementInput.magnitude < 0.1f)
            return;
            
        // Ici, on reste en world space pour coller à votre comportement d'origine.
        // Vector3 moveVector = new Vector3(movementInput.x, 0, movementInput.y);
        Vector3 moveVector = transform.TransformDirection(new Vector3(movementInput.x, 0, movementInput.y));
        if (controller != null)
        {
            controller.Move(moveVector * moveSpeed * Time.fixedDeltaTime);
        }
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
    
    private void Update()
    {
        HandleFaceCamera();
    }
    
    private void HandleFaceCamera()
    {
        if (mainCamera != null && playerController.faceCamera)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            if (playerInput.playerIndex == playerInversedIndex)
            {
                Vector3 currentRot = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(currentRot.x, 180, currentRot.z);
            }
        }
    }
    
    public Vector2 GetMovementInput() => movementInput;
}
