using FFXIVClientStructs.FFXIV.Component.GUI;
using SimpleTweaksPlugin;

namespace STK.Test; 

public unsafe class TestAddon : Addon {
    public TestAddon() : base("STK.Test.Addon") { }
    
    public override bool GetUldData(out byte[] data) {
        try {
            var dir = 
            data = File.ReadAllBytes(Path.Join(Path.GetDirectoryName(STKTest._tweakProvider!.AssemblyPath), "Test.uld"));
            return true;
        } catch (Exception ex) {
            SimpleLog.Error(ex);
            data = Array.Empty<byte>();
            return false;
        }
    }

    private AtkTextNode* TextNode;
    private AtkTextNode* TextNode1;
    private AtkTextNode* TextNode2;
    
    
    protected override void OnSetup() {
        SimpleLog.Log("OnSetup");
        TextNode = AtkUnitBase->GetTextNodeById(5);
        TextNode1 = AtkUnitBase->GetTextNodeById(8);
        TextNode2 = AtkUnitBase->GetTextNodeById(9);
        if (TextNode != null) TextNode->SetText("Testing...");

        base.OnSetup();
    }

    protected override void OnDraw() {
        TextNode1->SetText($"{DateTime.Now.ToLongTimeString()}");
        base.OnDraw();
    }

    protected override long ReceiveGlobalEvent(AtkEventType a2, uint a3, void* a4, uint* a5) {
        SimpleLog.Log($"Receive Global Event [{a2}, {a3}]");
        
        TextNode->SetText($"Last Global Event: {a2}");
        
        return 0;
    }
}
