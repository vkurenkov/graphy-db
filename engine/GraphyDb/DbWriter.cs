using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System;
using System.Diagnostics;

namespace GraphyDb.IO
{
    public static class DbWriter
    {
        private static TraceSource traceSource = new TraceSource("TraceGraphyDb");
        static string nodePath =  "node.storage.db";
        static string edgePath = "edge.storage.db";
        static string labelPath = "labe.storage.db";
        static string propertyPath = "property.storage.db";
        static string propertyNamePath = "property_name.storage.db";
        static string stringPath = "string.storage.db";

        /// <summary>
        /// Create storage files if missing
        /// </summary>
        public static void InitializeFiles()
        {
            string dbPath = ConfigurationManager.AppSettings["dbPath"];
            List<string> dbFilePaths = new List<string> {nodePath, edgePath, labelPath, propertyPath,
                                              propertyNamePath, stringPath};
            try
            {
                foreach (string filePath in dbFilePaths)
                {
                    if (!File.Exists(Path.Combine(dbPath, filePath))) File.Create(Path.Combine(dbPath, filePath));
                }
            }
            catch (Exception ex) {
                traceSource.TraceEvent(TraceEventType.Error, 1,
                string.Format("File Init Falied: {0}", ex));
            }
        
        }

    }
}


