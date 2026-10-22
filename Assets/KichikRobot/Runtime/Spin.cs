using UnityEngine;

/// <summary>Orqa fondagi tishli g'ildiraklarni sekin aylantiradi.</summary>
public class Spin : MonoBehaviour
{
    public float degreesPerSecond = 8f;

    void Update()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
    }
}
