using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace STK.Generators.AddonVirtualFunctionGenerator {
    
    
    [Generator]
    public class VirtualFunctionGenerator : ISourceGenerator{
        
        private Template template;

        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new ContextReceiver());

            template = Template.Parse(Templates.Template);
        }

        public void Execute(GeneratorExecutionContext context) {
            if (context.SyntaxContextReceiver is not ContextReceiver receiver) return;

            foreach (var structObj in receiver.Structs) {
                if (structObj.VirtualFunctions.Any()) {
                    
                    var filename = structObj.Namespace + "." + structObj.Name + ".VirtualFunctions.generated.cs";
                    var source = template.Render(new { Struct = structObj });
                    context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
                }
            }
            
            
            
            
            
        }
    }
}
