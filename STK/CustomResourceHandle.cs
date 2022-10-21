using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace STK; 

public unsafe class CustomResourceHandle : IDisposable{

    [StructLayout(LayoutKind.Explicit, Size = 8 * FuncCount)]
    public struct VirtualTable {
        public const int FuncCount = 45;

        [FieldOffset(0x00)] public delegate*unmanaged<ResourceHandle*, byte, void*> dtor;
        [FieldOffset(0xB8)] public delegate*unmanaged<ResourceHandle*, byte*> getData;
    }

    public delegate bool GetCustomData(out byte[] data);
    
    private VirtualTable* original;
    private VirtualTable* customVirtualTable;

    private ResourceHandle* resourceHandle;

    private static readonly List<CustomResourceHandle> CustomResourceHandles = new();

    public bool IsDisposed { get; private set; } = false;


    private Dictionary<string, GetCustomData> fileNames;
    private Dictionary<string, IntPtr> allocatedData = new();


    public CustomResourceHandle(ResourceHandle* resourceHandle, Dictionary<string, GetCustomData> fileNames) {
        this.fileNames = fileNames;
        Logging.Log("Setting Up Custom Resource Handle");
        this.resourceHandle = resourceHandle;
        ReplaceVirtualTable();
        CustomResourceHandles.Add(this);
    }
    
    private void ReplaceVirtualTable() {
        
        customVirtualTable = (VirtualTable*) IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(VirtualTable), 8UL);
        original = (VirtualTable*)resourceHandle->vtbl;

        var o = (byte*)(original);
        var n = (byte*)(customVirtualTable);
        for (var u = 0UL; u < (ulong) sizeof(VirtualTable); u++) *(n + u) = *(o + u);


        customVirtualTable->dtor = &Destroy;
        customVirtualTable->getData = &GetData;
        resourceHandle->vtbl = customVirtualTable;
        Logging.Log("Replaced Virtual Table");
    }

    private static bool TryGet(ResourceHandle* resourceHandle, out CustomResourceHandle? customResourceHandle) {
        customResourceHandle = CustomResourceHandles.FirstOrDefault(c => c.resourceHandle == resourceHandle);
        return customResourceHandle != null && customResourceHandle.resourceHandle != null && !customResourceHandle.IsDisposed;
    }

    [UnmanagedCallersOnly]
    public static void* Destroy(ResourceHandle* resourceHandle, byte free) {
        Logging.Log("Destroy");
        if (TryGet(resourceHandle, out var crh) && crh != null) {
            return crh.Destroy(free);
        }
        return null;
    }

    public void* Destroy(byte free) {
        Logging.Log("Destroying Custom Resource Handle");
        return original->dtor(resourceHandle, free);
    }
    
    [UnmanagedCallersOnly]
    public static byte* GetData(ResourceHandle* resourceHandle) {
        if (TryGet(resourceHandle, out var crh) && crh != null) {
            return crh.GetData();
        }
        return null;
    }
    
    public byte* GetData() {
        var fileName = this.resourceHandle->FileName.ToString();
        Logging.Log($"Getting Data for CustomResourceHandle : {fileName}");

        if (this.allocatedData.ContainsKey(fileName)) {
            
            // return (byte*) allocatedData[fileName].ToPointer();
        }
        
        
        if (this.fileNames.ContainsKey(fileName)) {
            Logging.Log("Use Custom GetData");
            if (this.fileNames[fileName](out var bytes)) {
                var alloc = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, alloc, bytes.Length);
                // allocatedData.Add(fileName, alloc);
                return (byte*) alloc.ToPointer();
            } else {
                Logging.Log("Custom Data returned false.");
            }
        }
        return original->getData(resourceHandle);
    }

    public void* GetOriginalData() {
        return original->getData(resourceHandle);
    }
    
    public void Dispose() {
        if (IsDisposed) return;
        if (this.resourceHandle != null) {
            this.resourceHandle->vtbl = original;
        }
        CustomResourceHandles.Remove(this);
        IsDisposed = true;
    }
    
    
}
