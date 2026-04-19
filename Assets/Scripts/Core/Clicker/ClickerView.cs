namespace UnityEngine
{
    public class ClickerView : MonoBehaviour
    {
        [SerializeField] private GameObject _xpPrefab;
        [SerializeField] private GameObject _panel;

        public GameObject xpPrefab => _xpPrefab;
        public GameObject panel => _panel;
    }
}