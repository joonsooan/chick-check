using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public static class LocalizationCsvImporter
{
    private const string CsvDirectory = "Localization/CSV";

    [MenuItem("Chick Check/Localization/Import All CSV")]
    public static void ImportAll()
    {
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", CsvDirectory));
        if (!Directory.Exists(directory))
        {
            Debug.LogError($"CSV directory not found: {directory}");
            return;
        }

        foreach (string file in Directory.GetFiles(directory, "*.csv"))
        {
            string tableName = Path.GetFileNameWithoutExtension(file);
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection == null)
            {
                Debug.LogError($"No String Table Collection named '{tableName}' for {file}");
                continue;
            }

            HashSet<string> keysBefore = new HashSet<string>();
            foreach (SharedTableData.SharedTableEntry entry in collection.SharedData.Entries)
            {
                keysBefore.Add(entry.Key);
            }

            using (StreamReader reader = new StreamReader(file, Encoding.UTF8))
            {
                Csv.ImportInto(reader, collection, BuildColumns(), createUndo: false, reporter: null, removeMissingEntries: true);
            }

            foreach (SharedTableData.SharedTableEntry entry in collection.SharedData.Entries)
            {
                keysBefore.Remove(entry.Key);
            }

            foreach (string removedKey in keysBefore)
            {
                Debug.LogWarning($"[{tableName}] removed key not present in CSV: {removedKey}");
            }

            Debug.Log($"Imported {tableName} from {file} ({collection.SharedData.Entries.Count} keys, {keysBefore.Count} removed)");
        }

        AssetDatabase.SaveAssets();
    }

    private static List<CsvColumns> BuildColumns()
    {
        List<CsvColumns> columns = new List<CsvColumns>
        {
            new KeyIdColumns { IncludeSharedComments = false },
        };

        foreach (Locale locale in LocalizationEditorSettings.GetLocales())
        {
            columns.Add(new LocaleColumns
            {
                LocaleIdentifier = locale.Identifier,
                FieldName = locale.Identifier.Code,
                IncludeComments = false,
            });
        }

        return columns;
    }
}
