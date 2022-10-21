using System;
using System.Runtime.InteropServices;

namespace STK;

public unsafe partial class Addon {
    public partial struct VirtualTable {
        [FieldOffset(8)] public delegate*unmanaged<FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*, FFXIVClientStructs.FFXIV.Component.GUI.AtkEventType,uint,void*,uint*,long> vReceiveGlobalEvent;
        [FieldOffset(16)] public delegate*unmanaged<FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*, FFXIVClientStructs.FFXIV.Component.GUI.AtkEventType,uint,void*,uint*,long> vReceiveEvent;
        [FieldOffset(128)] public delegate*unmanaged<FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*, byte> vOnClose;
        [FieldOffset(328)] public delegate*unmanaged<FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*, void> vOnDraw;
        [FieldOffset(368)] public delegate*unmanaged<FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*, int,FFXIVClientStructs.FFXIV.Component.GUI.AtkValue*,void> vOnSetup;
        [FieldOffset(344)] public delegate*unmanaged<FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*, byte> vLoadUldResourceHandle;
    }

    [UnmanagedCallersOnly]
    private static long VReceiveGlobalEvent(FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase* unitBase,FFXIVClientStructs.FFXIV.Component.GUI.AtkEventType a2,uint a3,void* a4,uint* a5) {
        if (TryGet(unitBase, out var addon) && addon != null) {
            return addon.ReceiveGlobalEvent(a2,a3,a4,a5);
            
        }
        return _atkUnitBase->vReceiveGlobalEvent(unitBase,a2,a3,a4,a5);
    }

    
    
    protected virtual partial long ReceiveGlobalEvent(FFXIVClientStructs.FFXIV.Component.GUI.AtkEventType a2,uint a3,void* a4,uint* a5) {

        return original->vReceiveGlobalEvent(this.AtkUnitBase,a2,a3,a4,a5);
    }
    
    [UnmanagedCallersOnly]
    private static long VReceiveEvent(FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase* unitBase,FFXIVClientStructs.FFXIV.Component.GUI.AtkEventType a2,uint a3,void* a4,uint* a5) {
        if (TryGet(unitBase, out var addon) && addon != null) {
            return addon.ReceiveEvent(a2,a3,a4,a5);
            
        }
        return _atkUnitBase->vReceiveEvent(unitBase,a2,a3,a4,a5);
    }

    
    
    protected virtual partial long ReceiveEvent(FFXIVClientStructs.FFXIV.Component.GUI.AtkEventType a2,uint a3,void* a4,uint* a5) {

        return original->vReceiveEvent(this.AtkUnitBase,a2,a3,a4,a5);
    }
    
    [UnmanagedCallersOnly]
    private static byte VOnClose(FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase* unitBase) {
        if (TryGet(unitBase, out var addon) && addon != null) {
            return addon.OnClose() ? (byte) 1 : (byte) 0;
            
        }
        return _atkUnitBase->vOnClose(unitBase);
    }

    
    [UnmanagedCallersOnly]
    private static void VOnDraw(FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase* unitBase) {
        if (TryGet(unitBase, out var addon) && addon != null) {
            addon.OnDraw();
            return;
        }
        _atkUnitBase->vOnDraw(unitBase);
    }

    
    
    protected virtual partial void OnDraw() {

        original->vOnDraw(this.AtkUnitBase);
    }
    
    [UnmanagedCallersOnly]
    private static void VOnSetup(FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase* unitBase,int atkValueCount,FFXIVClientStructs.FFXIV.Component.GUI.AtkValue* atkValues) {
        if (TryGet(unitBase, out var addon) && addon != null) {
            addon.OnSetup(atkValueCount,atkValues);
            return;
        }
        _atkUnitBase->vOnSetup(unitBase,atkValueCount,atkValues);
    }

    
    [UnmanagedCallersOnly]
    private static byte VLoadUldResourceHandle(FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase* unitBase) {
        if (TryGet(unitBase, out var addon) && addon != null) {
            return addon.LoadUldResourceHandle() ? (byte) 1 : (byte) 0;
            
        }
        return _atkUnitBase->vLoadUldResourceHandle(unitBase);
    }

    

    private partial void ReplaceVirtualTable(VirtualTable* newTable) {
        newTable->vReceiveGlobalEvent = &VReceiveGlobalEvent;
        newTable->vReceiveEvent = &VReceiveEvent;
        newTable->vOnClose = &VOnClose;
        newTable->vOnDraw = &VOnDraw;
        newTable->vOnSetup = &VOnSetup;
        newTable->vLoadUldResourceHandle = &VLoadUldResourceHandle;
    }
}
