using System;

namespace GraphyDb.IO
{
    public class NodeBlock : IEquatable<NodeBlock>
    {
        public bool Equals(NodeBlock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return NodeId == other.NodeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NodeBlock) obj);
        }

        public override int GetHashCode()
        {
            return NodeId;
        }

        public static bool operator ==(NodeBlock left, NodeBlock right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NodeBlock left, NodeBlock right)
        {
            return !Equals(left, right);
        }

        public bool Used;
        public int NodeId;
        public int FirstInRelationId;
        public int FirstOutRelationId;
        public int NextPropertyId;
        public int LabelId;

        public NodeBlock(bool used, int nodeId, int firstInRelationId, int firstOutRelationId,  int nextPropertyId, int labelId)
        {
            Used = used;
            NodeId = nodeId;
            FirstInRelationId = firstInRelationId;
            NextPropertyId = nextPropertyId;
            FirstOutRelationId = firstOutRelationId;
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
            this.StoragePath = DbControl.LabelPath;
        }

        public LabelBlock(bool used, string data, int id) : base(DbControl.LabelPath, used, data, id)
        {
        }
    }

    public class PropertyNameBlock : GenericStringBlock
    {
        public PropertyNameBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
        {
            this.StoragePath = DbControl.PropertyNamePath;
        }

        public PropertyNameBlock(bool used, string data, int id) : base(DbControl.PropertyNamePath, used, data, id)
        {
        }
    }

    public class StringBlock : GenericStringBlock
    {
        public StringBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
        {
            this.StoragePath = DbControl.StringPath;
        }

        public StringBlock(bool used, string data, int id) : base(DbControl.StringPath, used, data, id)
        {
        }
    }

    public class EdgeBlock : IEquatable<EdgeBlock>
    {
        public bool Equals(EdgeBlock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EdgeId == other.EdgeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EdgeBlock) obj);
        }

        public override int GetHashCode()
        {
            return EdgeId;
        }

        public static bool operator ==(EdgeBlock left, EdgeBlock right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EdgeBlock left, EdgeBlock right)
        {
            return !Equals(left, right);
        }

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
        public byte[] Value;
        public int NextProperty;
        public int NodeId;

        public PropertyBlock(string storagePath, int id, bool used, PropertyType ptType, int propertyName, byte[] value,
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
            this.StoragePath = DbControl.NodePropertyPath;
        }

        public NodePropertyBlock(int id, bool used, PropertyType ptType, int propertyName, byte[] value, int nextProperty,
            int nodeId) : base(DbControl.NodePropertyPath, id, used, ptType, propertyName, value, nextProperty, nodeId)
        {
        }
    }

    public class EdgePropertyBlock : PropertyBlock
    {
        public EdgePropertyBlock(PropertyBlock other) : base(other)
        {
            this.StoragePath = DbControl.EdgePropertyPath;
        }

        public EdgePropertyBlock(int id, bool used, PropertyType ptType, int propertyName, byte[] value, int nextProperty,
            int nodeId) : base(DbControl.EdgePropertyPath, id, used, ptType, propertyName, value, nextProperty, nodeId)
        {
        }
    }
}