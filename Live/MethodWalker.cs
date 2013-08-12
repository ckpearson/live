using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Live
{
    public class MethodWalker : SyntaxWalker
    {
        private SemanticModel m_model;

        public MethodWalker(SemanticModel model)
        {
            m_model = model;
            this.FoundMethods = new List<MethodDeclarationSyntax>();
        }

        public List<MethodDeclarationSyntax> FoundMethods { get; private set; }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            this.FoundMethods.Add(node);
        }
    }
}
