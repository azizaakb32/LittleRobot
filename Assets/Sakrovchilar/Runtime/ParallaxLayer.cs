using UnityEngine;

/// <summary>
/// Orqa fon qatlami (bulut, tepalik): kamera siljiganda sekinroq siljiydi va chuqurlik hissini beradi.
/// factor: 0 = qotib turadi, 1 = dunyo bilan birga harakatlanadi.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    public float factorX = 0.3f;
    public float factorY = 0.3f;

    Transform cam;
    Vector3 startCam;
    Vector3 startPos;
    bool ready;

    void LateUpdate()
    {
        if (!ready)
        {
            var c = Camera.main;
            if (c == null) return;
            cam = c.transform;
            startCam = cam.position;
            startPos = transform.position;
            ready = true;
        }
        Vector3 d = cam.position - startCam;
        transform.position = new Vector3(startPos.x + d.x * factorX, startPos.y + d.y * factorY, startPos.z);
    }
}
