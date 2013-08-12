using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Live
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [Order(Before = "default")]
    [TagType(typeof(MethodSmartTag))]
    public class MethodSmartTagTaggerProvider : IViewTaggerProvider
    {
        [Import(typeof(ITextStructureNavigatorSelectorService))]
        public ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (buffer == null || textView == null)
            {
                return null;
            }

            if (buffer == textView.TextBuffer)
            {
                return new MethodSmartTagTagger(buffer, textView, this) as ITagger<T>;
            }
            else
            {
                return null;
            }
        }
    }
}
