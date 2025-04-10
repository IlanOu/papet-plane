using Projectile;
using UnityEngine;

namespace Bonus
{
    public class BonusManager : MonoBehaviour
    {
        private BallBonus currentBonus;
        private DefaultBallBonus defaultBonus;
    
        [SerializeField] private float bonusDuration = 5f;
        
        void Awake()
        {
            defaultBonus = new DefaultBallBonus();
            currentBonus = defaultBonus;
        }
        
        // Delete the bonus after the specified duration
        void Update()
        {
            if (currentBonus != null && currentBonus != defaultBonus)
            {
                if (Time.time > currentBonus.duration + Time.time)
                {
                    ResetBonus();
                }
            }
        }
    
        public void ActivateBonus(BallBonus bonus)
        {
            currentBonus = bonus;
        }
    
        public void ResetBonus()
        {
            currentBonus = defaultBonus;
        }
    
        public BallBonus GetCurrentBonus()
        {
            return currentBonus;
        }
    }
}