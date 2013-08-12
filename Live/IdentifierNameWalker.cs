using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Live
{
    public class IdentifierNameWalker : SyntaxWalker
    {

        public IdentifierNameWalker()
        {
            this.FoundIdentifierNames = new List<IdentifierNameSyntax>();
        }

        public List<IdentifierNameSyntax> FoundIdentifierNames { get; private set; }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            this.FoundIdentifierNames.Add(node);
        }
    }
}
