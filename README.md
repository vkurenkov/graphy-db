# GraphyDb

This repository contains a graph database **GraphyDb** written using .NET Framework (version 4.7).

All classes you need belong to namespace `GraphyDb`

## How to create Nodes, Relations and assign Properties

Open database connection:
```C#
DbEngine engine = new DbEngine();
```

`DbEngine` implements interface `IDisposable` to nicely close all open file streams. After finishing your work with DB you should call method `Dispose()` by yourself or use operator `using`


```C#
using (var engine = new DbEngine()) {
    ...
}
```


Create new node with label `"user"` (every node must have a label):
```C#
Node user1 = engine.AddNode("user");
```

Add property `"age"` (propety keys must be strings) with value `19` to `user1`:
```C#
user1["age"] = 19;
```

GraphyDb currently supports node and relation properties with one of the following types of values:
* `int`
* `float`(32 bit)
* `bool`
* `string` (max length 16 unicode characters)

Add one more node with label `"user"` with property `"age"` set to `20`:
```C#
Node user2 = engine.AddNode("user");
user2["age"] = 20;
```

Create relation with label `"knows"` from `user1` to `user2`:
```C#
var relation1 = engine.AddRelation(user1, user2, "knows")
```

You can also assign properties to relations:
```C#
relation1["some_property"] = "zzz";
```

To commit changes (write them on disk):
```C#
engine.SaveChanges();
```

## How to delete Nodes, Relations, properties

You can delete Nodes and Relations via uniform syntax:
```C#
engine.Delete(x); // x can be a Node or Relation
```

Or call method Delete directly on Node or Relation:
```C#
x.Delete() // x can be a Node or Relation
```

To delete property:
```C#
x.DeleteProperty("property_name") // x can be a Node or Relation
```

Do not forget to commit changes on disk:
```C#
engine.SaveChanges();
```

## How to make queries

5 classes were created for querying:

* `NodeDescription(string label, Dictionary<string, object> props)`
* `RelationDescription(string label, Dictionary<string, object> props)`

`NodeDescription` and `RelationDescription` encapsulate required search parameters for Nodes and Relations respectively: you can search them by *label* (place `null` if you don't want to specify label) and by *properties* (place empty dictionary if you don't want to specify properties). Also there are shortcuts `NodeDescription.Any()` and `RelationDescription.Any()`, which match all Nodes / Relations.

* `NodeSet`
* `RelationSet`

These classes represent results of a Query. `nodeSet.Nodes` is a `HashSet<Node>` with found Nodes. `relationSet.Relations` is a `HashSet<Relation>` with found Relations

* `Query(DbEngine engine)`

The main class to create a Query. The following example will help you understand its syntas:
```C#
var query = new Query(engine);

var A = query.Match(nodeDescriptionForA)
var X = query.From(relationDescriptionForX)
var B = query.Match(nodeDescriptionForB)
var Y = query.To(relationDescriptionForY)
var C = query.Match(nodeDescriptionForC)

query.Execute()
```

After `query.Execute()` objects `A`, `B` and `C` (which are of class `NodeSet`) will be populated with Nodes; `X`, `Y` (which are of class `RelationSet`) will be populated with Relations such that:

`A <-X- B -Y-> C` (`X` is a relation from `B` to `A` and `Y` is a relation from `B` to `C`) considering all node and relation descriptions. With this simple syntax you can create as complex queries as you want.

## To delete entire database
```C#
engine.DropDatabase();
```


