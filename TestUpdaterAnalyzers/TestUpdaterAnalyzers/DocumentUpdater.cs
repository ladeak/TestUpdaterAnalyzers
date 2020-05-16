using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public class DocumentUpdater
    {
        private DocumentEditor _editor;
        private readonly Document _originalDoc;

        public DocumentUpdater(Document doc)
        {
            _originalDoc = doc;
        }

        public async Task Start(CancellationToken token = default)
        {
            _editor = await DocumentEditor.CreateAsync(_originalDoc, token);
        }

        public Document Complete() => _editor.GetChangedDocument();

        public void AddNSubstituteUsing()
        {
            CompilationUnitSyntax root = _editor.GetChangedRoot() as CompilationUnitSyntax;
            if (!root.Usings.Any(x => x.Name.GetText().ToString() == "NSubstitute"))
            {
                var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("NSubstitute"));
                _editor.InsertBefore((_editor.OriginalRoot as CompilationUnitSyntax).Usings.FirstOrDefault(), newUsing);
            }
        }

        public void AddNSubstituteExceptionExtensionsUsing()
        {
            CompilationUnitSyntax root = _editor.GetChangedRoot() as CompilationUnitSyntax;
            if (!root.Usings.Any(x => x.Name.GetText().ToString() == "NSubstitute.ExceptionExtensions"))
            {
                var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.QualifiedName(
                    SyntaxFactory.IdentifierName("NSubstitute"),
                    SyntaxFactory.IdentifierName("ExceptionExtensions")));
                _editor.InsertBefore((_editor.OriginalRoot as CompilationUnitSyntax).Usings.FirstOrDefault(), newUsing);
            }
        }
    }
}
