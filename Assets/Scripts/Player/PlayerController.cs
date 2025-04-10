using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Composants")]
    public CharacterController controller;
    public Animator animator;
    public Transform spawnPoint;
    public BallShooter ballShooter;
    public Renderer spriteRenderer;
    public bool faceCamera = true;

    // Références vers les autres contrôleurs
    [HideInInspector] public PlayerMovementController movementController;
    [HideInInspector] public PlayerAnimationController animationController;
    [HideInInspector] public PlayerReloadController playerReloadController;

    private PlayerInput playerInput;
    [HideInInspector] public int playerIndex;
    
    private void Awake()
    {
        // Récupération des composants essentiels
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<Renderer>();
        if (spawnPoint == null)
        {
            GameObject sp = new GameObject("SpawnPoint");
            spawnPoint = sp.transform;
            spawnPoint.SetParent(transform);
            spawnPoint.localPosition = new Vector3(0, 0, 0.5f);
        }
        
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            InitPlayer(playerInput.playerIndex);
            Debug.Log($"Joueur {playerIndex + 1} initialisé");
        }
        
        // Récupérer les autres modules sur le même GameObject
        movementController = GetComponent<PlayerMovementController>();
        animationController = GetComponent<PlayerAnimationController>();
        playerReloadController = GetComponent<PlayerReloadController>();
    }

    private void Start()
    {
        ballShooter.reloading.AddListener(() => playerReloadController.OnReloading());
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