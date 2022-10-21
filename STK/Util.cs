using System.Diagnostics;
using System.Text;

namespace STK; 

internal static unsafe class Util {


    internal static void WriteString(byte* ptr, string? value, Encoding? encoding = null) {
        encoding ??= Encoding.UTF8;
        value ??= string.Empty;
        var bytes = encoding.GetBytes(value + "\0");
        WriteRaw(ptr, bytes);
    }

    internal static void WriteRaw(byte* ptr, byte[] bytes) {
        for (var i = 0; i < bytes.Length; i++) *(ptr + i) = bytes[i];
    }


    
    
    private static ulong beginModule = 0;
    private static ulong endModule = 0;
    internal static long OffsetFromBase(ulong address) {

        try {
            if (endModule == 0 && beginModule == 0) {
                try {
                    beginModule = (ulong)(Process.GetCurrentProcess().MainModule?.BaseAddress ?? IntPtr.Zero).ToInt64();
                    endModule = beginModule + (ulong)(Process.GetCurrentProcess().MainModule?.ModuleMemorySize ?? 0);
                } catch {
                    endModule = 1;
                }
            }
        } catch {
            //
        }


        if (address < beginModule) {
            var diff = beginModule - address;
            if (diff > long.MaxValue) return long.MinValue;
            return -(long)diff;
        } else {
            var diff = address - beginModule;
            if (diff > long.MaxValue) return long.MaxValue;
            return (long)diff;
        }
    }
    
    
}
