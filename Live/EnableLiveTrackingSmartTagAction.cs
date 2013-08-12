using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Live
{
    public class EnableLiveTrackingSmartTagAction : ISmartTagAction
    {
        private ITrackingSpan m_identSpan;
        private ITextSnapshot m_snapshot;
        private MethodDeclarationSyntax m_methodSyntax;

        public EnableLiveTrackingSmartTagAction(ITrackingSpan identSpan, MethodDeclarationSyntax methodSyntax)
        {
            m_identSpan = identSpan;
            m_snapshot = m_identSpan.TextBuffer.CurrentSnapshot;
            m_methodSyntax = methodSyntax;
        }

        public ReadOnlyCollection<SmartTagActionSet> ActionSets
        {
            get { return null; }
        }

        public string DisplayText
        {
            get { return "Enable Live Tracking For " + m_methodSyntax.Identifier.ValueText; }
        }

        public System.Windows.Media.ImageSource Icon
        {
            get { return null; }
        }

        public void Invoke()
        {
            var comp = Compilation.Create("meth", syntaxTrees: new[] { m_methodSyntax.SyntaxTree });
            var sem = comp.GetSemanticModel(m_methodSyntax.SyntaxTree);
            if (m_methodSyntax.Modifiers.Any(s => s.Kind == SyntaxKind.StaticKeyword))
            {
                MessageBox.Show("Cannot enable live coding for static methods");
                return;
            }
            var varwalker = new VariableWalker(sem);
            var identwalker = new IdentifierNameWalker();
            varwalker.Visit(m_methodSyntax.Body);
            identwalker.Visit(m_methodSyntax.Body);

            List<IdentifierNameSyntax> nonlocalrefs = new List<IdentifierNameSyntax>();
            foreach (var ident in identwalker.FoundIdentifierNames)
            {
                if (!nonlocalrefs.Any(i => i.Identifier.ValueText == ident.Identifier.ValueText))
                {
                    if (varwalker.FoundVariables.Count(v => v.Identifier.ValueText == ident.Identifier.ValueText) == 0)
                    {
                        nonlocalrefs.Add(ident);
                    }
                }
            }

            if (nonlocalrefs.Count > 0)
            {
                string refs = nonlocalrefs.Select(i => i.Identifier.ValueText)
                    .Aggregate<string, string>("", (n, s) => s + n + ",").TrimEnd(',');

                MessageBox.Show(string.Format("Live-coding not supported for method, non-local references('{0}') are not supported", refs));
                return;
            }

            if (m_methodSyntax.ParameterList.Parameters.Count > 0)
            {
                if (m_methodSyntax.ParameterList.Parameters.Any(p => p.Default == null))
                {
                    MessageBox.Show("Live-coding not supported for method, parameters must have default values");
                    return;
                }

                if (m_methodSyntax.ParameterList.Parameters.Any(p => p.Default != null &&
                    ((EqualsValueClauseSyntax)p.Default).Value.Kind == SyntaxKind.IdentifierName))
                {
                    MessageBox.Show("Live-coding not supported for method, parameter default values must be literals");
                    return;
                }
            }

            //var walker = new VariableWalker(sem);
            //walker.Visit(m_methodSyntax);
            //if (walker.FoundVariables.Any())
            //{
            //    var names = walker.FoundVariables.Aggregate<VariableDeclaratorSyntax, string>("",
            //        (s, dec) =>
            //        {
            //            return s + dec.Identifier.ValueText + ",";
            //        });

            //    MessageBox.Show("Local variables: " + names.TrimEnd(','));
            //}
            //var methodCode = m_methodSyntax.ToFullString();
            //ParseOptions options = ParseOptions.Default;
            //options = options.WithKind(Roslyn.Compilers.SourceCodeKind.Script);
            ////var submission = Compilation.CreateSubmission("sub", syntaxTree: SyntaxTree.ParseText(methodCode, options: options));
            //var engine = new ScriptEngine();
            //var session = engine.CreateSession();
            //var submission = session.CompileSubmission<object>(methodCode);
            //object res = submission.Execute();
        }

        public bool IsEnabled
        {
            get { return true; }
        }
    }
}
