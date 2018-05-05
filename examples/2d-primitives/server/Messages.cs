using System.Collections.Generic;

public enum MessageType : byte { ParseObjects = 0, GetNeighbors = 1, Between = 2 };

public class ParseObjectsMessage
{
    public struct PrimitiveObject
    {
        public float X;
        public float Y;
        public string Shape;
        public string Color;

        public override string ToString()
        {
            return X.ToString() + "; " + Y.ToString() + "; " + Shape + "; " + Color + ";";
        }
    }

    public List<PrimitiveObject> Objects;
}
