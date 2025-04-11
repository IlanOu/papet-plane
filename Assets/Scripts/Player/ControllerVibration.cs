using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public static class ControllerVibration
{
    private static MonoBehaviour _coroutineRunner;
    private static Dictionary<int, Coroutine> _currentVibrations = new Dictionary<int, Coroutine>();

    // Initialise le runner de coroutines si nécessaire
    private static void EnsureInitialized()
    {
        if (_coroutineRunner == null)
        {
            GameObject go = new GameObject("ControllerVibrationRunner");
            _coroutineRunner = go.AddComponent<CoroutineRunner>();
            Object.DontDestroyOnLoad(go);
        }
    }

    /// <summary>
    /// Fait vibrer la manette d'un joueur spécifique
    /// </summary>
    /// <param name="playerIndex">Index du joueur (0 pour joueur 1, 1 pour joueur 2, etc.)</param>
    /// <param name="lowFrequency">Intensité des moteurs à basse fréquence (0-1)</param>
    /// <param name="highFrequency">Intensité des moteurs à haute fréquence (0-1)</param>
    /// <param name="duration">Durée de la vibration en secondes</param>
    public static void Vibrate(int playerIndex, float lowFrequency, float highFrequency, float duration)
    {
        EnsureInitialized();
        
        // Arrêter une vibration existante pour ce joueur
        if (_currentVibrations.TryGetValue(playerIndex, out Coroutine coroutine) && coroutine != null)
        {
            _coroutineRunner.StopCoroutine(coroutine);
        }
        
        // Obtenir la manette du joueur spécifique
        Gamepad gamepad = GetGamepadForPlayer(playerIndex);
        if (gamepad == null) return;
        
        // Démarrer la nouvelle vibration
        Coroutine newCoroutine = _coroutineRunner.StartCoroutine(VibrateCoroutine(gamepad, playerIndex, lowFrequency, highFrequency, duration));
        _currentVibrations[playerIndex] = newCoroutine;
    }

    /// <summary>
    /// Fait vibrer la manette actuellement active (si un seul joueur)
    /// </summary>
    public static void Vibrate(float lowFrequency, float highFrequency, float duration)
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;
        
        // Trouver l'index du joueur pour cette manette
        int playerIndex = GetPlayerIndexForGamepad(gamepad);
        Vibrate(playerIndex, lowFrequency, highFrequency, duration);
    }

    /// <summary>
    /// Fait vibrer la manette d'un joueur spécifique (vibration légère)
    /// </summary>
    public static void VibrateLight(int playerIndex, float duration = 0.2f)
    {
        Vibrate(playerIndex, 0.2f, 0.2f, duration);
    }

    /// <summary>
    /// Fait vibrer la manette d'un joueur spécifique (vibration moyenne)
    /// </summary>
    public static void VibrateMedium(int playerIndex, float duration = 0.4f)
    {
        Vibrate(playerIndex, 0.4f, 0.4f, duration);
    }

    /// <summary>
    /// Fait vibrer la manette d'un joueur spécifique (vibration forte)
    /// </summary>
    public static void VibrateStrong(int playerIndex, float duration = 0.6f)
    {
        Vibrate(playerIndex, 0.7f, 0.7f, duration);
    }

    /// <summary>
    /// Fait vibrer la manette d'un joueur spécifique (vibration impact)
    /// </summary>
    public static void VibrateImpact(int playerIndex)
    {
        Vibrate(playerIndex, 1.0f, 0.7f, 0.1f);
    }

    /// <summary>
    /// Arrête la vibration pour un joueur spécifique
    /// </summary>
    public static void StopVibration(int playerIndex)
    {
        if (_coroutineRunner != null && _currentVibrations.TryGetValue(playerIndex, out Coroutine coroutine) && coroutine != null)
        {
            _coroutineRunner.StopCoroutine(coroutine);
            _currentVibrations.Remove(playerIndex);
        }
        
        Gamepad gamepad = GetGamepadForPlayer(playerIndex);
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    /// <summary>
    /// Arrête toutes les vibrations sur toutes les manettes
    /// </summary>
    public static void StopAllVibrations()
    {
        if (_coroutineRunner != null)
        {
            foreach (var coroutine in _currentVibrations.Values)
            {
                if (coroutine != null)
                    _coroutineRunner.StopCoroutine(coroutine);
            }
            _currentVibrations.Clear();
        }
        
        // Arrêter les vibrations sur toutes les manettes connectées
        var gamepads = Gamepad.all;
        foreach (var gamepad in gamepads)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    /// <summary>
    /// Obtient la manette associée à un joueur spécifique
    /// </summary>
    private static Gamepad GetGamepadForPlayer(int playerIndex)
    {
        // Si nous avons suffisamment de manettes connectées
        if (Gamepad.all.Count > playerIndex)
        {
            return Gamepad.all[playerIndex];
        }
        
        // Fallback à la manette actuelle si aucune n'est trouvée
        return Gamepad.current;
    }

    /// <summary>
    /// Obtient l'index du joueur pour une manette spécifique
    /// </summary>
    private static int GetPlayerIndexForGamepad(Gamepad gamepad)
    {
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            if (Gamepad.all[i] == gamepad)
                return i;
        }
        return 0; // Par défaut, retourne le joueur 1
    }

    /// <summary>
    /// Vibration pour un joueur utilisant PlayerInput
    /// </summary>
    public static void VibrateForPlayer(PlayerInput playerInput, float lowFrequency, float highFrequency, float duration)
    {
        if (playerInput != null)
        {
            Vibrate(playerInput.playerIndex, lowFrequency, highFrequency, duration);
        }
    }

    private static IEnumerator VibrateCoroutine(Gamepad gamepad, int playerIndex, float lowFrequency, float highFrequency, float duration)
    {
        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        yield return new WaitForSeconds(duration);
        gamepad.SetMotorSpeeds(0f, 0f);
        _currentVibrations.Remove(playerIndex);
    }
}

// Classe helper pour exécuter les coroutines
public class CoroutineRunner : MonoBehaviour 
{
    private void OnDestroy()
    {
        ControllerVibration.StopAllVibrations();
    }
}