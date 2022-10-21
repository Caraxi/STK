using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace STK.Generators.AddonVirtualFunctionGenerator {
    internal class ContextReceiver : ISyntaxContextReceiver {

        public List<Struct> Structs { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
            if (context.Node is not ClassDeclarationSyntax sds) return;

            var methods = sds.ChildNodes().OfType<MethodDeclarationSyntax>().Where(m =>
            {
                var sm = (IMethodSymbol) context.SemanticModel.GetDeclaredSymbol(m);
                return sm != null && sm.GetAttributes()
                    .Any(a => a.AttributeClass?.Name is "VirtualFunctionAttribute");
            }).ToList();

            if (methods.Count <= 0) return;
            
            if (context.SemanticModel.GetDeclaredSymbol(sds) is not INamedTypeSymbol structType) return;

            // if (structType.Name != "Addon") return;
            
            var structObj = new Struct
            {
                Name = structType.Name,
                Namespace = structType.ContainingNamespace.ToDisplayString(),
                VirtualFunctions = new List<Function>(),
            };
            
            foreach (var m in methods) {
                if (context.SemanticModel.GetDeclaredSymbol(m) is not IMethodSymbol ms) continue;
                var format = new SymbolDisplayFormat(
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
                var functionObj = new Function
                {
                    Name = ms.Name,
                    ReturnType = ms.ReturnType.ToDisplayString(format),
                    HasBoolReturn = ms.ReturnType.ToDisplayString() == "bool",
                    HasReturn = ms.ReturnType.ToDisplayString() != "void",
                    IsPartial = ms.IsPartialDefinition,
                    HasParams = ms.Parameters.Any(),
                    ParamList = string.Join(",",
                        ms.Parameters.Select(p => $"{p.Type.ToDisplayString(format)} {p.Name}")),
                    ParamTypeList = string.Join(",", ms.Parameters.Select(p => p.Type.ToDisplayString(format))),
                    ParamNameList = string.Join(",", ms.Parameters.Select(p => p.Name))
                };

                if (ms.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "VirtualFunctionAttribute") is { } virtualFuncAttr) {
                    functionObj.VirtualOffset = (int)(virtualFuncAttr.ConstructorArguments[0].Value ?? 0);
                    structObj.VirtualFunctions.Add(functionObj);
                }
            }

            Structs.Add(structObj);
            
            
            
            
        }



    }
}
