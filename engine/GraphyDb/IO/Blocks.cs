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
            if (obj.GetType() != GetType()) return false;
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

        public NodeBlock(bool used, int nodeId, int firstInRelationId, int firstOutRelationId, int nextPropertyId,
            int labelId)
        {
            Used = used;
            NodeId = nodeId;
            FirstInRelationId = firstInRelationId;
            NextPropertyId = nextPropertyId;
            FirstOutRelationId = firstOutRelationId;
            LabelId = labelId;
        }
    }

    public abstract class GenericStringBlock
    {
        public bool Used;
        public string Data;
        public int Id;

        public GenericStringBlock(bool used, string data, int id)
        {
            Used = used;
            Data = data;
            Id = id;
        }

        protected GenericStringBlock(GenericStringBlock other)
        {
            Used = other.Used;
            Data = other.Data;
            Id = other.Id;
        }
    }

    public class LabelBlock : GenericStringBlock
    {
        public LabelBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
        {
        }

        public LabelBlock(bool used, string data, int id) : base(used, data, id)
        {
        }
    }

    public class PropertyNameBlock : GenericStringBlock
    {
        public PropertyNameBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
        {
        }

        public PropertyNameBlock(bool used, string data, int id) : base(used, data, id)
        {
        }
    }

    public class StringBlock : GenericStringBlock
    {
        public StringBlock(GenericStringBlock genericStringBlock) : base(genericStringBlock)
        {
        }

        public StringBlock(bool used, string data, int id) : base(used, data, id)
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
            if (obj.GetType() != GetType()) return false;
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

    public abstract class PropertyBlock
    {
        public int Id;
        public bool Used;
        public PropertyType PtType;
        public int PropertyName;
        public byte[] Value;
        public int NextProperty;
        public int NodeId;

        protected PropertyBlock(int id, bool used, PropertyType ptType, int propertyName, byte[] value,
            int nextProperty, int nodeId)
        {
            Id = id;
            Used = used;
            PtType = ptType;
            PropertyName = propertyName;
            Value = value;
            NextProperty = nextProperty;
            NodeId = nodeId;
        }

        protected PropertyBlock(PropertyBlock other)
        {
            Id = other.Id;
            NextProperty = other.NextProperty;
            PropertyName = other.PropertyName;
            NodeId = other.NodeId;
            PtType = other.PtType;
            Used = other.Used;
            Value = other.Value;
        }
    }

    public class NodePropertyBlock : PropertyBlock
    {
        public NodePropertyBlock(PropertyBlock other) : base(other)
        {
        }


        public NodePropertyBlock(int id, bool used, PropertyType ptType, int propertyName, byte[] value,
            int nextProperty,
            int nodeId) : base(id, used, ptType, propertyName, value, nextProperty, nodeId)
        {
        }
    }

    public class EdgePropertyBlock : PropertyBlock
    {
        public EdgePropertyBlock(PropertyBlock other) : base(other)
        {
        }

        public EdgePropertyBlock(int id, bool used, PropertyType ptType, int propertyName, byte[] value,
            int nextProperty,
            int nodeId) : base(id, used, ptType, propertyName, value, nextProperty, nodeId)
        {
        }
    }
}