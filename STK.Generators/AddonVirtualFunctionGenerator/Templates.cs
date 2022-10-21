namespace STK.Generators.AddonVirtualFunctionGenerator {
    
    internal static class Templates {
        internal const string Template = @"using System;
using System.Runtime.InteropServices;

namespace {{ struct.namespace }};

public unsafe partial class {{ struct.name }} {
    public partial struct VirtualTable {
        {{~ for vf in struct.virtual_functions ~}}
        [FieldOffset({{ vf.virtual_offset * 8 }})] public delegate*unmanaged<FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*, {{ if vf.has_params }}{{ vf.param_type_list}},{{ end }}{{ if vf.has_bool_return }}byte{{ else }}{{ vf.return_type }}{{ end }}> v{{vf.name }};
        {{~ end ~}}
    }

    {{~ for vf in struct.virtual_functions ~}}
    [UnmanagedCallersOnly]
    private static {{ if vf.has_bool_return }}byte{{ else }}{{ vf.return_type }}{{ end }} V{{vf.name}}(FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase* unitBase{{ if vf.has_params }},{{vf.param_list}}{{ end }}) {
        if (TryGet(unitBase, out var addon) && addon != null) {
            {{ if vf.has_return }}return {{ end }}addon.{{ vf.name }}({{ vf.param_name_list }}){{ if vf.has_bool_return }} ? (byte) 1 : (byte) 0{{ end }};
            {{ if !vf.has_return }}return;{{ end }}
        }
        {{ if vf.has_return }}return {{ end }}_atkUnitBase->v{{vf.name}}(unitBase{{ if vf.has_params }},{{ vf.param_name_list }}{{ end }});
    }

    {{ if vf.is_partial }}
    
    protected virtual partial {{ vf.return_type }} {{ vf.name }}({{ vf.param_list }}) {

        {{ if vf.has_return }}return {{ end }}original->v{{vf.name}}(this.AtkUnitBase{{ if vf.has_params }},{{ vf.param_name_list }}{{ end }});
    }
    {{ end }}
    {{~ end ~}}

    private partial void ReplaceVirtualTable(VirtualTable* newTable) {
        {{~ for vf in struct.virtual_functions ~}}
        newTable->v{{ vf.name }} = &V{{ vf.name }};
        {{~ end ~}}
    }
}
";
    }
    
}
