using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using SimpleTweaksPlugin;
using SimpleTweaksPlugin.Debugging;
using ULD;

namespace STK.Test.ULDEditor; 

public class UldTester : IDisposable {

    private ULD.Uld currentFile;
    private string currentFileName = "No File Open";
    private bool currentFileFromGame = false;
    
    private Action activeModal;
    private bool isFileDirty = false;

    private string lastError = string.Empty;
    
    private BufferWriter simple = null;


    private byte[] CompareFile = null;
    
    public void DrawWindow() {
        if (ImGui.Begin($"{(isFileDirty?"*":"")}ULD Tester - {(currentFileFromGame?"[GameFile] ":"")}{currentFileName}###uldTester_mainWindow", ImGuiWindowFlags.MenuBar)) {

            if (CompareFile == null) {
                CompareFile = File.ReadAllBytes(@"C:\Users\Cara\Desktop\New folder\New Folder\ui\uld\notification.uld");
            }
            
            
            if (ImGui.IsWindowAppearing() && activeModal == null && currentFile == null) {
                activeModal = FullTest;
            }
            DrawInterface();
        }

        ImGui.End();
    }
    public void DrawInterface() {
        DrawMenu();

        if (!string.IsNullOrEmpty(lastError)) {
            ImGui.TextColored(ImGuiColors.DalamudRed, lastError);
            if (ImGui.SmallButton("ok")) lastError = null;
            ImGui.Separator();
        }
        
        if (activeModal != null) {
            activeModal();
            return;
        }
        
        ImGui.Text("Hello World");
        
        
    }
    
    private string inputLoadFromGameUldName = "";
    private void LoadFromGame() {

        if (isFileDirty) {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.TextWrapped("You have unsaved changes. Loading a new file will discard any unsaved changed.");
            ImGui.PopStyleColor();
        }
        
        ImGui.Text("Enter ULD Name");

        var loadFromList = false;
        
        ImGui.InputText("###loadFromGame_inputText", ref inputLoadFromGameUldName, 100);
        var buttonSize = ImGui.GetItemRectSize();
        if (ImGui.BeginChild("fileSelector", new Vector2(buttonSize.X, buttonSize.Y * 8), true)) {
            foreach (var f in GameUld.List) {
                if (ImGui.Selectable($"{f}", f == inputLoadFromGameUldName)) {
                    if (inputLoadFromGameUldName == f) loadFromList = true;
                    inputLoadFromGameUldName = f;
                    
                }
            }
        }
        ImGui.EndChild();
        
        
        

        var name = inputLoadFromGameUldName;
        if (!name.EndsWith(".uld")) name = name + ".uld";
        if (!name.Contains("/")) name = "ui/uld/" + name;
        
        if (name.Length > 0 && Service.Data.FileExists($"{name}")) {
            if (loadFromList || ImGui.Button("Load ULD", buttonSize)) {
                try {
                    ULD.Logging.LogMessage -= PluginLog.Log;
                    ULD.Logging.LogMessage += PluginLog.Log;
                    
                    
                    
                    
                    var uld = Service.Data.GameData.GetFile(name);
                    
                    if (uld != null) {
                        CompareFile = uld.Data;
                        currentFile = new ULD.Uld(new BufferReader(uld.Data));
                        simple = currentFile.Encode();
                        currentFileName = $"{name}";
                        currentFileFromGame = true;
                        activeModal = DebugCompareFile;
                    }
                } catch (Exception ex) {
                    currentFile = null;
                    CompareFile = null;
                    currentFileName = "Failed to Load";
                    currentFileFromGame = false;
                    activeModal = null;
                    lastError = ex.ToString();
                }
            }
            
        } else {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.1f, 0.1f, 0.1f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.1f, 0.1f, 0.1f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.1f, 0.1f, 0.1f, 1f));
            ImGui.Button($"Does Not Exist", buttonSize);
            ImGui.PopStyleColor(4);
        }
        
        if (ImGui.Button("Cancel", buttonSize)) {
            activeModal = null;
        }
    }

    private void LoadFile() {
        
        if (isFileDirty) {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.TextWrapped("You have unsaved changes. Loading a new file will discard any unsaved changed.");
            ImGui.PopStyleColor();
        }
        
        
        if (ImGui.Button("Load From File")) {
            activeModal = null;
        }
    }

    private static int compareInspectOffset = 0;
    private static int compareHoverIndex = -1;
    private static int hoveredIndex = -1;
    
    
    private static void DrawCompareSide(byte[] data, byte[] compareData, bool isLeft = false) {
            ImGui.PushFont(UiBuilder.MonoFont);

            var byteSpacing = 3f;
            var lineSpacing = 2f;
            
            ImGui.GetFont().FallbackChar = '.';

            var charSize = ImGui.CalcTextSize("W");
            var byteSize = ImGui.CalcTextSize("00");
            var lineCount = (data.Length / 16) + 1;
            var size = new Vector2(
                (charSize.X + byteSize.X + byteSpacing) * 16 + byteSpacing * 13,
                (charSize.Y + lineSpacing) * lineCount
                );
            
            var dl = ImGui.GetWindowDrawList();

            var charsPos = ImGui.GetCursorScreenPos();
            var bytesPos = ImGui.GetCursorScreenPos();

            if (isLeft) {
                bytesPos.X += (charSize.X * 16) + byteSpacing * 10;
            } else {
                charsPos.X += ((byteSize.X + byteSpacing) * 16) + byteSpacing * 10;
            }

            var mousePos = ImGui.GetMousePos();
            for (var i = 0; i < data.Length; i++) {
                
                var c = (char)data[i];
                if (Char.IsControl(c)) c = '.';

                var rowCol = new Vector2(i % 16, MathF.Floor(i / 16f));
                
                var yPos = charsPos.Y + (rowCol.Y * (charSize.Y + lineSpacing));

                var color = 0xFFFFFFFF;

                var charPosX = charsPos.X + charSize.X * rowCol.X;
                var bytePosX = bytesPos.X + (byteSize.X + byteSpacing) * rowCol.X + (rowCol.X >= 8 ? (byteSpacing * 3) : 0);
                
                var thisCharPos = new Vector2(charPosX, yPos);
                var thisBytePos = new Vector2(bytePosX, yPos);
                
                
                if (i == hoveredIndex) {
                    color = 0xFF00FFFF;
                    dl.AddRect(thisCharPos, thisCharPos + charSize, 0xFF00FFFF);
                    dl.AddRect(thisBytePos, thisBytePos + byteSize, 0xFF00FFFF);

                }
                if (i == compareInspectOffset) {
                    color = 0xFF00FF00;
                    dl.AddRect(thisCharPos - Vector2.One, thisCharPos + charSize + Vector2.One, 0xFF00FF00);
                    dl.AddRect(thisBytePos - Vector2.One, thisBytePos + byteSize + Vector2.One, 0xFF00FF00);
                }


                if (i >= compareData.Length) {
                    color = 0xFF555555;
                } else if (data[i] != compareData[i]) {
                    color = 0xFF3333FF;
                }
                
               

                dl.AddText(thisCharPos, color, $"{c}");
                dl.AddText(thisBytePos, color, $"{data[i]:X2}");

                if (mousePos.Y >= yPos && mousePos.Y <= yPos + charSize.Y) {
                    if ((mousePos.X >= charPosX && mousePos.X <= charPosX + charSize.X) || (mousePos.X >= bytePosX && mousePos.X <= bytePosX + byteSize.X)) {
                        compareHoverIndex = i;
                        if (ImGui.GetIO().MouseClicked[0]) {
                            compareInspectOffset = i;
                    
                        }
                        
                    }
                }
            }
            ImGui.Dummy(size);
            ImGui.PopFont();
        }
    
    private void DebugCompareFile() {
        
        DebugManager.PrintOutObject(currentFile, 0);
        
        if (simple == null) return;
        var original = CompareFile;

        //export ??= currentFile.Export();
        

        var output = simple.ToArray();

        var baseY = ImGui.GetCursorPosY();


        var hoveredIndex = compareHoverIndex;
        compareHoverIndex = -1;
        


        if (ImGui.BeginChild("compareView", new Vector2(-250 * ImGui.GetIO().FontGlobalScale, -1), true)) {
            DrawCompareSide(output.ToArray(), original, true);
            ImGui.SameLine();
            ImGui.Dummy(new Vector2(50));
            ImGui.SameLine();
            DrawCompareSide(original, output.ToArray());
        }
        ImGui.EndChild();
        ImGui.SameLine();
        if (ImGui.BeginTable("compareInspect", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {

            void Row(string a, string b, string c) {
                ImGui.TableNextColumn();
                ImGui.Text($"{a}");
                ImGui.TableNextColumn();
                ImGui.Text($"{b}");
                ImGui.TableNextColumn();
                ImGui.Text($"{c}");
            }
            
            Row("Offset", $"0x{compareInspectOffset:X}", $"0x{compareInspectOffset:X}");

            var dataA = output.Skip(compareInspectOffset).ToArray();
            var dataB = original.Skip(compareInspectOffset).ToArray();
            
            var byteA = output.Length > compareInspectOffset ? output[compareInspectOffset] : (byte) 0;
            var byteB = original.Length > compareInspectOffset ? original[compareInspectOffset] : (byte) 0;
            
            Row("Binary", Convert.ToString(byteA, 2).PadLeft(8, '0'), Convert.ToString(byteB, 2).PadLeft(8, '0'));

            Row("int8", $"{(sbyte)byteA}", $"{(sbyte)byteB}");
            Row("uint8", $"{byteA}", $"{byteB}");
            
            var shortA = dataA.Length >= 2 ? BitConverter.ToInt16(dataA) : (short)0;
            var shortB = dataB.Length >= 2 ? BitConverter.ToInt16(dataB) : (short)0;
            
            Row("int16", $"{shortA}", $"{shortB}");
            Row("uint16", $"{(ushort)shortA}", $"{(ushort)shortB}");
            
            
            var intA = dataA.Length >= 4 ? BitConverter.ToInt32(dataA) : 0;
            var intB = dataB.Length >= 4 ? BitConverter.ToInt32(dataB) : 0;
            Row("int16", $"{intA}", $"{intB}");
            Row("int16", $"{(uint)intA}", $"{(uint)intB}");
            
            
            ImGui.EndTable();
        }
    }


    public class TestRun {
        
        public bool IsRunning { get; private set; } = true;
        public bool IsSuccess { get; private set; } = false;
        private string uldName;


        private bool? fileExists = null;
        private byte[]? originalData = null;
        private byte[]? outputData = null;
        private ULD.Uld? parsedData = null;
        private bool? dataMatches = null;
        
        
        private string? jsonOutput = null;
        private ULD.Uld? jsonParsed = null;
        private byte[]? jsonParsedOutput = null;
        private bool? jsonMatches = null;


        private Vector4 color = ImGuiColors.DalamudYellow;
        private string message = "Starting...";
        private string? fullMessage = null;



        public TestRun(string uldName) {
            this.uldName = uldName;
        }

        public void Step() {
            if (!IsRunning) return;
            try {
                if (fileExists == null) {
                    color = ImGuiColors.DalamudYellow;
                    message = "Checking File Exists";
                    fileExists = Service.Data.GameData.FileExists(uldName);
                    return;
                }

                if (fileExists.Value == false) {
                    color = ImGuiColors.DalamudOrange;
                    message = $"{uldName} does not exist.";
                    IsRunning = false;
                    return;
                }

                if (originalData == null) {
                    color = ImGuiColors.DalamudYellow;
                    message = "Fetching ULD Data";
                    originalData = Service.Data.GameData.GetFile(uldName)?.Data;

                    if (originalData == null) {
                        message = "Failed Fetching Data";
                        color = ImGuiColors.DalamudRed;
                        IsRunning = false;
                    }

                    return;
                }

                if (parsedData == null) {
                    color = ImGuiColors.DalamudYellow;
                    message = "Parsing ULD Data";
                    parsedData = new(new BufferReader(originalData));


                    if (parsedData.ATK[0] != null && parsedData.ATK[0]?.RewriteDataOffset != 0) {
                        message = $"RewriteDataOffset: 0x{parsedData.ATK[0]?.RewriteDataOffset:X}";
                        IsRunning = false;
                    }
                
                    if (parsedData.ATK[1] != null && parsedData.ATK[1]?.RewriteDataOffset != 0) {
                        message = $"RewriteDataOffset: 0x{parsedData.ATK[1]?.RewriteDataOffset:X}";
                        IsRunning = false;
                    }

                    return;
                }

                if (outputData == null) {
                    color = ImGuiColors.DalamudYellow;
                    message = "Exporting ULD Data";
                    outputData = parsedData.Encode();
                    return;
                }

                if (dataMatches == null) {
                    color = ImGuiColors.DalamudYellow;
                    message = "Checking Sanity";


                    if (originalData.Length != outputData.Length) {
                        message = "Sanity Check Fail: Output size different.";
                        color = ImGuiColors.DalamudRed;
                        dataMatches = false;
                        IsRunning = false;
                        return;
                    } else {

                        for (var i = 0; i < originalData.Length && i < outputData.Length; i++) {
                            if (originalData[i] != outputData[i]) {
                                message = $"Santity Check Fail: Byte Mismatch at {i}";
                                color = ImGuiColors.DalamudRed;
                                dataMatches = false;
                                IsRunning = false;
                                return;
                            }
                        }

                        dataMatches = true;
                        return;

                    }
                }

                message = "Completed Successfully";
                color = ImGuiColors.HealerGreen;
                IsRunning = false;
                IsSuccess = true;

            } catch (Exception ex) {
                IsRunning = false;
                message = ex.Message;
                fullMessage = ex.ToString();
                color = ImGuiColors.DalamudRed;
            }
        }
        
        public void ImGuiOutput() {
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(message);
            ImGui.PopStyleColor();
            if (fullMessage != null && ImGui.IsItemHovered()) {
                ImGui.SetTooltip(fullMessage);
            }
            
            ImGui.SameLine();
            ImGui.PushID(uldName);
            hoveredIndex = compareHoverIndex;
            compareHoverIndex = -1;
            if (ImGui.TreeNode($"Details##{uldName}")) {
                
                if (originalData != null && ImGui.TreeNode("Original Data")) {
                    DrawCompareSide(originalData, originalData, true);
                    ImGui.TreePop();
                }

                if (parsedData != null) {
                    DebugManager.PrintOutObject(parsedData, 0, headerText: "Parsed Data");
                }
                
                if (outputData != null && ImGui.TreeNode("Output Data")) {
                    DrawCompareSide(outputData, originalData ?? outputData, true);
                    ImGui.TreePop();
                }
                
                if (jsonOutput != null && ImGui.TreeNode("JSON Output")) {
                    if (ImGui.Button("Copy to Clipboard")) {
                        ImGui.SetClipboardText(jsonOutput);
                    }
                    ImGui.TextWrapped(jsonOutput);
                    
                    ImGui.TreePop();
                }
                
                

                ImGui.TreePop();
            }
            ImGui.PopID();
            
            
            
        }
        
    }

    private Dictionary<string, TestRun> fullTestResults = new Dictionary<string, TestRun>();
    private TestRun? currentTestRun = null;

    private bool hideSuccess = true;
    
    public void FullTest() {

        var runningTests = fullTestResults.Values.Count(f => f.IsRunning);

        ImGui.Checkbox("Hide Successful Runs", ref hideSuccess);
        
        ImGui.SameLine();

        var successCount = fullTestResults.Values.Count(x => x.IsSuccess);
        
        ImGui.Text($"Success: {successCount} / {GameUld.List.Count} ({successCount * 100 / GameUld.List.Count}%%)");


        if (ImGui.BeginTable("fullUldTest", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg)) {
           
            ImGui.TableSetupColumn("ULD", ImGuiTableColumnFlags.WidthFixed, 300);
            ImGui.TableSetupColumn("Result");
            
            ImGui.TableHeadersRow();
            
            foreach (var uld in GameUld.List) {
                var hasResult = fullTestResults.TryGetValue(uld, out var result);
                if (hasResult && result is { IsSuccess: true } && result != currentTestRun && hideSuccess) {
                    continue;
                }
                
                
                ImGui.TableNextColumn();
                ImGui.Text(uld);
                if (ImGui.IsItemClicked()) {
                    try {
                        var name = uld;
                        var uldData = Service.Data.GameData.GetFile(name);

                        if (uldData != null) {
                            CompareFile = uldData.Data;
                            currentFile = new ULD.Uld(new BufferReader(uldData.Data));
                            simple = currentFile.Encode();
                            currentFileName = $"{name}";
                            currentFileFromGame = true;
                            activeModal = DebugCompareFile;
                        }
                    } catch (Exception ex) {
                        lastError = ex.ToString();
                    }
                    
                }
                
                ImGui.TableNextColumn();

                
                
                if (!hasResult) {
                    ImGui.TextDisabled("Waiting...");
                    if (runningTests < 10) {
                        currentTestRun = new TestRun(uld);
                        fullTestResults.Add(uld, currentTestRun);
                        runningTests++;
                    }
                } else {
                    if (result == null) {
                        ImGui.TextDisabled("Broken.");
                    } else {

                        if (currentTestRun == result && currentTestRun.IsRunning == false) {
                            currentTestRun = null;
                        } else {
                            for (var i = 0; i < 1; i++) result.Step();
                        }
                        
                        result.ImGuiOutput();
                    }
                }
            }
            
            
            
            
            
            ImGui.EndTable();
            
        }
        
        
        
    }
    
    
    
    private void SaveFile() {
        /*
        if (currentFile != null) {
            
            // var output = currentFile.Export();
            if (ImGui.Button("Save To File")) {
                try {
                    File.WriteAllBytes(@"C:\Users\Cara\Desktop\New folder\New Folder\ui\uld\testOutput.uld", output.ToArray());
                } catch (Exception ex) {
                    lastError = $"Failed to save: {ex.Message}";
                    //
                }
                
            }
        }
        */
    }
    


    private void DrawMenu() {
        if (ImGui.BeginMenuBar()) {
            if (ImGui.BeginMenu("File")) {
                if (ImGui.MenuItem("Load ULD from Game")) {
                    activeModal = LoadFromGame;
                }
                if (ImGui.MenuItem("Load ULD from File")) {
                    activeModal = LoadFile;
                }
                if (ImGui.MenuItem("Save ULD to File")) {
                    activeModal = SaveFile;
                }

                if (ImGui.MenuItem("Full Test")) {
                    activeModal = FullTest;
                }
                ImGui.EndMenu();
            }
            ImGui.EndMenuBar();
        }
        
    }
    
    public void Dispose() {
        
    }
    
    
}
