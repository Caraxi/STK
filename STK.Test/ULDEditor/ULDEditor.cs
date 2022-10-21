using System.Diagnostics;
using System.Numerics;
using System.Text;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using Lumina.Data.Files;
using SimpleTweaksPlugin;
using SimpleTweaksPlugin.Debugging;
using ULD;
using ULD.Component;
using ULD.Node;
using ULD.Node.Component;

namespace STK.Test.ULDEditor; 

public class UldEditor : IDisposable {

    private ULD.Uld? currentFile;
    private string currentFileName = "No File Open";
    private bool currentFileFromGame = false;
    
    private Action? activeModal;
    private bool isFileDirty = false;

    private string lastError = string.Empty;

    private PreviewAddon? previewAddon;


    private ResNode? cutNode = null;

    private List<NodePropertyEditor> nodeEditors = new List<NodePropertyEditor>();

    public void DrawWindow() {

        var isOpen = true;
        
        if (ImGui.Begin($"{(isFileDirty?"*":"")}ULD Editor - {(currentFileFromGame?"[GameFile] ":"")}{currentFileName}###uldEditor_mainWindow", ref isOpen, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoCollapse)) {
            if (ImGui.IsWindowAppearing() && activeModal == null && currentFile == null) {
                activeModal = LoadFile;
            }
            DrawInterface();
        }
        ImGui.End();
        
        nodeEditors.RemoveAll(n => !n.IsOpen);
        foreach (var n in nodeEditors) {
            n.Draw();
        }

        if (!isOpen) {
            Service.PluginInterface.UiBuilder.Draw -= this.DrawWindow;
            STKTest.editor = null;
            previewAddon?.Dispose();
            previewAddon = null;
        }
    }

    public void DrawListEditorTabs<T>(ListHeader<T> list, ULD.Uld uld) where T : ListElement, new() {
        if (ImGui.BeginTabBar("##uldEditor_listEditorTabs", ImGuiTabBarFlags.None)) {
            foreach (var e in list.Elements) {
                if (ImGui.BeginTabItem($"{e.GetType().Name}#{e.Id}")) {
                    DrawElementEditor(e, uld);
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }
    
    public void DrawListEditor<T>(ListHeader<T> list, ULD.Uld uld, bool useTabs = false) where T : ListElement, new() {
        T? deleteElement = null;
        foreach (var e in list.Elements) {
            if (ImGui.CollapsingHeader($"{e.GetType().Name}#{e.Id}")) {
                if (ImGui.Button($"Delete##{e.GetType()}#{e.Id}")) {
                    deleteElement = e;
                }
                DrawElementEditor(e, uld);
            }
        }

        if (deleteElement != null) {
            list.Elements.Remove(deleteElement);
        }
    }


    public uint GetNextNodeId(ResNode? rootNode, bool final = true) {
        if (rootNode == null) return 0 + (final ? 1U : 0);
        return Math.Max(rootNode.Id, Math.Max(GetNextNodeId(rootNode.Child, false), GetNextNodeId(rootNode.NextSibling, false))) + (final ? 1U : 0);
    }

    public void DrawElementEditor<T>(T element, ULD.Uld uld) where T : ListElement {
        
        var references = element.GetReferences(uld);
        if (references.Count > 0) {
            ImGui.Text("References:");
            ImGui.Indent();
            foreach (var reference in references) {
                // if (reference == element) continue;
                ImGui.Text("-");
                ImGui.SameLine();
                ImGui.TextDisabled(reference.Description);
                ImGui.SameLine();
                if (reference.Reference is ListElement le) {
                    ImGui.Text($"{reference.Reference.GetType().Name}#{le.Id}");
                } else {
                    ImGui.Text($"{reference.Reference.GetType().Name}");
                }
            }
            ImGui.Unindent();
        }
        
        switch (element) {
            case ComponentBase component:
                DrawComponentEditor(component, uld);
                break;
            case Widget widget:
                DrawWidgetEditor(widget, uld);
                break;
            case Asset asset:
                DrawAssetEditor(asset, uld);
                break;
            case Parts parts:
                DrawPartsEditor(parts, uld);
                break;
            default:
                ImGui.Text("Item Not Editable");
                break;
        }
    }


    public void DrawPartsEditor(Parts parts, ULD.Uld uld) {
        DebugManager.PrintOutObject(parts, 0, new List<string>() { $"ULD.PartsList.{parts.Id}" }, true);
    }
    
    
    public void DrawComponentEditor(ComponentBase component, ULD.Uld uld) {
        DebugManager.PrintOutObject(component, 0, new List<string>() { $"ULD.Component.{component.Id}" }, true);
    }
    
    public void DrawWidgetEditor(Widget widget, ULD.Uld uld) {
        DebugManager.PrintOutObject(widget, 0, new List<string>() { $"ULD.Widget.{widget.Id}" }, true);
        ImGui.Separator();
        DrawNodeList(widget.RootNode, uld, widget.RootNode);
    }

    private bool childOfCutNode = false;
    
    public void DrawNodeList(ResNode? node, ULD.Uld uld, ResNode? rootNode, List<string>? id = null) {
        if (node == null) return;
        id ??= new List<string>();
        
        if (node == cutNode) childOfCutNode = true;

        ImGui.PushID(string.Join('#', id) + $"#Node{node.Id}");
        ImGui.ColorConvertHSVtoRGB(ImGui.GetID("color") % 1024f / 1024f, 1f, 1f, out var r, out var g, out var b); 
        var color = new Vector4(r, g, b, 1f);
        var colorInt = ImGui.ColorConvertFloat4ToU32(color);
        
        ImGui.PushStyleColor(ImGuiCol.Text, color);


        var flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.NoTreePushOnOpen;

        var hasChildren = node.Child != null || node is BaseComponentNode;
        
        
        if (!hasChildren) flags |= ImGuiTreeNodeFlags.Bullet;

        
        ImGui.BeginGroup();
        
        var headerOpen = ImGui.TreeNodeEx($"##{node.GetType().Name}#{node.Id}", flags);
        ImGui.PopStyleColor();
        
        var o = ImGui.GetItemRectMin();
        ImGui.SameLine();
        ImGui.Text($"{node.GetType().Name}");
        ImGui.SameLine();
        ImGui.TextDisabled($"#{node.Id}");
        ImGui.EndGroup();
        
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 2);
        ImGui.PushStyleColor(ImGuiCol.Border, color);
        
        if (ImGui.BeginPopupContextItem(string.Join('#', id) + $"Node#{node.Id}")) {
            ImGui.Text($"{node.GetType().Name}#{node.Id}");
            ImGui.Separator();
            if (node.PrevSibling != null) {
                if (ImGui.Selectable("Move Up")) {
                    
                }
            }
            if (node.NextSibling != null) {
                ImGui.Selectable("Move Down");
            }


            if (nodeEditors.All(ne => ne.Node != node)) {
                if (ImGui.Selectable("Edit Properties")) {
                    nodeEditors.Add(new NodePropertyEditor(node));
                }
            }
            
            
            
            ImGui.Separator();

            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF3333AA);
            if (ImGui.Selectable("Delete")) {
                if (node.Parent != null && node.Parent.Child == node) {
                    node.Parent.Child = node.NextSibling;
                }

                if (node.NextSibling != null) {
                    node.NextSibling.PrevSibling = node.PrevSibling;
                }

                if (node.PrevSibling != null) {
                    node.PrevSibling.NextSibling = node.NextSibling;
                }
            }
            ImGui.PopStyleColor();
            
            if (ImGui.Selectable("Cut")) {
                cutNode = node;
                childOfCutNode = false;
            }
            
            if (cutNode != node && cutNode != null && !childOfCutNode) {
                ImGui.Selectable("Paste as First Child");
                ImGui.Selectable("Paste as Last Child");
                ImGui.Selectable("Paste as Previous Sibling");
                ImGui.Selectable("Paste as Next Sibling");
            }

            
            
            ImGui.Separator();
            

            if (ImGui.Selectable("Duplicate Node")) {

                var data = node.Encode();
                var copy = ResNode.ReadNode(uld, new BufferReader(data));

                copy.Id = GetNextNodeId(rootNode);
                
                copy.PrevSibling = node;
                copy.NextSibling = node.NextSibling;
                copy.Parent = node.Parent;
                copy.Child = null;
                if (node.NextSibling != null) {
                    node.NextSibling.PrevSibling = copy;
                }
                node.NextSibling = copy;
            }
            
            ImGui.Separator();
            ImGui.Selectable("Cancel");
            ImGui.EndPopup();
        }
        
        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
        
        if (headerOpen && hasChildren) {
            ImGui.Indent();
                
            ImGui.BeginGroup();
            if (node.Child != null) DrawNodeList(node.Child, uld, rootNode, new List<string>(id) { $"Node#{node.Id}" });
        
            if (node is BaseComponentNode) {
            
                var component = uld.GetComponent(node.Type);
                if (component != null) {
                
                    DrawNodeList(component.RootNode, uld, component.RootNode, new List<string>(id) { $"Node#{node.Id}", $"Component#{component.Id}" });
                
                
                
                    // dl.AddRect(ImGui.GetItemRectMin()-Vector2.One, ImGui.GetItemRectMax(), 0xFF5588CC);

                }
            }
            ImGui.EndGroup();
            var dl = ImGui.GetWindowDrawList();
            //dl.AddRect(o, new Vector2(ImGui.GetWindowPos().X + ImGui.GetContentRegionMax().X,ImGui.GetItemRectMax().Y), colorInt);
            // dl.AddRect(o - Vector2.One, new Vector2(ImGui.GetWindowPos().X + ImGui.GetContentRegionMax().X,ImGui.GetItemRectMax().Y) + Vector2.One, colorInt);
        
            dl.AddLine(o, new Vector2(o.X, ImGui.GetItemRectMax().Y), colorInt, 3);
            dl.AddLine(o, new Vector2(ImGui.GetWindowPos().X + ImGui.GetContentRegionMax().X, o.Y), colorInt, 1);
            ImGui.Unindent();
        
        }
        ImGui.PopID();

        if (node.NextSibling != null) DrawNodeList(node.NextSibling, uld, rootNode, id);

        if (cutNode == node) childOfCutNode = false;

    }
    

    public void DrawAssetEditor(Asset asset, ULD.Uld uld) {
            ImGui.PushID($"ULD.Asset.{asset.Id}");
            DebugManager.PrintOutObject(asset, 0, new List<string>() { $"ULD.Asset.{asset.Id}" }, true);

            var path = asset.Path;
            var iconId = (int)asset.IconId;
            var themes = asset.Themes;

            ImGui.BeginGroup();
            
            ImGui.PushItemWidth(250);
            if (ImGui.InputText("Asset Path", ref path, 44)) {
                if (Encoding.UTF8.GetBytes(path).Length <= 44) {
                    asset.Path = path;
                    isFileDirty = true;
                }
            }
            
            if (ImGui.InputInt("Unknown 1", ref iconId)) {
                asset.IconId = (uint)iconId;
                isFileDirty = true;
            }
            
            ImGui.PopItemWidth();
            ImGui.EndGroup();

            
            if (asset.Path.EndsWith(".tex") && Service.Data.FileExists(asset.Path)) {
                
                var texture = GetTexture(asset.Path);
                if (texture != null) {
                    ImGui.SameLine();
                    ImGui.Image(texture.ImGuiHandle, new Vector2(texture.Width, texture.Height));
                }


            }
            
            
            ImGui.PopID();
    }

    private Dictionary<string, TextureWrap?> loadedTextures = new();

    private TextureWrap? GetTexture(string texturePath) {
        if (loadedTextures.TryGetValue(texturePath, out var texture)) {
            return texture;
        }
        loadedTextures.Add(texturePath, null);

        Task.Run(() => {
            try {
                if (Service.Data.FileExists(texturePath)) {
                    var textureData = Service.Data.GetFile<TexFile>(texturePath);
                    if (textureData == null) return;
                    var tex = Service.PluginInterface.UiBuilder.LoadImageRaw(textureData.GetRgbaImageData(), textureData.Header.Width, textureData.Header.Height, 4);
                    loadedTextures[texturePath] = tex;
                }
            } catch {
                //
            }
        });
        
        
        
        
        return null;
    }



    public void DrawInterface() {
        DrawMenu();

        if (!string.IsNullOrEmpty(lastError)) {
            ImGui.TextColored(ImGuiColors.DalamudRed, lastError);
            if (ImGui.SmallButton("ok")) lastError = string.Empty;
            ImGui.Separator();
        }
        
        if (activeModal != null) {
            activeModal();
            return;
        }
        
        if (currentFile == null) {
            ImGui.Text("No file open");
            return;
        }

        if (ImGui.BeginTabBar("ULD Editor Tabs")) {

            if (ImGui.BeginTabItem("General")) {

                if (currentFile.ATK[0]?.Assets != null && currentFile.ATK[0].Assets.Version != "0101") {
                    if (ImGui.Button("Upgrade ATK.Assets Version")) {
                        currentFile.ATK[0].Assets.Version = "0101";
                    }
                }
                
                
                
                
                ImGui.EndTabItem();
            }


            for (var i = 0; i < currentFile.ATK?.Length; i++) {
                var atk = currentFile.ATK[i];
                if (atk == null) continue;
                if (atk.Assets != null && ImGui.BeginTabItem($"Assets[{i+1}]")) {
                    DrawListEditor(atk.Assets, currentFile);
                    ImGui.EndTabItem();
                }
                
                if (atk.Parts != null && ImGui.BeginTabItem($"Parts[{i+1}]")) {
                    DrawListEditor(atk.Parts, currentFile);
                    ImGui.EndTabItem();
                }

                if (atk.Components != null && ImGui.BeginTabItem($"Components[{i+1}]")) {
                    DrawListEditor(atk.Components, currentFile);
                    ImGui.EndTabItem();
                }

                if (atk.Widgets != null && ImGui.BeginTabItem($"Widgets[{i+1}]")) {
                    DrawListEditorTabs(atk.Widgets, currentFile);
                    ImGui.EndTabItem();
                }
            }
            
            ImGui.EndTabBar();
        }
    }
    
    private string inputLoadFromGameUldName = "";
    private string listSelected = "";
    private void LoadFromGame() {

        if (isFileDirty) {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            ImGui.TextWrapped("You have unsaved changes. Loading a new file will discard any unsaved changed.");
            ImGui.PopStyleColor();
        }
        
        ImGui.Text("Enter ULD Name");

        string? loadFromList = null;
        
        ImGui.InputText("###loadFromGame_inputText", ref inputLoadFromGameUldName, 100);
        var buttonSize = ImGui.GetItemRectSize();
        if (ImGui.BeginChild("fileSelector", new Vector2(buttonSize.X, buttonSize.Y * 8), true)) {
            foreach (var f in GameUld.List) {
                if (inputLoadFromGameUldName.Length > 0 && !f.Contains(inputLoadFromGameUldName, StringComparison.InvariantCultureIgnoreCase)) continue;
                if (ImGui.Selectable($"{f}", f == listSelected)) {
                    if (listSelected == f) {
                        loadFromList = f;
                    }
                    listSelected = f;
                }
            }
        }
        ImGui.EndChild();
        
        var name = loadFromList ?? inputLoadFromGameUldName;
        if (!name.EndsWith(".uld")) name = name + ".uld";
        if (!name.Contains("/")) name = "ui/uld/" + name;
        
        if (name.Length > 0 && Service.Data.FileExists($"{name}")) {
            if (loadFromList != null || ImGui.Button("Load ULD", buttonSize)) {
                try {
                    ULD.Logging.LogMessage -= PluginLog.Log;
                    ULD.Logging.LogMessage += PluginLog.Log;
                    
                    var uld = Service.Data.GameData.GetFile(name);
                    
                    if (uld != null) {
                        currentFile = new ULD.Uld(new BufferReader(uld.Data));
                        currentFileName = $"{name}";
                        currentFileFromGame = true;
                        activeModal = null;
                    }
                } catch (Exception ex) {
                    currentFile = null;
                    previewAddon?.Dispose();
                    previewAddon = null;
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
            previewAddon?.Dispose();
            previewAddon = null;
            var bytes = File.ReadAllBytes(Path.Join(Path.GetDirectoryName(STKTest._tweakProvider!.AssemblyPath), "Test.uld"));
            currentFile = new ULD.Uld(new BufferReader(bytes));
            
            activeModal = null;
        }
    }

    private void SaveFile() {
        if (currentFile != null) {
            var output = currentFile.Encode();
            if (ImGui.Button("Save To File")) {
               
            }
        }
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
                if (currentFile != null && ImGui.MenuItem("Save ULD to File")) {
                    try {
                        var output = currentFile.Encode();
                        File.WriteAllBytes(Path.Join(Path.GetDirectoryName(STKTest._tweakProvider!.AssemblyPath), "Test.uld"), output.ToArray());
                        lastError = "Saved";
                    } catch (Exception ex) {
                        lastError = $"Failed to save: {ex.Message}";
                        //
                    }
                }
                ImGui.EndMenu();
            }

            if (currentFile != null) {

                if (ImGui.MenuItem("Preview")) {
                    if (!STK.Initalized) STK.Initalize();
                    
                    previewAddon?.Dispose();
                    previewAddon = new PreviewAddon(currentFile);
                }
                
                
            }
            
            ImGui.EndMenuBar();
        }
        
    }
    
    public void Dispose() {
        previewAddon?.Dispose();
    }
    
    
}
