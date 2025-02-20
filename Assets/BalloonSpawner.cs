using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonSpawner : MonoBehaviour
{
    public GameObject balloonPrefab; // Prefab del globo
    public OVRInput.RawButton spawnButton = OVRInput.RawButton.RIndexTrigger; // Botón para generar el globo
    public Transform spawnPoint; // Punto de aparición del globo
    public float floatingForce = 2.0f; // Fuerza de flotación para que el globo suba
    public float randomDriftForce = 0.3f; // Pequeña fuerza para simular deriva del aire
    public AudioSource source;
    public AudioClip shootingAudioClip;

    void Update()
    {
        // Detectar si se ha presionado el botón especificado
        if (OVRInput.GetDown(spawnButton))
        {
            SpawnBalloon();
        }
    }

    void SpawnBalloon()
    {
        // Instanciar el globo en la posición y rotación del punto de aparición
        source.PlayOneShot(shootingAudioClip);
        GameObject balloon = Instantiate(balloonPrefab, spawnPoint.position, spawnPoint.rotation);

        // Obtener el Rigidbody
        Rigidbody rb = balloon.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = balloon.AddComponent<Rigidbody>();
        }

        // Configurar propiedades físicas
        rb.mass = 0.1f; // Masa ligera
        rb.useGravity = false; // Desactivar la gravedad de Unity
        rb.drag = 1.5f; // Más alto para que se mueva lentamente
        rb.angularDrag = 0.8f; // Evita giros bruscos
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Mejor detección de colisiones

        // Añadir un Collider si no existe
        if (balloon.GetComponent<Collider>() == null)
        {
            balloon.AddComponent<SphereCollider>();
        }

        // Configurar material físico para el rebote
        PhysicMaterial physicMaterial = new PhysicMaterial();
        physicMaterial.bounciness = 0.8f; // Más rebote
        physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        physicMaterial.bounceCombine = PhysicMaterialCombine.Maximum;

        balloon.GetComponent<Collider>().material = physicMaterial;

        // Iniciar la flotación del globo
        StartCoroutine(ApplyFloatingForce(rb));
    }

    IEnumerator ApplyFloatingForce(Rigidbody rb)
    {
        while (rb != null)
        {
            // Aplicar una fuerza hacia arriba para simular la flotación
            rb.AddForce(Vector3.up * floatingForce, ForceMode.Acceleration);

            // Agregar una ligera fuerza aleatoria para simular la deriva del viento
            Vector3 randomDrift = new Vector3(
                Random.Range(-randomDriftForce, randomDriftForce),
                0,
                Random.Range(-randomDriftForce, randomDriftForce)
            );

            rb.AddForce(randomDrift, ForceMode.Acceleration);

            yield return new WaitForSeconds(0.1f); // Pequeño retraso para hacer la simulación más realista
        }
    }
}
