using FFXIVClientStructs.FFXIV.Component.GUI;

namespace STK; 


public unsafe delegate byte AddonReceiveEvent(AtkUnitBase* atkUnitBase, ushort eventType, uint eventParam, void* eventData, void* a5);


public unsafe delegate byte ReceiveEvent(AtkEventListener* listener, AtkEventType eventType, uint eventParam, void* eventData, void* a5);
public delegate byte ReceiveEventTypeAndParam(AtkEventType eventType, uint eventParam);
public delegate byte ReceiveEventTypeOnly(AtkEventType eventType);

public delegate void ReceiveEventTypeAndParamNoReturn(AtkEventType eventType, uint eventParam);
public delegate void ReceiveEventTypeOnlyNoReturn(AtkEventType eventType);