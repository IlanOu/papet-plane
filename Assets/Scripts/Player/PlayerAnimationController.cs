using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        public Animator animator;
        public Transform spawnPoint;
    
        // Animations standards
        private const string ANIM_IDLE_DOWN  = "Idle_Down";
        private const string ANIM_IDLE_UP    = "Idle_Up";
        private const string ANIM_IDLE_LEFT  = "Idle_Left";
        private const string ANIM_IDLE_RIGHT = "Idle_Right";
        private const string ANIM_WALK_DOWN  = "Walk_Down";
        private const string ANIM_WALK_UP    = "Walk_Up";
        private const string ANIM_WALK_LEFT  = "Walk_Left";
        private const string ANIM_WALK_RIGHT = "Walk_Right";
    
        // Animations "plane"
        private const string ANIM_IDLE_PLANE_DOWN  = "Idle_Down_Plane";
        private const string ANIM_IDLE_PLANE_UP    = "Idle_Up_Plane";
        private const string ANIM_IDLE_PLANE_LEFT  = "Idle_Left_Plane";
        private const string ANIM_IDLE_PLANE_RIGHT = "Idle_Right_Plane";
        private const string ANIM_WALK_PLANE_DOWN  = "Walk_Down_Plane";
        private const string ANIM_WALK_PLANE_UP    = "Walk_Up_Plane";
        private const string ANIM_WALK_PLANE_LEFT  = "Walk_Left_Plane";
        private const string ANIM_WALK_PLANE_RIGHT = "Walk_Right_Plane";
    
        public string reloadAnimation = "Reload";
        public int reloadAnimationFrameCount = 4;
    
        private string currentAnimationState;
        private Vector2 movementInput;
        private Vector3 lastMoveDirection;
        private string lastDirectionState = "down";
    
        [SerializeField] private PlayerController playerController;
    
        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }
    
        private void Update()
        {
            // Récupération de l'input via le mouvement
            PlayerMovementController pmc = GetComponent<PlayerMovementController>();
            if (pmc != null)
            {
                movementInput = pmc.GetMovementInput();
            }
            HandleAnimation();
        }
    
        private void HandleAnimation()
        {
            Vector3 moveVector = new Vector3(movementInput.x, 0, movementInput.y);
        
            if (playerController.playerReloadController.IsReloading())
            {
                ChangeAnimationState(reloadAnimation);
            }
            else if (moveVector.magnitude < 0.1f)
            {
                UpdateIdleAnimation();
            }
            else
            {
                lastMoveDirection = moveVector.normalized;
                if (spawnPoint != null)
                    spawnPoint.forward = new Vector3(lastMoveDirection.x, 0, lastMoveDirection.z);
                UpdateMovementAnimation(movementInput);
            }
        }
    
        private void UpdateMovementAnimation(Vector2 input)
        {
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
            if(angle < 0) angle += 360f;
            bool isInversedPlayer = (playerController.GetComponent<PlayerInput>().playerIndex == 1);
        
            if (isInversedPlayer)
            {
                if (angle >= 22.5f && angle < 67.5f)
                {
                    ChangeAnimationState(ANIM_WALK_LEFT);
                    lastDirectionState = "left";
                }
                else if (angle >= 67.5f && angle < 112.5f)
                {
                    ChangeAnimationState(ANIM_WALK_DOWN);
                    lastDirectionState = "down";
                }
                else if (angle >= 112.5f && angle < 202.5f)
                {
                    ChangeAnimationState(ANIM_WALK_RIGHT);
                    lastDirectionState = "right";
                }
                else if (angle >= 202.5f && angle < 247.5f)
                {
                    ChangeAnimationState(ANIM_WALK_RIGHT);
                    lastDirectionState = "right";
                }
                else if (angle >= 247.5f && angle < 292.5f)
                {
                    ChangeAnimationState(ANIM_WALK_UP);
                    lastDirectionState = "up";
                }
                else
                {
                    ChangeAnimationState(ANIM_WALK_LEFT);
                    lastDirectionState = "left";
                }
            }
            else
            {
                if (angle >= 22.5f && angle < 67.5f)
                {
                    ChangeAnimationState(ANIM_WALK_RIGHT);
                    lastDirectionState = "right";
                }
                else if (angle >= 67.5f && angle < 112.5f)
                {
                    ChangeAnimationState(ANIM_WALK_UP);
                    lastDirectionState = "up";
                }
                else if (angle >= 112.5f && angle < 202.5f)
                {
                    ChangeAnimationState(ANIM_WALK_LEFT);
                    lastDirectionState = "left";
                }
                else if (angle >= 202.5f && angle < 247.5f)
                {
                    ChangeAnimationState(ANIM_WALK_LEFT);
                    lastDirectionState = "left";
                }
                else if (angle >= 247.5f && angle < 292.5f)
                {
                    ChangeAnimationState(ANIM_WALK_DOWN);
                    lastDirectionState = "down";
                }
                else
                {
                    ChangeAnimationState(ANIM_WALK_RIGHT);
                    lastDirectionState = "right";
                }
            }
        }

        public void UpdateIdleAnimation()
        {
            bool isInversedPlayer = (playerController.GetComponent<PlayerInput>().playerIndex == 1);
            // Si l'arme est chargée, utiliser la version "plane"
            bool usePlaneIdle = (playerController.ballShooter != null && playerController.ballShooter.IsLoaded());
        
            if (isInversedPlayer)
            {
                if (lastDirectionState == "up")
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_DOWN : ANIM_IDLE_DOWN);
                else if (lastDirectionState == "down")
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_UP : ANIM_IDLE_UP);
                else if (lastDirectionState == "left")
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_RIGHT : ANIM_IDLE_RIGHT);
                else if (lastDirectionState == "right")
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_LEFT : ANIM_IDLE_LEFT);
                else
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_UP : ANIM_IDLE_UP);
            }
            else
            {
                if (lastDirectionState == "up")
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_UP : ANIM_IDLE_UP);
                else if (lastDirectionState == "down")
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_DOWN : ANIM_IDLE_DOWN);
                else if (lastDirectionState == "left")
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_LEFT : ANIM_IDLE_LEFT);
                else if (lastDirectionState == "right")
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_RIGHT : ANIM_IDLE_RIGHT);
                else
                    ChangeAnimationState(usePlaneIdle ? ANIM_IDLE_PLANE_DOWN : ANIM_IDLE_DOWN);
            }
        }
    
        public void ChangeAnimationState(string newState)
        {
            if (animator == null || currentAnimationState == newState)
                return;
        
            animator.Play(newState);
            currentAnimationState = newState;
        }
    }
}
