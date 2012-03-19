using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RegDiff.Core
{
    public class RegDiff
    {
        public IDictionary<string, Diff> GetDifferences(Stream baseStream, Stream changedStream)
        {
            IDictionary<string, string> baseReg = GetDictionary(baseStream);
            IDictionary<string, string> changedReg = GetDictionary(changedStream);
            IEnumerable<string> addedKeys = changedReg.Keys.Where(changedKey => !baseReg.Keys.Contains(changedKey));
            Dictionary<string, Diff> changedKeys = addedKeys.ToDictionary(k => k,
                                                                          k => new Diff
                                                                                   {
                                                                                       BaseVal = null,
                                                                                       ChangedVal = changedReg[k],
                                                                                       DiffType = DiffType.Added
                                                                                   });

            foreach (string key in baseReg.Select(basePair => basePair.Key))
            {
                if (changedReg.Keys.Contains(key))
                {
                    if (changedReg[key] != baseReg[key])
                    {
                        // change detected
                        changedKeys.Add(key,
                                        new Diff
                                            {
                                                BaseVal = baseReg[key],
                                                ChangedVal = changedReg[key],
                                                DiffType = DiffType.Changed
                                            });
                    }
                }
                else
                {
                    // removed
                    changedKeys.Add(key, new Diff
                                             {
                                                 BaseVal = baseReg[key],
                                                 ChangedVal = null,
                                                 DiffType = DiffType.Removed
                                             });
                }
            }

            return changedKeys;
        }

        public void WriteDifferences(string baseFilePath, string changedFilePath, string outputBasePath,
                                     string outputChangesPath)
        {
            IDictionary<string, Diff> diffs = GetDifferences(baseFilePath, changedFilePath);
            WriteDifferences(diffs, outputBasePath, outputChangesPath);
        }

        public void WriteDifferences(IDictionary<string, Diff> diffs, string outputBasePath, string outputChangesPath)
        {
            try
            {
                using (var baseStream = new StreamWriter(File.Open(outputBasePath, FileMode.Create, FileAccess.Write)))
                {
                    IOrderedEnumerable<KeyValuePair<string, Diff>> baseKeys = diffs.Where(
                        d => d.Value.DiffType == DiffType.Changed || d.Value.DiffType == DiffType.Removed).
                        OrderBy(d => d.Key);

                    foreach (var keyValuePair in baseKeys)
                    {
                        baseStream.WriteLine(keyValuePair.Key);
                        baseStream.WriteLine(keyValuePair.Value.BaseVal);
                        baseStream.WriteLine();
                    }
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Error writting file: " + outputBasePath, e);
            }

            try
            {
                using (
                    var changesStream = new StreamWriter(File.Open(outputChangesPath, FileMode.Create, FileAccess.Write))
                    )
                {
                    IOrderedEnumerable<KeyValuePair<string, Diff>> changeKeys = diffs.Where(
                        d => d.Value.DiffType == DiffType.Changed || d.Value.DiffType == DiffType.Added).OrderBy
                        (d => d.Key);

                    foreach (var keyValuePair in changeKeys)
                    {
                        changesStream.WriteLine(keyValuePair.Key);
                        changesStream.WriteLine(keyValuePair.Value.ChangedVal);
                        changesStream.WriteLine();
                    }
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Error writting file: " + outputChangesPath, e);
            }
        }

        public IDictionary<string, Diff> GetDifferences(string baseFilePath, string changedFilePath)
        {
            using (FileStream baseRegFile = File.OpenRead(baseFilePath))
            {
                using (FileStream changedRegFile = File.OpenRead(changedFilePath))
                {
                    return GetDifferences(baseRegFile, changedRegFile);
                }
            }
        }

        public IDictionary<string, string> GetDictionary(Stream regFile)
        {
            string line;
            string key = null;
            var value = new StringBuilder();
            var reader = new StreamReader(regFile);
            var dictionary = new Dictionary<string, string>();

            while ((line = reader.ReadLine()) != null)
            {
                if (line.FirstOrDefault() == ';' || line == string.Empty)
                {
                    // comments start with ; in .reg files
                    continue;
                }

                if (line.FirstOrDefault() == '[')
                {
                    if (key != null)
                    {
                        dictionary.Add(key, value.ToString());
                    }

                    key = line;
                    value.Clear();
                }
                else
                {
                    value.Append(line);
                    value.Append(Environment.NewLine);
                }
            }

            dictionary.Add(key, value.ToString());

            return dictionary;
        }
    }
}