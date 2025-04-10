using Projectile;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Composants")]
        public CharacterController controller;
        public Animator animator;
        public Transform spawnPoint;
        public Renderer spriteRenderer;
        public BallShooter ballShooter;
    
        public PlayerMovementController movementController;
        public PlayerAnimationController animationController;
        public PlayerReloadController playerReloadController;

        public PlayerInput playerInput;
    
        [Header("Paramètres")]
        public bool faceCamera = true;
        [HideInInspector] public int playerIndex;
    
        private void Awake()
        {
            // Récupération des composants essentiels
            if (controller == null) 
                controller = GetComponent<CharacterController>();
            if (animator == null) 
                animator = GetComponentInChildren<Animator>();
            if (spriteRenderer == null) 
                spriteRenderer = GetComponentInChildren<Renderer>();
        
            // Récupérer le PlayerInput
            if (playerInput == null)
                playerInput = GetComponent<PlayerInput>();
        
            // Récupérer les autres modules sur le même GameObject
            if (movementController == null)
                movementController = GetComponent<PlayerMovementController>();
            if (animationController == null)
                animationController = GetComponent<PlayerAnimationController>();
            if (playerReloadController == null)
                playerReloadController = GetComponent<PlayerReloadController>();
        
            // Récupérer le BallShooter
            if (ballShooter == null)
                ballShooter = GetComponent<BallShooter>();
        }

        private void Start()
        {
            ballShooter.reloading.AddListener(() => playerReloadController.OnReloading());
        
            if (spawnPoint == null)
            {
                GameObject sp = new GameObject("SpawnPoint");
                spawnPoint = sp.transform;
                spawnPoint.SetParent(transform);
                spawnPoint.localPosition = new Vector3(0, 0, 0.5f);
            }
        
            if (playerInput != null)
            {
                InitPlayer(playerInput.playerIndex);
                Debug.Log($"Joueur {playerIndex + 1} initialisé");
            }
        }

        public void InitPlayer(int index)
        {
            playerIndex = index;
            gameObject.name = $"Player_{index + 1}";
            // Par exemple, pour inverser la rotation pour un joueur précis
            if (index == 1)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }
}