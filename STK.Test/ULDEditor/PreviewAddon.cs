using SimpleTweaksPlugin;
using ULD;

namespace STK.Test.ULDEditor; 

public class PreviewAddon : Addon {

    private Uld uld;
    
    public PreviewAddon(Uld uldFile) : base("ULD.Editor.Preview") {
        this.uld = uldFile;
    }
    
    public override bool GetUldData(out byte[] data) {
        try {
            data = uld.Encode();
            return true;
        } catch (Exception ex) {
            SimpleLog.Error(ex);
            data = Array.Empty<byte>();
            return false;
        }
    }
    
}
