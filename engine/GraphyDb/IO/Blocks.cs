using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphyDb.IO
{
    public class NodeBlock
    {
        public bool Used;
        public int NodeId;
        public int NextRelationId;
        public int NextPropertyId;
        public int LabelId;

        public NodeBlock(bool used, int nodeId, int nextRelationId, int nextPropertyId, int labelId)
        {
            Used = used;
            NodeId = nodeId;
            NextRelationId = nextRelationId;
            NextPropertyId = nextPropertyId;
            LabelId = labelId;
        }
    }

    public class LabelBlock
    {
        public bool Used;
        public string Data;
        public int LabelId;

        public LabelBlock(bool used, string data, int labelId)
        {
            Used = used;
            Data = data;
            LabelId = labelId;
        }
    }
}
