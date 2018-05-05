using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum MessageType : byte { GetNeighbors = 1, Between = 2 };

struct GetNeighborsMessage
{
    public string Shape;
    public string Color;
    public int Range;
}