using Bonus;
using UnityEngine;

public class PowerUpBouncy : MonoBehaviour
{
    [SerializeField] private float bounciness = 0.8f;
    [SerializeField] private float lifeTime = 5f;
    
    private void OnTriggerEnter(Collider other)
    {
        // Vérifiez si c'est un joueur
        if (other.CompareTag("Player"))
        {
            // Trouvez le BonusManager de CE joueur spécifique
            BonusManager bonusManager = other.GetComponent<BonusManager>();
            if (bonusManager == null)
            {
                // Ou cherchez-le sur le parent/enfant si l'organisation de vos objets est différente
                bonusManager = other.GetComponentInParent<BonusManager>();
            }
            
            if (bonusManager != null)
            {
                Debug.Log("Activation du bonus Bouncy");
                BouncyBallBonus bonus = new BouncyBallBonus();
                // Configurer le bonus
                bonusManager.ActivateBonus(bonus);
                Destroy(gameObject);
            }
        }
    }
}