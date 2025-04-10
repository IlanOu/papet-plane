using System;
using Projectile;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerHit : MonoBehaviour
    {
        [Header("Composants")]
        [SerializeField] private PlayerController playerController;
        
        [Header("Events")]
        public UnityEvent onHit;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Plane"))
            {
                if (other.GetComponent<BallBehaviour>() != null)
                    if (playerController.playerIndex == other.GetComponent<BallBehaviour>().ownerIndex) return;
                
                onHit.Invoke();
                Destroy(other.gameObject);
            }
        }
    }
}