using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;

namespace Outracks.UnoDevelop.CodeNinja
{
    public static class DocumentationCache
    {
        static string CacheFilePath
        {
            get
            {
                var dirPath = Path.Combine(Path.GetTempPath(), "RealtimeStudio");
                var filePath = Path.Combine(dirPath, "DocumentationCache.xml");
                Directory.CreateDirectory(dirPath);
                return filePath;
            }
        }

        static async Task<Dictionary<string, Entry>> LoadCache()
        {
            var documentation = new Dictionary<string, Entry>();

            using(var fileStream = new FileStream(CacheFilePath, FileMode.Open, FileAccess.Read))
            using (var reader = XmlReader.Create(fileStream, new XmlReaderSettings { Async = true }))
            while (await reader.ReadAsync())
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Name != "Element") continue;

                string key = null;
                string value = null;
                string url = null;
                var hasMoreContent = false;

                using (var subReader = reader.ReadSubtree())
                while (await subReader.ReadAsync())
                {
                    if (subReader.Name == "Key")
                        key = await subReader.ReadElementContentAsStringAsync();

                    if (subReader.Name == "Value")
                        value = await subReader.ReadElementContentAsStringAsync();

                    if (subReader.Name == "Url")
                        url = await subReader.ReadElementContentAsStringAsync();

                    if (subReader.Name == "HasMoreContent")
                        hasMoreContent = subReader.ReadElementContentAsBoolean();
                }

                if (key != null) documentation.Add(key, new Entry(value, url, hasMoreContent));
            }
            return documentation;
        }

        public class Entry
        {
            public readonly string Documentation;
            public readonly string Url;
            public readonly bool HasMoreContent;

            public Entry(string doc, string url, bool hasMoreContent)
            {
                Documentation = doc;
                Url = url;
                HasMoreContent = hasMoreContent;
            }
        }

        static Dictionary<string, Entry> _documentation;

        public static async void Update()
        {
            try
            {
                if (_documentation == null && File.Exists(CacheFilePath))
                    _documentation = await LoadCache();
            }
            catch (Exception) { /* NOTE: We should log this.. */ }
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync("https://beta.outracks.com/api/v1/docs/intellisense"); //TODO: Use updated URL if we start using this again
                if (!response.IsSuccessStatusCode) return;

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.Create(CacheFilePath))
                    await responseStream.CopyToAsync(fileStream);

                _documentation = await LoadCache();
            }
            catch (Exception) { /* NOTE: We should log this.. */ }
        }

        public static Entry GetEntry(string key)
        {
            if (_documentation == null) return null;

            if (_documentation.ContainsKey(key))
            {
                LastAccessedEntry = _documentation[key];
                return LastAccessedEntry;
            }
            return null;
        }

        public static void LaunchHelpForLastAccessedEntry()
        {
            if (LastAccessedEntry != null)
                System.Diagnostics.Process.Start(LastAccessedEntry.Url);
            else
                System.Diagnostics.Process.Start("http://beta.outracks.com/docs"); //TODO: Use updated URL if we start using this again
        }

        public static Entry LastAccessedEntry { get; private set; }

        public static string GetILID(Entity e)
        {
            if (e is DataType)
            {
                if (e is VoidType) return "";

                var dt = (e as DataType);

                if (dt is GenericParameterType)
                {
                    return dt.Name;
                }
                
                dt = dt.MasterDefinition;

                

                string s;

                if (dt.Parent != null)
                    s = GetILID(dt.Parent) + "." + dt.Name;
                else
                    s = dt.Name;

                if (dt.IsGenericDefinition)
                {
                    s += "`" + (dt as GenericType).GenericParameters.Length;
                }

                return s;                
            }
            else if (e is Member)
            {
                var m = (e as Member).MasterDefinition;

                if (m is Constructor)
                {
                    var ctor = m as Constructor;

                    var s = GetILID(m.DeclaringType) + "..ctor";

                    if (ctor.Parameters.Count() > 0)
                    {
                        s += "(";
                        foreach (var p in ctor.Parameters)
                        {
                            if (p != ctor.Parameters.First()) s += ",";
                            s += GetILID(p.Type);
                        }
                        s += ")";
                    }

                    return s;
                }
                else if (m is Method)
                {
                    var method = m as Method;

                    if (m.DeclaringType == null)
                        return "";

                    var s = GetILID(m.DeclaringType) + "." + m.Name;

                    if (method.Parameters.Any())
                    {
                        s += "(";
                        foreach (var p in method.Parameters)
                        {
                            if (p != method.Parameters.First()) s += ",";
                            s += GetILID(p.Type);
                        }
                        s += ")";
                    }

                    return s;
                }
            }

            return e.ToString();
        }

        public static Entry GetEntry(Entity e)
        {
            return GetEntry(GetILID(e));
        }
    }
}
