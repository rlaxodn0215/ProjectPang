// Copy into an isolated Unity project's Assets/Editor together with the tool and GSTU.
// Run with -executeMethod GoogleSpeadSheetReaderChecks.Run -gstuCheckStage missing|generate|import|reload|player.
// fixture.json is a read-only extraction of BingoRoulette.xlsx, not an import feature of the tool.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GoogleSheetsToUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class GoogleSpeadSheetReaderChecks
{
    [Serializable] private sealed class Fixture { public Sheet[] sheets = null; }
    [Serializable] private sealed class Sheet { public string title = null; public Cells[] rows = null; }
    [Serializable] private sealed class Cells { public string[] cells = null; }
    private static int assertions;
    private const string ReaderPath = "Assets/CheckReader.asset";

    public static void Run()
    {
        try
        {
            string stage = Argument("-gstuCheckStage");
            ScalarChecks();
            SheetChecks();
            var fixtures = JsonUtility.FromJson<Fixture>(File.ReadAllText("fixture.json"));
            if (stage == "missing")
            {
                var sound = fixtures.sheets.Single(s => s.title == "Sound");
                ExpectFailure(() => Parse(sound, 0), "Sound!C1");
                var prefab = Parse(fixtures.sheets.Single(s => s.title == "Prefab"), 1);
                Check(prefab.rows.Count == 1 && (string)prefab.rows[0].values["AssetPath"] == "Prefab/UI/Slot", "Prefab fixture");
                Check(!Directory.Exists("Assets/Generated") && !Directory.Exists("Assets/Data"), "Unknown enum causes no generated output/assets");
            }
            else if (stage == "generate")
            {
                var parsed = fixtures.sheets.Select((sheet, index) => Parse(sheet, index)).ToList();
                Check(parsed[0].rows.Count == 6 && parsed[1].rows.Count == 1, "Actual workbook row counts");
                Check((bool)parsed[0].rows[0].values["Loop"], "Workbook bool");
                Check((float)parsed[0].rows[3].values["Volume"] == 0.3f, "Workbook float");
                var reader = ScriptableObject.CreateInstance<GoogleSpeadSheetReader>();
                reader.spreadsheetId = "fixture";
                reader.generatedCodeFolder = "Assets/Generated";
                reader.dataFolder = "Assets/Data";
                reader.catalogFolder = "Assets/Catalogs";
                AssetDatabase.CreateAsset(reader, ReaderPath);
                GoogleSpeadSheetReaderPipeline.GenerateCode(reader, parsed);
                Check(File.Exists("Assets/Generated/SoundData.cs") && File.Exists("Assets/Generated/PrefabCatalog.cs"), "Generated code");
            }
            else if (stage == "import") ImportChecks(fixtures);
            else if (stage == "reload") ReloadChecks();
            else if (stage == "player") PlayerChecks();
            else throw new InvalidOperationException("Unknown stage: " + stage);
            File.WriteAllText("checks-" + stage + ".json", "{\"success\":true,\"assertions\":" + assertions + "}");
            Debug.Log("GSTU_CHECKS_OK " + stage + " " + assertions);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void ScalarChecks()
    {
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Check((float)GoogleSpeadSheetReaderPipeline.ParseValue("0.3", typeof(float)) == 0.3f, "Invariant float");
            Check((bool)GoogleSpeadSheetReaderPipeline.ParseValue("TRUE", typeof(bool)), "Sheets bool");
            Check((int)GoogleSpeadSheetReaderPipeline.ParseValue("-12", typeof(int)) == -12, "Integer");
            Check((string)GoogleSpeadSheetReaderPipeline.ParseValue("  text  ", typeof(string)) == "  text  ", "Preserve strings");
            var integers = (List<int>)GoogleSpeadSheetReaderPipeline.ParseValue("[1, -2, 3]", typeof(List<int>));
            Check(integers.SequenceEqual(new[] { 1, -2, 3 }), "JSON int list");
            var strings = (List<string>)GoogleSpeadSheetReaderPipeline.ParseValue("[\"A,B\",\"\",\"\\uD55C\\uAE00\",\"quote\\\"\\n\"]", typeof(List<string>));
            Check(strings[0] == "A,B" && strings[1] == "" && strings[2] == "한글" && strings[3] == "quote\"\n", "JSON string escapes");
            var longs = (List<long>)GoogleSpeadSheetReaderPipeline.ParseValue("[9223372036854775807]", typeof(List<long>));
            Check(longs[0] == long.MaxValue, "JSON integer precision");
            Check(((List<bool>)GoogleSpeadSheetReaderPipeline.ParseValue("[true,false]", typeof(List<bool>))).Count == 2, "JSON bool list");
            Check(((List<float>)GoogleSpeadSheetReaderPipeline.ParseValue("[1e-2,2.5]", typeof(List<float>)))[0] == 0.01f, "JSON float list");
            Check(((List<double>)GoogleSpeadSheetReaderPipeline.ParseValue("[]", typeof(List<double>))).Count == 0, "Empty JSON list");
            foreach (string invalid in new[] { "", "[1,]", "[null]", "[01]", "[[1]]", "[{}]", "[true]", "[\"1\"]", "[1]x" })
                ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ParseValue(invalid, typeof(List<int>)), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ParseValue("[TRUE]", typeof(List<bool>)), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ParseValue("[\"bad\\q\"]", typeof(List<string>)), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ParseValue("NaN", typeof(float)), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ParseValue("Infinity", typeof(double)), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ParseValue("", typeof(bool)), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ParseValue("2147483648", typeof(int)), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ResolveType("ThisEnumDoesNotExist"), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ResolveType("List<List<int>>"), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ValidateAssetFolder("Assets/../Outside"), null);
            ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ValidateAssetFolder("C:/Outside"), null);
            Check(GoogleSpeadSheetReaderPipeline.ValidateAssetFolder("Assets/Custom/Data") == "Assets/Custom/Data", "Custom Assets path");
            Check(GoogleSpeadSheetReaderPipeline.ColumnLetters(27) == "AA" && GoogleSpeadSheetReaderPipeline.ColumnLetters(703) == "AAA", "Columns beyond Z");
            var enumType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
            {
                try { return a.GetTypes(); } catch { return new Type[0]; }
            }).SingleOrDefault(t => t.FullName == "GstuCheckFixture.ESoundType");
            if (enumType != null)
            {
                Check(GoogleSpeadSheetReaderPipeline.ParseValue("BGM", enumType).ToString() == "BGM", "Existing enum");
                ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ParseValue("0", enumType), null);
                ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ParseValue("bgm", enumType), null);
                var listType = typeof(List<>).MakeGenericType(enumType);
                Check(((System.Collections.IList)GoogleSpeadSheetReaderPipeline.ParseValue("[\"BGM\",\"SFX\"]", listType)).Count == 2, "JSON enum list");
            }
        }
        finally { CultureInfo.CurrentCulture = old; }
    }

    private static void SheetChecks()
    {
        var rows = new List<List<string>> { new List<string> { "Name", "Key(int)", "Values(List<string>)" } };
        for (int i = 1; i <= 150; i++) rows.Add(new List<string> { "Row" + i, i.ToString(CultureInfo.InvariantCulture), "[\"A,B\"]" });
        var parsed = ParseRows("Large", rows, 9);
        Check(parsed.rows.Count == 150 && parsed.rows[149].key == 150, "Over 100 rows");
        rows.Add(new List<string> { "", "", "" });
        Check(ParseRows("Large", rows, 9).rows.Count == 150, "Skip entirely empty rows");
        rows[2][1] = "1";
        ExpectFailure(() => ParseRows("Large", rows, 9), "Large!B3");
        rows[2][1] = "2";
        rows[2][0] = "Row1";
        ExpectFailure(() => ParseRows("Large", rows, 9), "Large!A3");
        rows[2][0] = "Row2";
        rows[2][2] = "";
        ExpectFailure(() => ParseRows("Large", rows, 9), "Large!C3");
        rows[2][2] = "[]";
        rows[0][1] = "Key(long)";
        ExpectFailure(() => ParseRows("Large", rows, 9), "Key(int)");
        rows[0][1] = "Key(int)";
        rows[0][2] = "class(string)";
        ExpectFailure(() => ParseRows("Large", rows, 9), "Large!C1");
        ExpectFailure(() => ParseRows("Bad Sheet", rows, 9), "worksheet");
    }

    private static void ImportChecks(Fixture fixtures)
    {
        var reader = AssetDatabase.LoadAssetAtPath<GoogleSpeadSheetReader>(ReaderPath);
        var inspector = Editor.CreateEditor(reader) as GoogleSpeadSheetReaderEditor;
        Check(inspector != null, "Custom Inspector registered");
        var visualTree = inspector.CreateInspectorGUI();
        Check(visualTree.Query<UnityEngine.UIElements.Button>().ToList().Count == 4, "Four Inspector actions");
        Check(visualTree.Query<UnityEditor.UIElements.PropertyField>().ToList().Count == 6, "Editable Inspector settings and output paths");
        UnityEngine.Object.DestroyImmediate(inspector);
        var parsed = fixtures.sheets.Select((sheet, index) => Parse(sheet, index)).ToList();
        GoogleSpeadSheetReaderPipeline.ApplyImport(reader, "fixture", parsed);
        Check(reader.Imports.Count == 2 && reader.Imports[0].rows.Count == 6 && reader.Imports[1].rows.Count == 1, "Initial import counts");
        var soundRecord = reader.Imports.Single(r => r.sheetId == 0);
        string bgmGuid = soundRecord.rows.Single(r => r.key == 1).guid;
        var catalog = soundRecord.catalog;
        var original = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(bgmGuid));
        var holder = ScriptableObject.CreateInstance<GoogleSpeadSheetReader>();
        var serializedHolder = new SerializedObject(holder);
        var references = serializedHolder.FindProperty("imports");
        references.arraySize = 1;
        references.GetArrayElementAtIndex(0).FindPropertyRelative("catalog").objectReferenceValue = original;
        serializedHolder.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.CreateAsset(holder, "Assets/UnrelatedReader.asset");
        var unrelated = ScriptableObject.CreateInstance(original.GetType());
        AssetDatabase.CreateAsset(unrelated, "Assets/Data/Sound/999.asset");
        string unrelatedGuid = AssetDatabase.AssetPathToGUID("Assets/Data/Sound/999.asset");

        var changed = fixtures.sheets.Select((sheet, index) => Parse(sheet, index)).ToList();
        changed[0].rows[0].name = "BGM_Renamed";
        changed[0].rows[0].values["Name"] = "BGM_Renamed";
        changed[0].rows[0].values["Volume"] = 0.25f;
        changed[0].rows.RemoveAt(5);
        var addition = new GoogleSpeadSheetReaderPipeline.Row
        { key = 7, name = "NewSound", values = new Dictionary<string, object>(changed[0].rows[1].values) };
        addition.values["Key"] = 7;
        addition.values["Name"] = "NewSound";
        changed[0].rows.Add(addition);
        GoogleSpeadSheetReaderPipeline.ApplyImport(reader, "fixture", changed);
        soundRecord = reader.Imports.Single(r => r.sheetId == 0);
        Check(soundRecord.rows.Single(r => r.key == 1).guid == bgmGuid, "Keep GUID on rename/update");
        Check(original.name == "BGM_Renamed" && (float)original.GetType().GetField("Volume").GetValue(original) == 0.25f, "Update existing instance");
        Check(soundRecord.catalog == catalog, "Keep Catalog reference");
        Check(!File.Exists("Assets/Data/Sound/6.asset") && AssetDatabase.LoadMainAssetAtPath("Assets/Data/Sound/6.asset") == null, "Delete removed managed row");
        Check(holder.Imports[0].catalog == original, "Keep external SO reference");
        Check(AssetDatabase.AssetPathToGUID("Assets/Data/Sound/999.asset") == unrelatedGuid, "Never delete unowned row");
        var entries = (System.Collections.IEnumerable)catalog.GetType().GetProperty("Entries").GetValue(catalog);
        var entryKeys = entries.Cast<ScriptableObject>().Select(e => (int)e.GetType().GetField("Key").GetValue(e)).ToArray();
        Check(entryKeys.SequenceEqual(new[] { 1, 2, 3, 4, 5, 7 }), "Catalog preserves row order");
        var args = new object[] { 1, null };
        Check((bool)catalog.GetType().GetMethod("TryGetByKey").Invoke(catalog, args) && (ScriptableObject)args[1] == original, "Catalog key lookup");
        args = new object[] { 6, null };
        Check(!(bool)catalog.GetType().GetMethod("TryGetByKey").Invoke(catalog, args) && args[1] == null, "Catalog missing key");

        var before = Snapshot();
        var mismatched = fixtures.sheets.Select((sheet, index) => Parse(sheet, index)).ToList();
        mismatched[1].signature += "|Extra:int";
        ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ApplyImport(reader, "fixture", mismatched), "헤더 변경");
        Check(before == Snapshot(), "Schema failure does not mutate any data assets");
        ExpectFailure(() => GoogleSpeadSheetReaderPipeline.ApplyImport(reader, "different-source", changed), "다른 소스");
        Check(before == Snapshot(), "Source ownership failure does not mutate data assets");
        string metaBefore = File.ReadAllText("Assets/Generated/SoundData.cs.meta");
        GoogleSpeadSheetReaderPipeline.GenerateCode(reader, changed);
        Check(metaBefore == File.ReadAllText("Assets/Generated/SoundData.cs.meta"), "Code generation preserves meta");

        reader.dataFolder = "Assets/AlternateData";
        reader.catalogFolder = "Assets/AlternateCatalogs";
        GoogleSpeadSheetReaderPipeline.ApplyImport(reader, "fixture", changed);
        Check(reader.Imports.Count == 4, "Separate ownership at new paths");
        Check(File.Exists("Assets/Data/Sound/1.asset") && File.Exists("Assets/AlternateData/Sound/1.asset"), "Path change leaves old assets intact");
        reader.dataFolder = "Assets/Data";
        reader.catalogFolder = "Assets/Catalogs";
        GoogleSpeadSheetReaderPipeline.ApplyImport(reader, "fixture", changed);
        Check(reader.Imports.Single(r => r.sheetId == 0 && r.dataPath == "Assets/Data/Sound").rows.Single(r => r.key == 1).guid == bgmGuid, "Switch back keeps original GUID");
        File.WriteAllText("reload-guid.txt", bgmGuid);
        AssetDatabase.SaveAssetIfDirty(reader);
    }

    private static void ReloadChecks()
    {
        var reader = AssetDatabase.LoadAssetAtPath<GoogleSpeadSheetReader>(ReaderPath);
        string guid = File.ReadAllText("reload-guid.txt");
        var record = reader.Imports.Single(r => r.sheetId == 0 && r.dataPath == "Assets/Data/Sound");
        Check(record.rows.Single(r => r.key == 1).guid == guid, "Ownership survives editor restart");
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
        Check(asset != null && asset.name == "BGM_Renamed", "Data SO survives editor restart");
        var holder = AssetDatabase.LoadAssetAtPath<GoogleSpeadSheetReader>("Assets/UnrelatedReader.asset");
        Check(holder.Imports[0].catalog == asset, "External SO reference survives editor restart");
        var arguments = new object[] { 1, null };
        Check((bool)record.catalog.GetType().GetMethod("TryGetByKey").Invoke(record.catalog, arguments) && (ScriptableObject)arguments[1] == asset, "Catalog reference survives editor restart");
    }

    private static void PlayerChecks()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/CheckScene.unity");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/CheckScene.unity" },
            locationPathName = "Build/Check.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        Check(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded, "Player build excludes editor implementation");
    }

    private static string Snapshot() => string.Join("\n", new[] { "Assets/Data", "Assets/Catalogs" }
        .Where(Directory.Exists).SelectMany(p => Directory.GetFiles(p, "*", SearchOption.AllDirectories)).OrderBy(p => p)
        .Select(p => p + ":" + Convert.ToBase64String(File.ReadAllBytes(p))));
    private static GoogleSpeadSheetReaderPipeline.ParsedSheet Parse(Sheet sheet, int id) =>
        ParseRows(sheet.title, sheet.rows.Select(r => r.cells.ToList()).ToList(), id);
    private static GoogleSpeadSheetReaderPipeline.ParsedSheet ParseRows(string title, List<List<string>> rows, int id)
    {
        var data = new ValueRange(rows) { range = "'" + title + "'!A1:AZ1000" };
        return GoogleSpeadSheetReaderPipeline.ParseSheet(new GoogleSpeadSheetReader.Worksheet
        { title = title, sheetId = id, rowCount = 1000, columnCount = 52 }, new GstuSpreadSheet(new GSTU_SpreadsheetResponce(data), "A", 1));
    }
    private static string Argument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : "";
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("CHECK FAILED: " + message);
        assertions++;
    }
    private static void ExpectFailure(Action action, string expected)
    {
        try { action(); }
        catch (Exception exception)
        {
            Check(expected == null || exception.Message.Contains(expected), "Expected error containing " + expected + ", got " + exception.Message);
            return;
        }
        throw new InvalidOperationException("CHECK FAILED: Expected rejection: " + expected);
    }
}
#endif
