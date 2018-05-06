using UnityEngine;

public class PrimitiveObject : MonoBehaviour {
    public string Shape { get { return m_shape; } }
    public string Color { get { return m_color; } }

    public string ToJson()
    {
        var packed = new Packed();
        packed.X = this.transform.position.x.Truncate(2);
        packed.Y = this.transform.position.y.Truncate(2);
        packed.Shape = this.Shape;
        packed.Color = this.Color;

        return JsonUtility.ToJson(packed);
    }

    private void Update()
    {
        this.transform.GetComponentInChildren<TextMesh>().text =
            "X: " + this.transform.position.x.Truncate(2).ToString() + "\n" +
            "Y: " + this.transform.position.y.Truncate(2).ToString();
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
