using System;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerHit : MonoBehaviour
    {
        
        [Header("Composants")]
        [SerializeField] private PlayerController playerController;
        
        [Header("Events")]
        public UnityEvent onCollisionEnter;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
        }
        
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Plane"))
            {
                onCollisionEnter.Invoke();
            }
        }
    }
}