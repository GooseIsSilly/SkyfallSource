using UnityEngine;

namespace TPSBR
{
    public class MapPOI : MonoBehaviour
    {
        [SerializeField] private string _poiName = "POI";
        [SerializeField] private Color _poiColor = Color.red;

        public string POIName => _poiName;
        public Color POIColor => _poiColor;
    }
}
