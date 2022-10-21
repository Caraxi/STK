using ImGuiNET;
using SimpleTweaksPlugin;
using SimpleTweaksPlugin.Debugging;
using SimpleTweaksPlugin.TweakSystem;
using STK.Test.ULDEditor;

namespace STK.Test;

public class STKTest : DebugHelper {

    private TestAddon? testAddon;

    internal static CustomTweakProvider? _tweakProvider;
    
    internal static UldEditor? editor = null;

    public override void Draw() {

        _tweakProvider = TweakProvider as CustomTweakProvider;
        
        if (editor == null) {
            if (ImGui.Button("Open ULD Editor")) {
                editor ??= new UldEditor();
                Service.PluginInterface.UiBuilder.Draw += editor.DrawWindow;
            }
        }

        if (testAddon == null) {
            if (ImGui.Button("Create Test Addon")) {
                if (!STK.Initalized) {
                    STK.Initalize();
                    Logging.Log = o => SimpleLog.Log(o);
                    Logging.Verbose = o => SimpleLog.Verbose(o);
                }

                testAddon = new TestAddon();
            }
        } else {
            if (ImGui.Button("Destroy Test Addon")) {
                testAddon?.Dispose();
                testAddon = null;
            }
        }
        
    }

    public override void Dispose() {
        testAddon?.Dispose();
        
        STK.Cleanup(); 
        
        if (editor != null) {
            Service.PluginInterface.UiBuilder.Draw -= editor.DrawWindow;
            editor?.Dispose();
        }
        
        
        base.Dispose();
        
        
        
        
    }

    public override string Name => "STK Test";
}
