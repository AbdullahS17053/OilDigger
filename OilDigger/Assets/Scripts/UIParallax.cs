using UnityEngine;
using UnityEngine.UI;

public class UIParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public RawImage image;       // RawImage allows texture offset
        public float speed = 0.1f;   // Scrolling speed
    }

    public ParallaxLayer[] layers;

    void Update()
    {
        foreach (var layer in layers)
        {
            if (layer.image != null)
            {
                Rect uv = layer.image.uvRect;
                uv.x += layer.speed * Time.deltaTime;
                layer.image.uvRect = uv;
            }
        }
    }
}
