using ImGuiNET;

namespace STK.Test.ULDEditor; 

public static class ImGuiExt {

    public static bool InputByte(string label, ref byte value) {
        var v = (int)value;
        if (ImGui.InputInt(label, ref v)) {
            value = (byte)v;
            return true;
        }

        return false;
    }
    public static bool InputUInt(string label, ref uint value) {
        var v = (int)value;
        if (ImGui.InputInt(label, ref v)) {
            value = (uint)v;
            return true;
        }

        return false;
    }
    
    public static bool InputShort(string label, ref short value) {
        var v = (int)value;
        if (ImGui.InputInt(label, ref v)) {
            value = (short)v;
            return true;
        }

        return false;
    }

    public static bool InputUShort(string label, ref ushort value) {
        var v = (int)value;
        if (ImGui.InputInt(label, ref v)) {
            value = (ushort)v;
            return true;
        }

        return false;
    }
    
    
    
}
