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
        public int FirstPropertyId;
        public int LabelId;

        public NodeBlock(bool used, int nodeId, int firstInRelationId, int firstOutRelationId, int firstPropertyId,
            int labelId)
        {
            Used = used;
            NodeId = nodeId;
            FirstInRelationId = firstInRelationId;
            FirstPropertyId = firstPropertyId;
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

    public class RelationBlock : IEquatable<RelationBlock>
    {
        public bool Equals(RelationBlock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RelationId == other.RelationId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RelationBlock) obj);
        }

        public override int GetHashCode()
        {
            return RelationId;
        }

        public static bool operator ==(RelationBlock left, RelationBlock right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RelationBlock left, RelationBlock right)
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
        public int FirstPropertyId;
        public int LabelId;
        public int RelationId;

        public RelationBlock()
        {
        }

        public RelationBlock(bool used, int firstNode, int secondNode, int firstNodePreviousRelation,
            int firstNodeNextRelation, int secondNodePreviousRelation, int secondNodeNextRelation, int firstPropertyId,
            int labelId, int relationId)
        {
            Used = used;
            FirstNode = firstNode;
            SecondNode = secondNode;
            FirstNodePreviousRelation = firstNodePreviousRelation;
            FirstNodeNextRelation = firstNodeNextRelation;
            SecondNodePreviousRelation = secondNodePreviousRelation;
            SecondNodeNextRelation = secondNodeNextRelation;
            FirstPropertyId = firstPropertyId;
            LabelId = labelId;
            RelationId = relationId;
        }
    }

    public abstract class PropertyBlock
    {
        public int PropertyId;
        public bool Used;
        public PropertyType PropertyType;
        public int PropertyNameId;
        public byte[] Value;
        public int NextProperty;
        public int NodeId;

        protected PropertyBlock(int propertyId, bool used, PropertyType propertyType, int propertyNameId, byte[] value,
            int nextProperty, int nodeId)
        {
            PropertyId = propertyId;
            Used = used;
            PropertyType = propertyType;
            PropertyNameId = propertyNameId;
            Value = value;
            NextProperty = nextProperty;
            NodeId = nodeId;
        }

        protected PropertyBlock(PropertyBlock other)
        {
            PropertyId = other.PropertyId;
            NextProperty = other.NextProperty;
            PropertyNameId = other.PropertyNameId;
            NodeId = other.NodeId;
            PropertyType = other.PropertyType;
            Used = other.Used;
            Value = other.Value;
        }
    }

    public class NodePropertyBlock : PropertyBlock
    {
        public NodePropertyBlock(PropertyBlock other) : base(other)
        {
        }


        public NodePropertyBlock(int propertyId, bool used, PropertyType propertyType, int propertyNameId, byte[] value,
            int nextProperty,
            int nodeId) : base(propertyId, used, propertyType, propertyNameId, value, nextProperty, nodeId)
        {
        }
    }

    public class RelationPropertyBlock : PropertyBlock
    {
        public RelationPropertyBlock(PropertyBlock other) : base(other)
        {
        }

        public RelationPropertyBlock(int propertyId, bool used, PropertyType propertyType, int propertyNameId,
            byte[] value,
            int nextProperty,
            int nodeId) : base(propertyId, used, propertyType, propertyNameId, value, nextProperty, nodeId)
        {
        }
    }
}