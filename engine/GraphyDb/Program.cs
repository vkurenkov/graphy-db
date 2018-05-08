using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphyDb
{
    public class Program
    {
        private static void Main(string[] args)
        {
            using (var engine = new DbEngine())
            {
//                engine.DropDatabase();

                Node user1 = engine.AddNode("user");
                user1["age"] = 19;

                Node user2 = engine.AddNode("user");
                user2["age"] = 20;

                var relation1 = engine.AddRelation(user1, user2, "knows");
                relation1["some_property"] = "zzz";

                engine.SaveChanges();

                var query = new Query(engine);

                var firstNodeDescription = new NodeDescription(
                    "user",
                    new Dictionary<string, object>
                    {
                        {"age", 19}
                    }
                );

                var relationDescription = new RelationDescription(
                    "knows",
                    new Dictionary<string, object>
                    {
                        {"some_property", "zzz"}
                    }
                );


                var a = query.Match(firstNodeDescription);
                var x = query.To(relationDescription);
                var b = query.Match(NodeDescription.Any());

                query.Execute();

                Console.WriteLine(a.Nodes.First()["age"]);
                Console.WriteLine(x.Relations.First()["some_property"]);
                Console.WriteLine(b.Nodes.First()["age"]);

                Console.ReadLine();

                engine.DropDatabase();
            }
            
        }
        
    }
}