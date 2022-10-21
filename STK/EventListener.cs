using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace STK; 

public unsafe class EventListener : IDisposable {

    public static implicit operator AtkEventListener*(EventListener listener) => listener.Listener;
    
    public delegate void* DtorDelegate(AtkEventListener* listener, byte a2);
    
    private ReceiveEvent eventHandler = NoEventHandle;
    private ReceiveEvent globalEventHandler = NoEventHandle;
    private DtorDelegate dtor;
    
    private IntPtr eventHandlerPtr;
    private IntPtr globalEventHandlerPtr;
    private IntPtr dtorPtr;
    
    public AtkEventListener* Listener { get; private set; }

    private EventListener() {
        this.dtor = Dtor;
    }
    
    public EventListener(ReceiveEventTypeOnly eventHandler): this() {
        Setup((_, type, _, _, _) => eventHandler(type));
    }
    
    public EventListener(ReceiveEventTypeOnlyNoReturn eventHandler): this() {
        Setup((_, type, _, _, _) => {
            eventHandler(type);
            return 1;
        });
    }
    
    public EventListener(ReceiveEventTypeAndParam eventHandler): this() {
        Setup((_, type, param, _, _) => eventHandler(type, param));
    }
    
    public EventListener(ReceiveEventTypeAndParamNoReturn eventHandler): this() {
        Setup((_, type, param, _, _) => {
            eventHandler(type, param);
            return 1;
        });
    }
    
    public EventListener(ReceiveEvent eventHandler, ReceiveEvent? globalEventHandler = null) : this() {
        Setup(eventHandler, globalEventHandler);
    }

    private void Setup(ReceiveEvent eventHandler, ReceiveEvent? globalEventHandler = null) {
        this.eventHandler = eventHandler;
        this.globalEventHandler = globalEventHandler ?? NoEventHandle;
        this.dtor = Dtor;
        
        Listener = (AtkEventListener*)IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(AtkEventListener), 8);
        Listener->vfunc = (void**) IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(void*) * 3, 8);

        eventHandlerPtr = Marshal.GetFunctionPointerForDelegate(this.eventHandler);
        globalEventHandlerPtr = Marshal.GetFunctionPointerForDelegate(this.globalEventHandler);
        dtorPtr = Marshal.GetFunctionPointerForDelegate(this.dtor);

        Listener->vfunc[0] = (void*)dtorPtr;
        Listener->vfunc[1] = (void*)globalEventHandlerPtr;
        Listener->vfunc[2] = (void*)eventHandlerPtr;
    }

    private void* Dtor(AtkEventListener* listener, byte a1) {
        Dispose();
        return null;
    }
    
    private static byte NoEventHandle(AtkEventListener* listener, AtkEventType eventType, uint eventParam, void* eventData, void* a5) => 0;

    public void Dispose() {
        IMemorySpace.Free(Listener->vfunc, (ulong) sizeof(void*) * 3);
        IMemorySpace.Free(Listener, (ulong) sizeof(AtkEventListener));
    }
}
