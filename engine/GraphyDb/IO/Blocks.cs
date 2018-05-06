using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphyDb;
using GraphyDb.IO;

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

    public class GenericStringBlock
    {
        public string StoragePath;
        public bool Used;
        public string Data;
        public int Id;

        public GenericStringBlock(string storagePath, bool used, string data, int id)
        {
            StoragePath = storagePath;
            Used = used;
            Data = data;
            Id = id;
        }

        protected GenericStringBlock(GenericStringBlock other)
        {
            this.StoragePath = other.StoragePath;
            this.Used = other.Used;
            this.Data = other.Data;
            this.Id = other.Id;
        }
    }

    public class LabelBlock : GenericStringBlock
    {
        public LabelBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
        {
            this.StoragePath = DbWriter.LabelPath;
        }

        public LabelBlock(bool used, string data, int id) : base(DbWriter.LabelPath, used, data, id)
        {
        }
    }

    public class PropertyNameBlock : GenericStringBlock
    {
        public PropertyNameBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
        {
            this.StoragePath = DbWriter.PropertyNamePath;
        }

        public PropertyNameBlock(bool used, string data, int id) : base(DbWriter.PropertyNamePath, used, data, id)
        {
        }
    }

    public class StringBlock : GenericStringBlock
    {
        public StringBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
        {
            this.StoragePath = DbWriter.StringPath;
        }

        public StringBlock(bool used, string data, int id) : base(DbWriter.StringPath, used, data, id)
        {
        }
    }

    public class EdgeBlock
    {
        public bool Used;
        public int FirstNode;
        public int SecondNode;
        public int FirstNodePreviousRelation;
        public int FirstNodeNextRelation;
        public int SecondNodePreviousRelation;
        public int SecondNodeNextRelation;
        public int NextProperty;
        public int LabelId;
        public int EdgeId;

        public EdgeBlock()
        {
        }

        public EdgeBlock(bool used, int firstNode, int secondNode, int firstNodePreviousRelation,
            int firstNodeNextRelation, int secondNodePreviousRelation, int secondNodeNextRelation, int nextProperty,
            int labelId, int edgeId)
        {
            Used = used;
            FirstNode = firstNode;
            SecondNode = secondNode;
            FirstNodePreviousRelation = firstNodePreviousRelation;
            FirstNodeNextRelation = firstNodeNextRelation;
            SecondNodePreviousRelation = secondNodePreviousRelation;
            SecondNodeNextRelation = secondNodeNextRelation;
            NextProperty = nextProperty;
            LabelId = labelId;
            EdgeId = edgeId;
        }
    }

    public class PropertyBlock
    {
        public string StoragePath;
        public int Id;
        public bool Used;
        public PropertyType PtType;
        public int PropertyName;
        public int Value;
        public int NextProperty;
        public int NodeId;

        public PropertyBlock(string storagePath, int id, bool used, PropertyType ptType, int propertyName, int value,
            int nextProperty, int nodeId)
        {
            StoragePath = storagePath;
            Id = id;
            Used = used;
            PtType = ptType;
            PropertyName = propertyName;
            Value = value;
            NextProperty = nextProperty;
            NodeId = nodeId;
        }

        public PropertyBlock(PropertyBlock other)
        {
            this.StoragePath = other.StoragePath;
            this.Id = other.Id;
            this.NextProperty = other.NextProperty;
            this.PropertyName = other.PropertyName;
            this.NodeId = other.NodeId;
            this.PtType = other.PtType;
            this.Used = other.Used;
            this.Value = other.Value;
        }
    }

    public class NodePropertyBlock : PropertyBlock
    {
        public NodePropertyBlock(PropertyBlock other) : base(other)
        {
            this.StoragePath = DbWriter.NodePropertyPath;
        }

        public NodePropertyBlock(int id, bool used, PropertyType ptType, int propertyName, int value, int nextProperty,
            int nodeId) : base(DbWriter.NodePropertyPath, id, used, ptType, propertyName, value, nextProperty, nodeId)
        {
        }
    }

    public class EdgePropertyBlock : PropertyBlock
    {
        public EdgePropertyBlock(PropertyBlock other) : base(other)
        {
            this.StoragePath = DbWriter.EdgePropertyPath;
        }

        public EdgePropertyBlock(int id, bool used, PropertyType ptType, int propertyName, int value, int nextProperty,
            int nodeId) : base(DbWriter.EdgePropertyPath, id, used, ptType, propertyName, value, nextProperty, nodeId)
        {
        }
    }

}