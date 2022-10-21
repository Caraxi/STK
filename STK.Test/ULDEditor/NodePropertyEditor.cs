using System.Numerics;
using System.Reflection;
using ImGuiNET;
using ULD.Node;
using ULD.Node.Component;

namespace STK.Test.ULDEditor; 

public class NodePropertyEditor {

    public ResNode Node { get; private set; }
    
    private Guid uniqueId = Guid.NewGuid();


    public bool IsOpen = true;

    private float propertyNameWidth = 150f;
    
    
    public NodePropertyEditor(ResNode node) {
        Node = node;
    }


    public void Draw() {
        if (!IsOpen) return;
        ImGui.PushID($"{uniqueId}");
        ImGui.SetNextWindowSize(new Vector2(400, 800), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(450, 400), new Vector2(800, 8000));
        if (ImGui.Begin($"{Node.GetType().Name}#{Node.Id} Properties##nodeProperties", ref IsOpen, ImGuiWindowFlags.NoSavedSettings)) {

            ImGui.Text("Parent:");
            ImGui.SameLine();
            var parent = Node.Parent;
            if (parent != null) {
                ImGui.Text($"{parent.GetType().Name}#{parent.Id}");
            } else {
                ImGui.Text("None");
            }
            
            
            ImGui.Text("Previous Sibling:");
            ImGui.SameLine();
            var pSibling = Node.PrevSibling;
            if (pSibling != null) {
                ImGui.Text($"{pSibling.GetType().Name}#{pSibling.Id}");
            } else {
                ImGui.Text("None");
            }
            
            ImGui.Text("Next Sibling:");
            ImGui.SameLine();
            var nSibling = Node.NextSibling;
            if (nSibling != null) {
                ImGui.Text($"{parent.GetType().Name}#{nSibling.Id}");
            } else {
                ImGui.Text("None");
            }
            
            ImGui.Text("Child:");
            ImGui.SameLine();
            var child = Node.Child;
            if (child != null) {
                ImGui.Text($"{child.GetType().Name}#{child.Id}");
            } else {
                ImGui.Text("None");
            }
            
            if (ImGui.BeginTable("propertiesTable", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders)) {
                ImGui.TableSetupColumn("Property", ImGuiTableColumnFlags.WidthFixed, propertyNameWidth);
                ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();




                switch (Node) {
                    case TextNode textNode:
                        DrawTextNodeProperties(textNode);
                        break;
                    case NineGridNode nineGridNode:
                        DrawNineGridNodeProperties(nineGridNode);
                        break;
                    case BaseComponentNode componentNode:
                        DrawComponentNodeProperties(componentNode);
                        break;
                    default:
                        DrawResNodeProperties(Node);
                        break;
                };
                
                ImGui.EndTable();
            }
            
            
            
            
            
            
        }
        ImGui.End();
        ImGui.PopID();
    }


    private void DrawComponentNodeProperties(BaseComponentNode node) {

        switch (node) {
            case WindowComponentNode wc:
                ImGui.TableNextColumn();
                ImGui.TextDisabled("WindowComponentNode");
                ImGui.TableNextColumn();
                ImGui.Text("");
                
                PropertyEditor("Title ID", ref wc.TitleTextId);
                PropertyEditor("Subtitle ID", ref wc.SubtitleTextId);
                PropertyEditor("Show Close Button", ref wc.CloseButton);
                PropertyEditor("Show Config Button", ref wc.ConfigButton);
                PropertyEditor("Show Help Button", ref wc.HelpButton);
                PropertyEditor("Show Header", ref wc.Header);
                
                break;
        }
        
        ImGui.TableNextColumn();
        ImGui.TextDisabled("ComponentNode");
        ImGui.TableNextColumn();
        ImGui.Text("");
        
        PropertyEditor("Index", ref node.Index);
        PropertyEditor("Up", ref node.Up);
        PropertyEditor("Down", ref node.Down);
        PropertyEditor("Left", ref node.Left);
        PropertyEditor("Right", ref node.Right);
        PropertyEditor("Cursor", ref node.Cursor);
        
        PropertyEditor("Repeat Up", ref node.RepeatUp);
        PropertyEditor("Repeat Down", ref node.RepeatDown);
        PropertyEditor("Repeat Left", ref node.RepeatLeft);
        PropertyEditor("Repeat Right", ref node.RepeatRight);

        PropertyEditor("Unk3", ref node.Unk3);
        PropertyEditor("Unk4", ref node.Unk4);
        
        PropertyEditor("Offset", ref node.OffsetX, ref node.OffsetY);
        
        DrawResNodeProperties(node);
        
    }
    
    private void DrawTextNodeProperties(TextNode textNode) {
        ImGui.TableNextColumn();
        ImGui.TextDisabled("TestNode");
        ImGui.TableNextColumn();
        ImGui.Text("");
        
        PropertyEditor("Text ID", ref textNode.TextId);
        EnumPropertyEditor("Sheet Type", "SheetType");
        
        PropertyEditor("Color", ref textNode.Color);
        EnumPropertyEditor("Alignment");
        EnumPropertyEditor("Font");
        PropertyEditor("Font Size", ref textNode.FontSize);
        PropertyEditor("Edge Color", ref textNode.EdgeColor);
        
        PropertyEditor("Bold", ref textNode.Bold);
        PropertyEditor("Italic", ref textNode.Italic);
        PropertyEditor("Edge", ref textNode.Edge);
        PropertyEditor("Glare", ref textNode.Glare);
        PropertyEditor("Multiline", ref textNode.Multiline);
        PropertyEditor("Ellipsis", ref textNode.Ellipsis);
        PropertyEditor("Paragraph", ref textNode.Paragraph);
        PropertyEditor("Emboss", ref textNode.Emboss);
        
        PropertyEditor("Character Spacing", ref textNode.CharSpacing);
        PropertyEditor("Line Spacing", ref textNode.LineSpacing);
        
        PropertyEditor("Unknown 2", ref textNode.Unk2);
        DrawResNodeProperties(textNode);
    }
    
    private void DrawResNodeProperties(ResNode node) {
        ImGui.TableNextColumn();
        ImGui.TextDisabled("ResNode");
        ImGui.TableNextColumn();
        ImGui.Text("");

        PropertyEditor("Position", ref Node.X, ref Node.Y);
        PropertyEditor("Size", ref Node.W, ref Node.H);
        PropertyEditor("Rotation", ref Node.Rotation);
        PropertyEditor("Scale", ref Node.ScaleX, ref Node.ScaleY);
        PropertyEditor("Origin", ref Node.OriginX, ref Node.OriginY);
        PropertyEditor("Priority", ref Node.Priority);
        
        PropertyEditor("Visible", ref Node.Visible);
        PropertyEditor("Enabled", ref Node.Enabled);
        PropertyEditor("Clip", ref Node.Clip);
        PropertyEditor("Fill", ref Node.Fill);
        
        PropertyEditor("Anchor Top", ref Node.AnchorTop);
        PropertyEditor("Anchor Left", ref Node.AnchorLeft);
        PropertyEditor("Anchor Right", ref Node.AnchorRight);
        PropertyEditor("Anchor Bottom", ref Node.AnchorBottom);
        
        PropertyEditor("Multiply Colour", ref Node.MultiplyRed, ref Node.MultiplyGreen, ref Node.MultiplyBlue);
        PropertyEditor("Add Colour", ref Node.AddRed, ref Node.AddGreen, ref Node.AddBlue);
        
        PropertyEditor("Alpha", ref Node.Alpha);
    }

    private void DrawNineGridNodeProperties(NineGridNode node) {
        ImGui.TableNextColumn();
        ImGui.TextDisabled("NineGridNode");
        ImGui.TableNextColumn();
        ImGui.Text("");
        
        PropertyEditor("Part List ID", ref node.PartListId);
        PropertyEditor("Part ID", ref node.PartId);
        
        EnumPropertyEditor("Grid Parts Type", nameof(node.GridPartsType));
        EnumPropertyEditor("Grid Render Type", nameof(node.GridRenderType));
        
        
        PropertyEditor("Top Offset", ref node.TopOffset);
        PropertyEditor("Left Offset", ref node.LeftOffset);
        PropertyEditor("Right Offset", ref node.RightOffset);
        PropertyEditor("Bottom Offset", ref node.BottomOffset);
        
        PropertyEditor("Unknown 3", ref node.Unk3);
        PropertyEditor("Unknown 4", ref node.Unk4);
       
        DrawResNodeProperties(node);
    }

    private Dictionary<string, FieldInfo?> enumProperties = new();
    

    private void EnumPropertyEditor(string name, string? enumName = null) {
        enumName ??= name;
        PropertyName(name);
        ImGui.SetNextItemWidth(-1);

        if (!enumProperties.TryGetValue(enumName, out var fi)) {
            fi = Node.GetType().GetField(enumName);
            enumProperties.Add(enumName, fi);
        }

        if (fi == null || !fi.FieldType.IsEnum) {
            ImGui.Text("Error");
        } else {
            var currentValue = fi.GetValue(Node);

            if (ImGui.BeginCombo($"###enumSelect_{name}_{enumName}", $"{currentValue}")) {

                var options = Enum.GetValues(fi.FieldType);

                foreach (var o in options) {
                    if (ImGui.Selectable($"{o}", currentValue == o)) {
                        fi.SetValue(Node, o);
                    }
                }
                ImGui.EndCombo();
            }
        }
    }
    
    private void PropertyName(string name) {
        ImGui.TableNextColumn();

        var textSize = ImGui.CalcTextSize(name);
        ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - textSize.X);
        ImGui.Text(name);
        ImGui.TableNextColumn();
    }
    
    private void PropertyEditor(string name, ref uint value) {
        PropertyName(name);
        ImGui.SetNextItemWidth(-1);
        ImGuiExt.InputUInt($"##{name}", ref value);
    }
    
    private void PropertyEditor(string name, ref byte value) {
        PropertyName(name);
        ImGui.SetNextItemWidth(-1);
        ImGuiExt.InputByte($"##{name}", ref value);
    }
    
    private void PropertyEditor(string name, ref short value) {
        PropertyName(name);
        ImGui.SetNextItemWidth(-1);
        ImGuiExt.InputShort($"##{name}", ref value);
    }
    
    private void PropertyEditor(string name, ref short value, ref short value2) {
        PropertyName(name);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 2);
        ImGuiExt.InputShort($"##{name}1", ref value);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGuiExt.InputShort($"##{name}2", ref value2);
    }
    private void PropertyEditor(string name, ref short value, ref short value2, ref short value3) {
        PropertyName(name);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 3);
        ImGuiExt.InputShort($"##{name}1", ref value);
        ImGui.SameLine();        
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 2);
        ImGuiExt.InputShort($"##{name}2", ref value2);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGuiExt.InputShort($"##{name}3", ref value3);
    }
    
    private void PropertyEditor(string name, ref ushort value) {
        PropertyName(name);
        ImGui.SetNextItemWidth(-1);
        ImGuiExt.InputUShort($"##{name}", ref value);
    }
    
    private void PropertyEditor(string name, ref ushort value, ref ushort value2) {
        PropertyName(name);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 2);
        ImGuiExt.InputUShort($"##{name}1", ref value);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGuiExt.InputUShort($"##{name}2", ref value2);
    }
    
    private void PropertyEditor(string name, ref float value) {
        PropertyName(name);
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat($"##{name}", ref value);
    }
    
    private void PropertyEditor(string name, ref bool value) {
        PropertyName(name);
        ImGui.Checkbox($"##{name}", ref value);
    }
    
    private void PropertyEditor(string name, ref float value, ref float value2) {
        PropertyName(name);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 2);
        ImGui.InputFloat($"##{name}1", ref value);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat($"##{name}2", ref value2);
    }
    
    
    
}
