using System.Runtime.InteropServices;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace STK; 

public unsafe partial class Addon {
    
    [StructLayout(LayoutKind.Explicit, Size = 8 * FuncCount)]
    public partial struct VirtualTable {
        public const int FuncCount = 72;
    }
    
    private VirtualTable* customVirtualTable;
    private VirtualTable* original;
    private static VirtualTable* _atkUnitBase;

    private void ReplaceVirtualTable() {

        if (_atkUnitBase == null) {
            try {
                var baseVTable =  STK.Scanner.GetStaticAddressFromSig("48 89 01 48 8D 05 ?? ?? ?? ?? 4C 89 41 28", -0xA);
                if (baseVTable != IntPtr.Zero) _atkUnitBase = (VirtualTable*)baseVTable;
            } catch {
                //
            }
        }
        
        customVirtualTable = (VirtualTable*) IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(VirtualTable), 8UL);
        original = (VirtualTable*)AtkUnitBase->AtkEventListener.vtbl;
        if (_atkUnitBase == null) _atkUnitBase = original;
        
        var o = (byte*)(original);
        var n = (byte*)(customVirtualTable);
        for (var u = 0UL; u < (ulong) sizeof(VirtualTable); u++) *(n + u) = *(o + u);

        ReplaceVirtualTable(customVirtualTable);
        
        AtkUnitBase->AtkEventListener.vtbl = customVirtualTable;
    }

    private partial void ReplaceVirtualTable(VirtualTable* newTable);
    
    private static bool TryGet(AtkUnitBase* atkUnitBase, out Addon? addon) {
        addon = Addons.FirstOrDefault(a => a.AtkUnitBase == atkUnitBase);
        return addon != null && addon.AtkUnitBase != null && !addon.IsDisposed;
    }

    [VirtualFunction(1)] 
    protected virtual partial long ReceiveGlobalEvent(AtkEventType a2, uint a3, void* a4, uint* a5);
    
    [VirtualFunction(2)]
    protected virtual partial long ReceiveEvent(AtkEventType a2, uint a3, void* a4, uint* a5);
    
    
    [VirtualFunction(16)]
    protected virtual bool OnClose() {
        Dispose();
        return true;
    }

    [VirtualFunction(41)]
    protected virtual partial void OnDraw();

    
    [VirtualFunction(46)]
    private void OnSetup(int atkValueCount, AtkValue* atkValues) {
        OnSetup();
    }

    protected virtual void OnSetup() {}
    

    public CustomResourceHandle? UldResourceHandle { get; private set; } = null;

    
    private IntPtr allocatedComponents = IntPtr.Zero;
    private IntPtr allocatedWidgets = IntPtr.Zero;
    
    

    [VirtualFunction(43)]
    private bool LoadUldResourceHandle() {
        
        Logging.Log($"Loaded State: {AtkUnitBase->UldManager.LoadedState} / {(ulong)AtkUnitBase->UldManager.UldResourceHandle:X}");

        if (allocatedComponents != IntPtr.Zero) Marshal.FreeHGlobal(allocatedComponents);
        if (allocatedWidgets != IntPtr.Zero) Marshal.FreeHGlobal(allocatedWidgets);
        
        
        if (this.AtkUnitBase->LoadUldByName($"TitleLicenseViewer")) {
            UldResourceHandle = new CustomResourceHandle((ResourceHandle*)AtkUnitBase->UldManager.UldResourceHandle, new Dictionary<string, CustomResourceHandle.GetCustomData>() {
                    [$"ui/uld/TitleLicenseViewer.uld"] = this.GetUldData
                }
                );
            Logging.Log($"Loaded State: {AtkUnitBase->UldManager.LoadedState} / {(ulong)AtkUnitBase->UldManager.UldResourceHandle:X}");

            return true;
        }

        

        return false;
    }
}
