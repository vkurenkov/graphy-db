using UnityEngine;

public class PrimitiveObject : MonoBehaviour {
    public string Shape { get { return m_shape; } }
    public string Color { get { return m_color; } }

    public string ToJson()
    {
        var packed = new Packed();
        packed.X = this.transform.position.x;
        packed.Y = this.transform.position.y;
        packed.Shape = this.Shape;
        packed.Color = this.Color;

        return JsonUtility.ToJson(packed);
    }

    private struct Packed
    {
        public float X;
        public float Y;
        public string Shape;
        public string Color;
    }

    [SerializeField]
    private string m_shape;
    [SerializeField]
    private string m_color;
}
