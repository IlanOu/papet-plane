using System.Collections;
using UnityEngine;

public class PlayerReloadController : MonoBehaviour
{
    [Header("Reload Settings")]
    public float reloadTime = 3f;
    
    [Tooltip("Durée par défaut du clip \"Reload\" à vitesse normale.")] 
    public float defaultReloadAnimationDuration = 0.5f;  
    
    private PlayerAnimationController animationController;
    private BallShooter ballShooter;
    
    private Coroutine reloadCoroutine;
    [HideInInspector] public bool isCurrentlyReloading = false;
    
    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        // Le BallShooter est récupéré via le PlayerController
        ballShooter = GetComponent<PlayerController>().ballShooter;
    }
    
    // Méthode à appeler pour lancer le reload
    public void OnReloading()
    {
        if (isCurrentlyReloading)
            return;
        
        isCurrentlyReloading = true;
        
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);
            
        reloadCoroutine = StartCoroutine(PlayReloadAnimation(reloadTime));
    }
    
    private IEnumerator PlayReloadAnimation(float reloadTime)
    {
        // Calculer le facteur de vitesse pour que l'animation Reload dure exactement reloadTime secondes.
        // Par exemple, si la durée par défaut du clip est 1 seconde et reloadTime vaut 3, alors on doit jouer l'animation à 1/3 de sa vitesse.
        float reloadAnimationSpeed = defaultReloadAnimationDuration / reloadTime;
        animationController.animator.speed = reloadAnimationSpeed;
        
        // Lancer l'animation "Reload" de la même manière que les autres animations.
        animationController.ChangeAnimationState(animationController.reloadAnimation);
        
        // Attendre exactement reloadTime secondes pendant que l'animation se joue.
        yield return new WaitForSeconds(reloadTime);
        
        // Une fois le reload terminé, réinitialiser la vitesse de l'animation à 1.
        animationController.animator.speed = 1f;
        
        isCurrentlyReloading = false;
        animationController.UpdateIdleAnimation();
        reloadCoroutine = null;
    }
    
    public bool IsReloading() => isCurrentlyReloading;
}
