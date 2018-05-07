using System;
using System.Collections.Generic;

public enum MessageType : byte { ParseObjects = 0, GetSideObjects = 1 };

public struct PrimitiveObject : IEquatable<PrimitiveObject>
{
    public float X;
    public float Y;
    public string Shape;
    public string Color;

    public override bool Equals(object obj)
    {
        return obj is PrimitiveObject && Equals((PrimitiveObject)obj);
    }

    public bool Equals(PrimitiveObject other)
    {
        return X == other.X &&
               Y == other.Y &&
               Shape == other.Shape &&
               Color == other.Color;
    }

    public override int GetHashCode()
    {
        var hashCode = -1471526351;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + X.GetHashCode();
        hashCode = hashCode * -1521134295 + Y.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Shape);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Color);
        return hashCode;
    }

    public override string ToString()
    {
        return "Primitive object: " + X.ToString() + "; " + Y.ToString() + "; " + Shape + "; " + Color + ";";
    }

    public static bool operator ==(PrimitiveObject object1, PrimitiveObject object2)
    {
        return object1.Equals(object2);
    }

    public static bool operator !=(PrimitiveObject object1, PrimitiveObject object2)
    {
        return !(object1 == object2);
    }
}

public class ParseObjectsMessage
{
    public List<PrimitiveObject> Objects;
}

public class GetSideObjects
{
    public string Shape;
    public string Color;
    public float PositionX;
    public float PositionY;
    public byte Side;

    public override string ToString()
    {
        return "Get side objects: " + Shape + "; " + Color + "; " + PositionX + "; " + 
            PositionY + "; " + Side + ";";
    }
}
