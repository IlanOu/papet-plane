using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        // AudioSource pour les SFX
        [SerializeField] private AudioSource sfxAudioSource;
        
        // AudioSource dédié pour la musique d'ambiance
        [SerializeField] private AudioSource musicAudioSource;
        
        // La musique d'ambiance à jouer
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private float backgroundMusicVolume = 0.5f;

        private void Awake()
        {
            // Implémentation du singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // S'assurer que les AudioSources existent
            if (sfxAudioSource == null)
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
                
            if (musicAudioSource == null)
                musicAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        private void Start()
        {
            // Configurer et démarrer la musique d'ambiance
            PlayBackgroundMusic(backgroundMusic, backgroundMusicVolume);
        }

        /// <summary>
        /// Joue un clip audio depuis le AudioManager.
        /// </summary>
        public void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioManager.PlaySound() - Aucun clip audio n'a été assigné.");
                return;
            }

            sfxAudioSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Joue une musique d'ambiance en boucle.
        /// </summary>
        public void PlayBackgroundMusic(AudioClip musicClip, float volume = 0.5f)
        {
            if (musicClip == null)
            {
                Debug.LogWarning("AudioManager.PlayBackgroundMusic() - Aucun clip audio n'a été assigné.");
                return;
            }
            
            musicAudioSource.clip = musicClip;
            musicAudioSource.loop = true;
            musicAudioSource.volume = volume;
            musicAudioSource.Play();
        }
        
        /// <summary>
        /// Change le volume de la musique d'ambiance.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicAudioSource.volume = volume;
        }
        
        /// <summary>
        /// Arrête la musique d'ambiance.
        /// </summary>
        public void StopBackgroundMusic()
        {
            musicAudioSource.Stop();
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