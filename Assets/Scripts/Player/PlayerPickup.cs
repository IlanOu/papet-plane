using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    private PlayerController playerController;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Magazine"))
        {
            playerController.ballShooter.FillAmmo(playerController.ballShooter.magazineCapacity);
            Destroy(other.gameObject);
        }
    }
}