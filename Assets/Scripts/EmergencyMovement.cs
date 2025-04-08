using UnityEngine;

public class EmergencyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Animator animator;
    
    // Variables
    private Vector3 movement;
    private string currentAnim;
    
    // États d'animation
    private const string IDLE_DOWN = "Idle_Down";
    private const string WALK_UP = "Walk_Up";
    private const string WALK_DOWN = "Walk_Down";
    private const string WALK_LEFT = "Walk_Left";
    private const string WALK_RIGHT = "Walk_Right";
    
    void Update()
    {
        // Input de base de Unity (ancien système)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Créer le vecteur de mouvement
        movement = new Vector3(horizontal, 0, vertical).normalized;
        
        Debug.Log($"Input: H={horizontal}, V={vertical}, Movement={movement}");
        
        // Déplacer le personnage directement (sans Character Controller)
        transform.Translate(movement * moveSpeed * Time.deltaTime);
        
        // Animer
        if (movement.magnitude > 0.1f)
        {
            // Animation selon la direction
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.z))
            {
                // Mouvement horizontal dominant
                if (movement.x > 0)
                    PlayAnimation(WALK_RIGHT);
                else
                    PlayAnimation(WALK_LEFT);
            }
            else
            {
                // Mouvement vertical dominant
                if (movement.z > 0)
                    PlayAnimation(WALK_UP);
                else
                    PlayAnimation(WALK_DOWN);
            }
        }
        else
        {
            // Idle par défaut
            PlayAnimation(IDLE_DOWN);
        }
    }
    
    void PlayAnimation(string animName)
    {
        if (animator == null) return;
        if (currentAnim == animName) return;
        
        animator.Play(animName);
        currentAnim = animName;
        Debug.Log($"Animation jouée: {animName}");
    }
}