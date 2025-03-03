using UnityEngine;

/// <summary>
/// Optional enhanced material effects for the emotion balloons.
/// Add this to the balloon prefab for more visual feedback during interactions.
/// </summary>
public class BalloonVisualEffects : MonoBehaviour
{
    [Header("Glow Effect Settings")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private float maxEmissionIntensity = 2.0f;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private Color emissionColor = Color.white;

    [Header("Components")]
    [SerializeField] private Renderer balloonRenderer;

    // Reference to the controller to know when activated
    private BalloonEmotionController controller;
    private bool isGlowing = false;
    private Material instancedMaterial;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Start()
    {
        // Get references
        controller = GetComponent<BalloonEmotionController>();

        if (balloonRenderer == null)
            balloonRenderer = transform.GetChild(0).GetComponent<Renderer>();

        // Create an instanced material to avoid affecting other balloons
        instancedMaterial = new Material(defaultMaterial);
        balloonRenderer.material = instancedMaterial;

        // Enable emission
        instancedMaterial.EnableKeyword("_EMISSION");

        // Listen for interaction events
        if (controller != null)
        {
            // You may need to add public events to the controller script
            // For demonstration purposes, we'll just use a public method in that script
        }
    }

    // This should be called by the BalloonEmotionController when activated
    public void StartGlowEffect()
    {
        isGlowing = true;
    }

    // This should be called by the BalloonEmotionController when deactivated
    public void StopGlowEffect()
    {
        isGlowing = false;
        // Reset emission
        instancedMaterial.SetColor(EmissionColorID, Color.black);
    }

    private void Update()
    {
        if (isGlowing)
        {
            // Calculate pulsing emission intensity
            float emission = Mathf.PingPong(Time.time * pulseSpeed, maxEmissionIntensity);
            Color finalEmissionColor = emissionColor * emission;

            // Apply to material
            instancedMaterial.SetColor(EmissionColorID, finalEmissionColor);
        }
    }

    private void OnDestroy()
    {
        // Clean up instanced material
        if (instancedMaterial != null)
        {
            Destroy(instancedMaterial);
        }
    }
}

/// <summary>
/// Optional particle effect system for when balloons are touched.
/// Add this to the balloon prefab for additional visual feedback.
/// </summary>
public class BalloonTouchParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem touchParticles;
    [SerializeField] private Color particleColor = Color.white;
    [SerializeField] private float particleDuration = 1.0f;

    private void Start()
    {
        // Create particles if not assigned
        if (touchParticles == null)
        {
            touchParticles = GetComponentInChildren<ParticleSystem>();

            if (touchParticles == null)
            {
                // Create a new particle system
                GameObject particleObj = new GameObject("TouchParticles");
                particleObj.transform.SetParent(transform);
                particleObj.transform.localPosition = Vector3.zero;

                touchParticles = particleObj.AddComponent<ParticleSystem>();

                // Configure particle system
                var main = touchParticles.main;
                main.duration = particleDuration;
                main.loop = false;
                main.startLifetime = 2.0f;
                main.startSpeed = 0.2f;
                main.startSize = 0.05f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var emission = touchParticles.emission;
                emission.enabled = false;

                var shape = touchParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.1f;

                var colorOverLifetime = touchParticles.colorOverLifetime;
                colorOverLifetime.enabled = true;

                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(particleColor, 0.0f), new GradientColorKey(particleColor, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                colorOverLifetime.color = gradient;
            }
        }

        // Ensure it's not playing at start
        touchParticles.Stop();
    }

    // This should be called by the BalloonEmotionController when touched
    public void PlayTouchEffect()
    {
        if (touchParticles != null)
        {
            // Set the color based on the current balloon color
            var main = touchParticles.main;
            main.startColor = particleColor;

            // Play the effect
            touchParticles.Play();
        }
    }
}