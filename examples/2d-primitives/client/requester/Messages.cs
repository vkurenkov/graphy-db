using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum MessageType : byte { GetSideObjects = 1, Between = 2 };
public enum Side: byte { Left = 0, Right = 1, Up = 2, Down = 3 };

struct GetSideObjects
{
    public string Shape;
    public string Color;
    public float PositionX;
    public float PositionY;
    public byte Side;
}