# GraphyDb

This repository contains a graph database **GraphyDb** written using .NET Framework (version 4.7).

All classes you need belong to namespace `GraphyDb`

## How to create Nodes, Relations and assign Properties

Open database connection:
```C#
DbEngine engine = new DbEngine();
```

Create new node with label `"user"` (every node must have a label):
```C#
Node user1 = engine.AddNode("user");
```

Add property `"age"` with value `19` to `user1`:
```C#
user1["age"] = 19
```

GraphyDb currently supports node and relation properties with one of the following types of values:
* `int`
* `float`(32 bit)
* `bool`
* `string` (max length 16 unicode characters)

Add one more node with label `"user"` with property `"age"` set to `20`:
```C#
Node user2 = engine.AddNode("user");
user2["age"] = 20
```

Create relation with label `"knows"` from `user1` to `user2`:
```C#
var relation1 = engine.AddRelation(user1, user2, "knows")
```

You can also assign properties to relations:
```C#
relation1["some_property"] = "zzz"
```

To commit changes (write them on disk):
```C#
engine.SaveChanges();
```



