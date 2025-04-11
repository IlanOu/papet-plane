using Game;
using UnityEngine;

namespace Player
{
    public class PlayerShortcuts : MonoBehaviour
    {
        public void QuitGame()
        {
            GameManager.Instance.RestartGame();
        }
    }
}