using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphyDb.IO
{
    internal static class DbFetcher
    {

        internal static readonly Dictionary<string, FileStream>
            ReadOnlyFileStreamDictionary = new Dictionary<string, FileStream>();

        public static void initializeFetcher()
        {
            //TODO 
        }

        public static HashSet<NodeBlock> SelectNodesByLabelAndProperty(int propertyKey, int propertyValue, int labelId)
        {
            var outputNodes = new HashSet<NodeBlock>();
            //TODO sequential scan of all properties
            return outputNodes;
        }

        public static HashSet<NodeBlock> SelectNodesByLabel(int labelId)
        {
            var outputNodes = new HashSet<NodeBlock>();
            
            return outputNodes;
        }
    }
}