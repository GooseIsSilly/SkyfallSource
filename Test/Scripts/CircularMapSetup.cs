using UnityEngine;
using UnityEngine.UI;

namespace TPSBR
{
    [RequireComponent(typeof(Image))]
    public class CircularMapSetup : MonoBehaviour
    {
        [Header("Circular Map Settings")]
        [SerializeField] private bool useCircularMask = true;
        [SerializeField] private Image mapImage;
        [SerializeField] private Sprite circleSprite;

        private void Start()
        {
            SetupCircularMap();
        }

        private void SetupCircularMap()
        {
            if (!useCircularMask || mapImage == null)
                return;

            Image maskImage = GetComponent<Image>();
            if (maskImage != null)
            {
                maskImage.type = Image.Type.Filled;
                maskImage.fillMethod = Image.FillMethod.Radial360;
                maskImage.fillOrigin = 0;
                maskImage.fillAmount = 1f;
                
                if (circleSprite != null)
                {
                    maskImage.sprite = circleSprite;
                }
                
                Mask mask = gameObject.GetComponent<Mask>();
                if (mask == null)
                {
                    mask = gameObject.AddComponent<Mask>();
                }
                mask.showMaskGraphic = false;
            }
        }

        public void SetCircularMaskEnabled(bool enabled)
        {
            useCircularMask = enabled;
            SetupCircularMap();
        }
    }
}
