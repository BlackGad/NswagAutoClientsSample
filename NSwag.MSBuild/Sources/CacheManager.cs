using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NSwag.MSBuild.Extensions;

namespace NSwag.MSBuild.Sources
{
    public class CacheManager : IDisposable
    {
        private readonly string _intermediatePath;
        private readonly TaskLoggingHelper _log;

        private Dictionary<int, CacheRecord> _cacheTable;

        #region Constructors

        public CacheManager(string intermediatePath, TaskLoggingHelper log)
        {
            if (intermediatePath == null) throw new ArgumentNullException(nameof(intermediatePath));
            _intermediatePath = intermediatePath;
            _log = log;
        }

        #endregion

        #region Properties

        private Dictionary<int, CacheRecord> CacheTable
        {
            get
            {
                if (_cacheTable != null) return _cacheTable;
                var cachePath = GetCachePath();
                if (File.Exists(cachePath))
                {
                    try
                    {
                        using (var stream = File.OpenRead(cachePath))
                        {
                            var formatter = new BinaryFormatter();
                            var cacheArray = (CacheRecord[])formatter.Deserialize(stream);
                            _cacheTable = cacheArray.ToDictionary(r => r.Key, r => r);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.LogWarning($"  - Cache index '{cachePath}' exist but failed to load. Details: {e.Message}");
                    }
                }

                return _cacheTable ?? (_cacheTable = new Dictionary<int, CacheRecord>());
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                var cachePath = GetCachePath();
                Path.GetDirectoryName(cachePath).EnsureDirectoryExist();

                using (var stream = File.OpenWrite(cachePath))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, _cacheTable.Values.ToArray());
                }
            }
            catch (Exception)
            {
                //Cache failed to save
            }
        }

        #endregion

        #region Members

        public void Cache(ITaskItem item, NSwagClient[] declarations)
        {
            if (item == null) return;

            var filePath = item.GetMetadata("FullPath");
            try
            {
                var hashCode = filePath.GetHashCode();
                _log.LogMessage(MessageImportance.Low, $"  - Adding {filePath} to cache");
                if (!File.Exists(filePath)) return;
                var actualDate = File.GetLastWriteTimeUtc(filePath).ToFileTime();
                var cacheTable = CacheTable;
                if (!cacheTable.ContainsKey(hashCode)) cacheTable.Add(hashCode, new CacheRecord());

                cacheTable[hashCode].Timestamp = actualDate;
                cacheTable[hashCode].Key = hashCode;
                cacheTable[hashCode].Declarations = declarations;
                _log.LogMessage(MessageImportance.Normal, $"  - {filePath} added to cache");
            }
            catch (Exception e)
            {
                _log.LogWarning($"  - {filePath} was not added to cache. Details: {e.Message}");
            }
        }

        public NSwagClient[] GetCached(ITaskItem item)
        {
            if (item == null) return null;

            try
            {
                var filePath = item.GetMetadata("FullPath");
                var hashCode = filePath.GetHashCode();
                if (!CacheTable.ContainsKey(hashCode)) return null;
                if (!File.Exists(filePath)) return null;

                var actualDate = File.GetLastWriteTimeUtc(filePath).ToFileTime();
                if (CacheTable[hashCode].Timestamp != actualDate) return null;
                return CacheTable[hashCode].Declarations;
            }
            catch (Exception e)
            {
                //Something goes wrong
                _log.LogWarning($"  - Cache extract from {item.ItemSpec} failed. Details: {e.Message}");
                return null;
            }
        }

        public string GetCachePath()
        {
            return Path.Combine(_intermediatePath, $"_{nameof(CacheManager)}.cache");
        }

        #endregion

        #region Nested type: CacheRecord

        [Serializable]
        class CacheRecord
        {
            #region Properties

            public NSwagClient[] Declarations { get; set; }
            public int Key { get; set; }
            public long Timestamp { get; set; }

            #endregion
        }

        #endregion
    }
}