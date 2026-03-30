#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class EquipmentCsvImporter
{
    private const string SourceCsvPath = "Assets/_Project/Chart/Equipment.csv";
    private const string OutputFolderPath = "Assets/_Project/Data";
    private const string CatalogAssetPath = "Assets/_Project/Data/SOEquipmentCatalog.asset";

    [MenuItem("Tools/Equipment/Import Equipment CSV")]
    public static void ImportDefaultCsv()
    {
        try
        {
            Import(SourceCsvPath, OutputFolderPath, CatalogAssetPath);
        }
        catch (EquipmentCsvImportException exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    public static void Import(string csvAssetPath, string outputFolderPath, string catalogAssetPath)
    {
        if (!AssetDatabase.IsValidFolder(outputFolderPath))
        {
            throw new EquipmentCsvImportException($"Output folder does not exist: {outputFolderPath}");
        }

        TextAsset csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(NormalizeAssetPath(csvAssetPath));
        if (csvAsset == null)
        {
            throw new EquipmentCsvImportException($"CSV asset could not be found: {csvAssetPath}");
        }

        string[] lines = csvAsset.text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        int headerLineIndex = FindFirstDataLineIndex(lines);

        if (headerLineIndex < 0)
        {
            throw new EquipmentCsvImportException("CSV file does not contain a header row.");
        }

        Dictionary<string, int> headerMap = BuildHeaderMap(SplitCsvLine(lines[headerLineIndex]));
        List<SOEquipmentData> importedAssets = new List<SOEquipmentData>();
        Dictionary<string, int> assetNameUseCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        AssetDatabase.StartAssetEditing();

        try
        {
            for (int lineIndex = headerLineIndex + 1; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                IReadOnlyList<string> columns = SplitCsvLine(line);
                string equipmentId = ReadRequired(columns, headerMap, "EquipID", lineIndex + 1);
                string rawType = ReadRequired(columns, headerMap, "Type", lineIndex + 1);
                string displayName = ReadRequired(columns, headerMap, "Name", lineIndex + 1);
                string description = ReadRequired(columns, headerMap, "Description", lineIndex + 1);
                int cost = ReadInt(columns, headerMap, "Cost", lineIndex + 1);
                int price = ReadInt(columns, headerMap, "Price", lineIndex + 1);
                string spriteAssetPath = ReadOptional(columns, headerMap, "Sprite");

                if (!Enum.TryParse(rawType, true, out EquipmentType equipmentType))
                {
                    throw new EquipmentCsvImportException($"Row {lineIndex + 1}: unknown equipment type '{rawType}'.");
                }

                EquipmentStatModifiers statModifiers = new EquipmentStatModifiers(
                    ReadOptionalInt(columns, headerMap, "MaxHealthBonus", 0),
                    ReadOptionalFloat(columns, headerMap, "MaxSpecialGaugeBonus", 0f),
                    ReadOptionalInt(columns, headerMap, "BasicAttackDamageBonus", 0),
                    ReadOptionalInt(columns, headerMap, "SpecialAttackDamageBonus", 0),
                    ReadOptionalFloat(columns, headerMap, "MoveDelayScale", 1f),
                    ReadOptionalFloat(columns, headerMap, "BasicAttackDelayScale", 1f),
                    ReadOptionalFloat(columns, headerMap, "SpecialGaugeCostScale", 1f),
                    ReadOptionalInt(columns, headerMap, "SuccessfulHitCountForRecovery", 0),
                    ReadOptionalInt(columns, headerMap, "RecoveryHealthOnTrigger", 0),
                    ReadOptionalFloat(columns, headerMap, "RecoverySpecialGaugeOnTrigger", 0f));

                EquipmentCombatEffects combatEffects = new EquipmentCombatEffects(
                    ReadOptionalInt(columns, headerMap, "AdditionalHitFaceCountPerSide", 0),
                    ReadOptionalFloat(columns, headerMap, "AdditionalHitDamageMultiplier", 1f),
                    ReadOptionalBool(columns, headerMap, "EnableBossWeakPointSpawn", false));

                EquipmentInputEffects inputEffects = new EquipmentInputEffects(
                    ReadOptionalBool(columns, headerMap, "EnableHoldToMove", false),
                    ReadOptionalFloat(columns, headerMap, "HoldMoveRepeatIntervalScale", 1f),
                    ReadOptionalBool(columns, headerMap, "EnableInputAdjust", false),
                    ReadOptionalBool(columns, headerMap, "EnableHoldToFire", false));

                EquipmentVisualEffects visualEffects = new EquipmentVisualEffects(
                    ReadOptionalBool(columns, headerMap, "UsePlayerTint", false),
                    new Color(
                        ReadOptionalFloat(columns, headerMap, "PlayerTintR", 1f),
                        ReadOptionalFloat(columns, headerMap, "PlayerTintG", 1f),
                        ReadOptionalFloat(columns, headerMap, "PlayerTintB", 1f),
                        ReadOptionalFloat(columns, headerMap, "PlayerTintA", 1f)));

                string assetFileName = CreateUniqueAssetFileName(equipmentId, assetNameUseCount);
                string assetPath = $"{outputFolderPath}/{assetFileName}.asset";

                SOEquipmentData equipmentAsset = LoadOrCreateEquipmentAsset(assetPath);
                Sprite spriteIcon = LoadSpriteOrNull(spriteAssetPath);

                equipmentAsset.SetData(
                    equipmentId,
                    equipmentType,
                    displayName,
                    description,
                    cost,
                    price,
                    spriteAssetPath,
                    spriteIcon);
                equipmentAsset.SetEffects(statModifiers, combatEffects, inputEffects, visualEffects);
                EditorUtility.SetDirty(equipmentAsset);

                importedAssets.Add(equipmentAsset);
            }

            SOEquipmentCatalog catalogAsset = LoadOrCreateCatalogAsset(catalogAssetPath);
            catalogAsset.ReplaceAll(importedAssets);
            EditorUtility.SetDirty(catalogAsset);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Imported {importedAssets.Count} equipment assets from {csvAssetPath}");
    }

    private static string CreateUniqueAssetFileName(
        string equipmentId,
        Dictionary<string, int> assetNameUseCount)
    {
        string baseFileName = SanitizeAssetFileName(equipmentId);

        if (!assetNameUseCount.TryGetValue(baseFileName, out int useCount))
        {
            assetNameUseCount.Add(baseFileName, 1);
            return baseFileName;
        }

        useCount++;
        assetNameUseCount[baseFileName] = useCount;

        return $"{baseFileName}_{useCount:000}";
    }

    private static SOEquipmentData LoadOrCreateEquipmentAsset(string assetPath)
    {
        string normalizedAssetPath = NormalizeAssetPath(assetPath);
        SOEquipmentData existingAsset = AssetDatabase.LoadAssetAtPath<SOEquipmentData>(normalizedAssetPath);

        if (existingAsset != null)
        {
            return existingAsset;
        }

        SOEquipmentData newAsset = ScriptableObject.CreateInstance<SOEquipmentData>();
        AssetDatabase.CreateAsset(newAsset, normalizedAssetPath);

        return newAsset;
    }

    private static SOEquipmentCatalog LoadOrCreateCatalogAsset(string assetPath)
    {
        string normalizedAssetPath = NormalizeAssetPath(assetPath);
        SOEquipmentCatalog existingAsset = AssetDatabase.LoadAssetAtPath<SOEquipmentCatalog>(normalizedAssetPath);

        if (existingAsset != null)
        {
            return existingAsset;
        }

        SOEquipmentCatalog newAsset = ScriptableObject.CreateInstance<SOEquipmentCatalog>();
        AssetDatabase.CreateAsset(newAsset, normalizedAssetPath);

        return newAsset;
    }

    private static Sprite LoadSpriteOrNull(string spriteAssetPath)
    {
        if (string.IsNullOrWhiteSpace(spriteAssetPath))
        {
            return null;
        }

        string normalizedAssetPath = NormalizeAssetPath(spriteAssetPath);
        Sprite directSprite = AssetDatabase.LoadAssetAtPath<Sprite>(normalizedAssetPath);

        if (directSprite != null)
        {
            return directSprite;
        }

        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(normalizedAssetPath);
        for (int index = 0; index < assets.Length; index++)
        {
            if (assets[index] is Sprite sprite)
            {
                return sprite;
            }
        }

        Debug.LogWarning($"Sprite could not be loaded from path: {normalizedAssetPath}");
        return null;
    }

    private static string NormalizeAssetPath(string assetPath)
    {
        return assetPath.Replace('\\', '/');
    }

    private static int FindFirstDataLineIndex(IReadOnlyList<string> lines)
    {
        for (int index = 0; index < lines.Count; index++)
        {
            if (!string.IsNullOrWhiteSpace(lines[index]))
            {
                return index;
            }
        }

        return -1;
    }

    private static Dictionary<string, int> BuildHeaderMap(IReadOnlyList<string> headers)
    {
        Dictionary<string, int> headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < headers.Count; index++)
        {
            string headerName = headers[index].Trim();
            if (string.IsNullOrWhiteSpace(headerName))
            {
                continue;
            }

            if (!headerMap.TryAdd(headerName, index))
            {
                throw new EquipmentCsvImportException($"CSV header contains a duplicate column name: {headerName}");
            }
        }

        return headerMap;
    }

    private static string ReadRequired(
        IReadOnlyList<string> columns,
        IReadOnlyDictionary<string, int> headerMap,
        string headerName,
        int rowNumber)
    {
        int columnIndex = GetRequiredHeaderIndex(headerMap, headerName);

        if (columnIndex >= columns.Count)
        {
            throw new EquipmentCsvImportException($"Row {rowNumber}: missing value for '{headerName}'.");
        }

        string value = columns[columnIndex].Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new EquipmentCsvImportException($"Row {rowNumber}: '{headerName}' cannot be empty.");
        }

        return value;
    }

    private static string ReadOptional(
        IReadOnlyList<string> columns,
        IReadOnlyDictionary<string, int> headerMap,
        string headerName)
    {
        if (!headerMap.TryGetValue(headerName, out int columnIndex))
        {
            return string.Empty;
        }

        if (columnIndex >= columns.Count)
        {
            return string.Empty;
        }

        return columns[columnIndex].Trim();
    }

    private static int ReadInt(
        IReadOnlyList<string> columns,
        IReadOnlyDictionary<string, int> headerMap,
        string headerName,
        int rowNumber)
    {
        string rawValue = ReadRequired(columns, headerMap, headerName, rowNumber);

        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedValue))
        {
            throw new EquipmentCsvImportException($"Row {rowNumber}: '{headerName}' must be an integer, but was '{rawValue}'.");
        }

        return parsedValue;
    }

    private static int ReadOptionalInt(
        IReadOnlyList<string> columns,
        IReadOnlyDictionary<string, int> headerMap,
        string headerName,
        int defaultValue)
    {
        string rawValue = ReadOptional(columns, headerMap, headerName);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return defaultValue;
        }

        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedValue))
        {
            throw new EquipmentCsvImportException($"Optional integer column '{headerName}' has invalid value '{rawValue}'.");
        }

        return parsedValue;
    }

    private static float ReadOptionalFloat(
        IReadOnlyList<string> columns,
        IReadOnlyDictionary<string, int> headerMap,
        string headerName,
        float defaultValue)
    {
        string rawValue = ReadOptional(columns, headerMap, headerName);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return defaultValue;
        }

        if (!float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
        {
            throw new EquipmentCsvImportException($"Optional float column '{headerName}' has invalid value '{rawValue}'.");
        }

        return parsedValue;
    }

    private static bool ReadOptionalBool(
        IReadOnlyList<string> columns,
        IReadOnlyDictionary<string, int> headerMap,
        string headerName,
        bool defaultValue)
    {
        string rawValue = ReadOptional(columns, headerMap, headerName);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return defaultValue;
        }

        if (!bool.TryParse(rawValue, out bool parsedValue))
        {
            throw new EquipmentCsvImportException($"Optional bool column '{headerName}' has invalid value '{rawValue}'.");
        }

        return parsedValue;
    }

    private static int GetRequiredHeaderIndex(IReadOnlyDictionary<string, int> headerMap, string headerName)
    {
        if (!headerMap.TryGetValue(headerName, out int columnIndex))
        {
            throw new EquipmentCsvImportException($"CSV header is missing required column '{headerName}'.");
        }

        return columnIndex;
    }

    private static string SanitizeAssetFileName(string equipmentId)
    {
        if (string.IsNullOrWhiteSpace(equipmentId))
        {
            return "unnamed";
        }

        StringBuilder builder = new StringBuilder(equipmentId.Length);

        foreach (char character in equipmentId.Trim())
        {
            if ((character >= 'A' && character <= 'Z')
                || (character >= 'a' && character <= 'z')
                || (character >= '0' && character <= '9'))
            {
                builder.Append(character);
                continue;
            }

            if (character == '_' || character == '-')
            {
                builder.Append('_');
            }
        }

        return builder.Length == 0 ? "unnamed" : builder.ToString();
    }

    private static List<string> SplitCsvLine(string line)
    {
        List<string> columns = new List<string>();
        StringBuilder currentValue = new StringBuilder();
        bool isInsideQuotes = false;

        for (int characterIndex = 0; characterIndex < line.Length; characterIndex++)
        {
            char character = line[characterIndex];

            if (character == '"')
            {
                bool isEscapedQuote = isInsideQuotes
                    && characterIndex + 1 < line.Length
                    && line[characterIndex + 1] == '"';

                if (isEscapedQuote)
                {
                    currentValue.Append('"');
                    characterIndex++;
                    continue;
                }

                isInsideQuotes = !isInsideQuotes;
                continue;
            }

            if (character == ',' && !isInsideQuotes)
            {
                columns.Add(currentValue.ToString());
                currentValue.Clear();
                continue;
            }

            currentValue.Append(character);
        }

        columns.Add(currentValue.ToString());
        return columns;
    }

    private sealed class EquipmentCsvImportException : Exception
    {
        public EquipmentCsvImportException(string message) : base(message)
        {
        }
    }
}
#endif
