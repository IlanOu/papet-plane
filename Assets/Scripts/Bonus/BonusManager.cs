using Audio;
using Projectile;
using UnityEngine;

namespace Bonus
{
    public class BonusManager : MonoBehaviour
    {
        private BallBonus currentBonus;
        private DefaultBallBonus defaultBonus;
        private float bonusEndTime;
        
        [Header("Aura Visuelle")]
        [Tooltip("Référence au GameObject contenant le quad avec le sprite d'aura")]
        [SerializeField] private GameObject bonusAura;
        [SerializeField] private Color defaultAuraColor = Color.clear;
        
        [Header("SFX")]
        [SerializeField] private AudioClip bonusSFX;
        [SerializeField] private float bonusSFXVolume = 10f;
        
        void Awake()
        {
            defaultBonus = new DefaultBallBonus();
            currentBonus = defaultBonus;
            
            // S'assurer que l'aura est désactivée au démarrage
            if (bonusAura != null)
            {
                bonusAura.SetActive(false);
            }
            else
            {
                CreateBonusAura();
            }
        }
        
        // Crée l'aura si elle n'existe pas déjà
        private void CreateBonusAura()
        {
            // Créer un quad pour l'aura
            bonusAura = new GameObject("BonusAura");
            bonusAura.transform.parent = this.transform;
            bonusAura.transform.localPosition = Vector3.zero;
            
            // Ajouter un MeshRenderer et MeshFilter pour le quad
            MeshRenderer renderer = bonusAura.AddComponent<MeshRenderer>();
            MeshFilter filter = bonusAura.AddComponent<MeshFilter>();
            filter.mesh = CreateQuadMesh();
            
            // Configurer le matériau avec un shader approprié
            Material auraMaterial = new Material(Shader.Find("Sprites/Default"));
            renderer.material = auraMaterial;
            
            // Ajuster l'échelle pour qu'elle entoure le personnage
            bonusAura.transform.localScale = new Vector3(1.5f, 1.5f, 1);
            
            // Positionner l'aura derrière le personnage
            bonusAura.transform.localPosition = new Vector3(0, 0, 0.1f);
            
            // Désactiver l'aura au départ
            bonusAura.SetActive(false);
        }
        
        // Crée un simple quad pour l'aura
        private Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            
            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0)
            };
            
            int[] triangles = new int[6]
            {
                0, 2, 1,
                2, 3, 1
            };
            
            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            
            return mesh;
        }
        
        void Update()
        {
            // Vérifie si un bonus non-défaut est actif et s'il a expiré
            if (currentBonus != null && currentBonus != defaultBonus)
            {
                if (Time.time >= bonusEndTime)
                {
                    ResetBonus();
                    Debug.Log("Bonus expiré et retiré");
                }
            }
        }
    
        public void ActivateBonus(BallBonus bonus)
        {
            if (bonus == null) return;
            
            // Play SFX
            AudioManager.Play(bonusSFX, bonusSFXVolume);
            
            currentBonus = bonus;
            bonusEndTime = Time.time + bonus.duration;
            Debug.Log($"Bonus activé pour {bonus.duration} secondes");
            
            // Activer l'aura et définir sa couleur si possible
            if (bonusAura != null)
            {
                // Activer l'aura
                bonusAura.SetActive(true);
                
                // Changer la couleur de l'aura si le bonus a une propriété de couleur
                Renderer auraRenderer = bonusAura.GetComponent<Renderer>();
                if (auraRenderer != null)
                {
                    // Utiliser la couleur du bonus si disponible, sinon utiliser la couleur par défaut
                    Color auraColor = bonus.GetAuraColor() != null ? bonus.GetAuraColor() : defaultAuraColor;
                    auraRenderer.material.color = auraColor;
                }
            }
        }
    
        public void ResetBonus()
        {
            if (currentBonus != defaultBonus)
            {
                Debug.Log("Retour au bonus par défaut");
                currentBonus = defaultBonus;
                
                // Désactiver l'aura
                if (bonusAura != null)
                {
                    bonusAura.SetActive(false);
                }
            }
        }
    
        public BallBonus GetCurrentBonus()
        {
            return currentBonus;
        }
        
        public float GetRemainingBonusTime()
        {
            if (currentBonus == defaultBonus) return 0f;
            return Mathf.Max(0f, bonusEndTime - Time.time);
        }
    }
}