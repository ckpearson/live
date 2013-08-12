using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Roslyn.Compilers.CSharp;
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
            var walker = new VariableWalker(sem);
            walker.Visit(m_methodSyntax);
            if (walker.FoundVariables.Any())
            {
                var names = walker.FoundVariables.Aggregate<VariableDeclaratorSyntax, string>("",
                    (s, dec) =>
                    {
                        return s + dec.Identifier.ValueText + ",";
                    });

                MessageBox.Show("Local variables: " + names.TrimEnd(','));
            }
        }

        public bool IsEnabled
        {
            get { return true; }
        }
    }
}
