using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        // Un AudioSource utilisé pour jouer les sons (tu peux en ajouter d'autres si besoin)
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            // Implémentation du singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Optionnel, si tu veux que l'audio persiste entre les scènes
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Joue un clip audio depuis le AudioManager.
        /// </summary>
        /// <param name="clip">Le clip audio à jouer</param>
        /// <param name="volume">Volume optionnel (par défaut: 1)</param>
        public void PlaySound(AudioClip clip, float volume = 1f)
        {
            // Vérifier que le clip n'est pas nul
            if (clip == null)
            {
                Debug.LogWarning("AudioManager.PlaySound() - Aucun clip audio n'a été assigné.");
                return;
            }

            // Joue le son via l'AudioSource (optionnel : peut être remplacé par PlayOneShot)
            audioSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Méthode statique pour jouer un son depuis n'importe quel script.
        /// </summary>
        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (Instance != null)
            {
                Instance.PlaySound(clip, volume);
            }
            else
            {
                Debug.LogWarning("AudioManager n'est pas présent dans la scène !");
            }
        }
    }
}