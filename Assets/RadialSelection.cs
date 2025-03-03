using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class RadialSelection : MonoBehaviour
{
    public OVRInput.Button spawnButton;
    public GameObject radialPartPrefab;
    public Transform radialPartCanvas;
    public Transform handTransform;

    public UnityEvent OnMuteSelected;
    public UnityEvent OnUnmuteSelected;

    public SetColorFromList cubeColor; // Referencia al cubo

    private List<GameObject> spawnedParts = new List<GameObject>();
    private int currentSelectedRadialPart = -1;
    private const int numberOfRadialPart = 3; // Menú fijo en 3 opciones

    void Update()
    {
        if (OVRInput.GetDown(spawnButton))
        {
            SpawnRadialPart();
        }
        if (OVRInput.Get(spawnButton))
        {
            GetSelectedRadialPart();
        }
        if (OVRInput.GetUp(spawnButton))
        {
            HideAndTriggerSelected();
        }
    }

    public void HideAndTriggerSelected()
    {
        if (currentSelectedRadialPart == 0)
        {
            OnMuteSelected.Invoke(); // Mute
            cubeColor.SetColor(0);   // Rojo
        }
        else if (currentSelectedRadialPart == 1)
        {
            cubeColor.SetColor(2); // Azul (opción para cambiar de escena en el futuro)
        }
        else if (currentSelectedRadialPart == 2)
        {
            
            OnUnmuteSelected.Invoke(); // Unmute
            cubeColor.SetColor(1);     // Amarillo
        }

        radialPartCanvas.gameObject.SetActive(false);
    }

    public void GetSelectedRadialPart()
    {
        Vector3 centerToHand = handTransform.position - radialPartCanvas.transform.position;
        Vector3 centerToHandProjected = Vector3.ProjectOnPlane(centerToHand, radialPartCanvas.forward);
        float angle = Vector3.SignedAngle(radialPartCanvas.up, centerToHandProjected, -radialPartCanvas.forward);
        if (angle < 0) angle += 360;

        currentSelectedRadialPart = (int)(angle * numberOfRadialPart / 360);

        // Debug en consola
        Debug.Log("Selección radial: " + currentSelectedRadialPart);

        for (int i = 0; i < spawnedParts.Count; i++)
        {
            if (i == currentSelectedRadialPart)
            {
                spawnedParts[i].GetComponent<Image>().color = Color.yellow;
                spawnedParts[i].transform.localScale = 1.1f * Vector3.one;
            }
            else
            {
                spawnedParts[i].GetComponent<Image>().color = Color.white;
                spawnedParts[i].transform.localScale = Vector3.one;
            }
        }
    }

    public void SpawnRadialPart()
    {
        radialPartCanvas.gameObject.SetActive(true);
        radialPartCanvas.position = handTransform.position;
        radialPartCanvas.rotation = handTransform.rotation;

        foreach (var item in spawnedParts)
        {
            Destroy(item);
        }
        spawnedParts.Clear();

        for (int i = 0; i < numberOfRadialPart; i++)
        {
            float angle = -i * 360 / numberOfRadialPart;
            GameObject spawnedRadialPart = Instantiate(radialPartPrefab, radialPartCanvas);
            spawnedRadialPart.transform.position = radialPartCanvas.position;
            spawnedRadialPart.transform.localEulerAngles = new Vector3(0, 0, angle);
            spawnedParts.Add(spawnedRadialPart);
        }
    }
}
