using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Live
{
    public class VariableWalker : SyntaxWalker
    {
        private SemanticModel m_model;

        public VariableWalker(SemanticModel model)
        {
            m_model = model;
            this.FoundVariables = new List<VariableDeclaratorSyntax>();
        }

        public List<VariableDeclaratorSyntax> FoundVariables { get; private set; }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            this.FoundVariables.Add(node);
        }
    }
}
