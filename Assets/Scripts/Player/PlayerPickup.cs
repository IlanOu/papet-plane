using UnityEngine;

namespace Player
{
    public class PlayerPickup : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
    
        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Magazine"))
            {
                ControllerVibration.VibrateMedium(playerController.playerIndex, 0.5f);
                playerController.ballShooter.FillAmmo(playerController.ballShooter.magazineCapacity);
                Destroy(other.gameObject);
            }
        }
    }
}