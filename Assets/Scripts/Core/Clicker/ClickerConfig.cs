namespace UnityEngine
{
    [CreateAssetMenu(fileName = "ClickerConfig", menuName = "ScriptableObjects/Config/ClickerConfig", order = 1)]
    public class ClickerConfig : ScriptableObject
    {
        public float animationDuration = 0.5f;
        
        [Space]
        public Vector3 moveDirection = new Vector3(0f, 1f, 0f);
        public float moveValue = 1f;

        [Space] 
        public float finalScale = 0.5f;
    }
}