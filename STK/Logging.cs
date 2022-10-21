namespace STK;

public static class Logging {
    private static readonly Action<object> NoAction = _ => { };
    public static Action<object> Log { internal get; set; } = NoAction;
    public static Action<object> Verbose { internal get; set; } = NoAction;


}
