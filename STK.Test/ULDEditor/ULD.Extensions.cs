using ULD;
using ULD.Component;
using ULD.Node;
using ULD.Node.Component;

namespace STK.Test.ULDEditor; 

public static class ULDExtensions {

    public class EncodableReference {
        public IEncodable Reference { get; init; }
        public string Description { get; init; }

        public override bool Equals(object? obj) {
            if (obj is not EncodableReference other) return false;
            return other.Reference == Reference;
        }
    }
    
    
    public static HashSet<EncodableReference> GetReferences(this IEncodable encodable, ULD.Uld uld) {
        var references = new HashSet<EncodableReference>();

        for (var atkI = 0; atkI < uld.ATK.Length; atkI++) {
            var atk = uld.ATK[atkI];
            if (atk == null) continue;
            if (encodable is ComponentBase component) {
                if (atk.Components != null) {
                    foreach (var c in atk.Components.Elements) {
                        if (c.Id == component.Id) continue; // skip self
                        foreach (var n in ResNode.Collapse(c.RootNode)) {
                            if (n is not BaseComponentNode cn) continue;
                            if (cn.Type == (NodeType)component.Id) {
                                references.Add(new EncodableReference {
                                    Reference = n,
                                    Description = $"ATK[{atkI}] Component[{c.Id}] Node[{n.Id}]"
                                });
                            }
                        }
                    }
                }
                
                if (atk.Widgets != null) {
                    foreach (var w in atk.Widgets.Elements) {
                        if (w.Id == component.Id) continue; // skip self
                        foreach (var n in ResNode.Collapse(w.RootNode)) {
                            if (n is not BaseComponentNode cn) continue;
                            if (cn.Type == (NodeType)component.Id) {
                                references.Add(new EncodableReference {
                                    Reference = n,
                                    Description = $"ATK[{atkI}] Widget[{w.Id}] Node[{n.Id}]"
                                });
                            }
                        }
                    }
                }
               
                
                
            }
        }
        
        
        

        return references;
    }
    
}
