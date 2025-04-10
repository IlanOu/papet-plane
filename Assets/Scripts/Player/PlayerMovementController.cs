using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerMovementController : MonoBehaviour
    {
        [Header("Composants")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private PlayerController playerController;
    
        [Header("Mouvement")]
        public float moveSpeed = 5f;
        [SerializeField] private bool inverseControls = true;
        [SerializeField] private int playerInversedIndex = 1;

        private Vector2 movementInput;
        private Camera mainCamera;
    
        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<CharacterController>();
        
            mainCamera = Camera.main;
        
            if (playerController == null)
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
            
            // Créer le vecteur de mouvement de base
            Vector3 moveVector = new Vector3(movementInput.x, 0, movementInput.y);
            
            // Inverser le mouvement si nécessaire
            if (playerController.playerIndex == playerInversedIndex && inverseControls)
            {
                moveVector = new Vector3(-moveVector.x, 0, -moveVector.z);
            }
            
            // Appliquer le mouvement
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
                if (playerController.playerIndex == playerInversedIndex)
                {
                    Vector3 currentRot = transform.rotation.eulerAngles;
                    transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y + 180, currentRot.z);
                }
            }
        }
    
        public Vector2 GetMovementInput() 
        {
            // Retourner les inputs inversés si nécessaire
            if (playerController.playerIndex == playerInversedIndex && inverseControls)
            {
                return new Vector2(-movementInput.x, -movementInput.y);
            }
            return movementInput;
        }
    }
}