using System.Collections;
using Audio;
using Projectile;
using UnityEngine;

namespace Player
{
    public class PlayerReloadController : MonoBehaviour
    {
        [Header("Composants")]
        [SerializeField] private PlayerAnimationController animationController;
        [SerializeField] private BallShooter ballShooter;
    
        [Header("Reload Settings")]
        [Tooltip("Durée par défaut du clip \"Reload\" à vitesse normale.")] 
        public float defaultReloadAnimationDuration = 0.5f;  
    
        private Coroutine reloadCoroutine;
        [HideInInspector] public bool isCurrentlyReloading = false;
    
        [Header("SFX")]
        [SerializeField] private AudioClip reloadSFX;
        [SerializeField] private float reloadSFXVolume = 1f;
        
        private void Awake()
        {
            if (animationController == null)
                animationController = GetComponent<PlayerAnimationController>();
            
            if (ballShooter == null)
                ballShooter = GetComponent<BallShooter>();
        }
    
        // Méthode à appeler pour lancer le reload
        public void OnReloading()
        {
            if (isCurrentlyReloading)
                return;
        
            isCurrentlyReloading = true;
        
            if (reloadCoroutine != null)
                StopCoroutine(reloadCoroutine);
            
            AudioManager.Play(reloadSFX, reloadSFXVolume);
            reloadCoroutine = StartCoroutine(PlayReloadAnimation(ballShooter.reloadTime));
            StartCoroutine(ReloadVibration());
        }
    
        private IEnumerator ReloadVibration()
        {
            if (GetComponent<PlayerController>() == null)
                yield break;
            int playerIdx = GetComponent<PlayerController>().playerIndex;
    
            // Plusieurs petites vibrations pendant le rechargement
            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForSeconds(0.5f);
                ControllerVibration.VibrateMedium(playerIdx, 0.1f);
            }
        }
        
        private IEnumerator PlayReloadAnimation(float reloadTime)
        {
            // Calculer le facteur de vitesse pour que l'animation Reload dure exactement ballShooter.reloadTime secondes.
            // Par exemple, si la durée par défaut du clip est 1 seconde et ballShooter.reloadTime vaut 3, alors on doit jouer l'animation à 1/3 de sa vitesse.
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
}
