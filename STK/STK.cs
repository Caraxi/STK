using System.Diagnostics;
using FFXIVClientStructs;

namespace STK; 

public static class STK {

    public static bool Initalized { get; private set; }

    internal static SigScanner Scanner { get; private set; } = null!;
    
    public static void Initalize() {
        if (Initalized) return;
        var module = Process.GetCurrentProcess().MainModule;
        Scanner = new SigScanner(module);
        Initalized = true;
    }

    public static void Initalize(IntPtr moduleCopy) {
        if (Initalized) return;
        var module = Process.GetCurrentProcess().MainModule;
        Scanner = new SigScanner(module, moduleCopy);
        Initalized = true;
    }
    
    
    public static void Cleanup() {
        Addon.Cleanup();
    }
}
