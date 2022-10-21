using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace STK; 

public unsafe partial class Addon : IDisposable {

    private static readonly List<Addon> Addons = new();
    
    public static void Cleanup() {
        for (var i = Addons.Count - 1; i >= 0; i--) {
            Addons[i].Dispose();
        }
        Addons.Clear();
    }

    public AtkUnitBase* AtkUnitBase { get; }
    
    public string AddonName { get; }
    

    private static ushort? GetUnusedID() {
        var i = ushort.MaxValue;
        while (AtkStage.GetSingleton()->RaptureAtkUnitManager->GetAddonById(i) != null && i > 49999) i--;
        if (i <= 50000) return null;
        return i;
    }
    
    public Addon(string addonName) {
        this.AddonName = addonName;
        if (!STK.Initalized) throw new Exception("STK not Initalized");
        var id = GetUnusedID();
        if (id == null) throw new Exception("No ID was available.");
        
        Logging.Log("Attemtping to create a new addon");

        Addons.Add(this);

        var lvAgent = Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.LicenseViewer);
        var lvAddon = AtkStage.GetSingleton()->RaptureAtkUnitManager->GetAddonByName("LicenseViewer");

        var doReopen = lvAddon != null;
        if (lvAddon == null) {
            lvAgent->Show();
            lvAddon = AtkStage.GetSingleton()->RaptureAtkUnitManager->GetAddonByName("LicenseViewer");
        }
        
        if (lvAddon != null) {
            lvAddon->ID = id.Value;
            Util.WriteString(lvAddon->Name, addonName);
            
            lvAgent->Hide();
            if (doReopen) lvAgent->Show();
            AtkUnitBase = lvAddon;
            ReplaceVirtualTable();
        }
    }
    
    public void Close() {
        if (AtkUnitBase != null) {
            AtkUnitBase->IsVisible = false;
            AtkUnitBase->Hide(true);
        }
    }


    public virtual bool GetUldData(out byte[] data) {
        data = Array.Empty<byte>();
        return false;
    }
    
    public bool IsDisposed { get; private set; } = false;

    protected virtual void OnDispose() { }

    public void Dispose() {
        if (IsDisposed) return;
        Close();
        Addons.Remove(this);
        OnDispose();
        IsDisposed = true;
    }
}
