using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Live
{
    public class MethodSmartTag : SmartTag
    {
        public MethodSmartTag(ReadOnlyCollection<SmartTagActionSet> actionSets)
            : base(SmartTagType.Factoid, actionSets)
        {

        }
    }
}
