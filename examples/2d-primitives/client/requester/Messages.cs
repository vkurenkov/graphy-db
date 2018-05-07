using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum MessageType : byte { GetSideObjects = 1 };
public enum Side: byte { Left = 0, Right = 1, Up = 2, Down = 3 };

struct GetSideObjects
{
    public string Shape;
    public string Color;
    public float PositionX;
    public float PositionY;
    public byte Side;
}

struct PrimitiveObject
{
    public string Color;
    public string Shape;
    public float X;
    public float Y;

    public override string ToString()
    {
        return "Color: " + Color + "; Shape: " + Shape + "; X: " + X.ToString() + "; Y: " + Y.ToString() + ";";
    }
}