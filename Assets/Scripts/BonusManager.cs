using UnityEngine;

public class BonusManager : MonoBehaviour
{
    private BallBonus currentBonus;
    private DefaultBallBonus defaultBonus;
    
    void Awake()
    {
        defaultBonus = new DefaultBallBonus();
        currentBonus = defaultBonus;
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