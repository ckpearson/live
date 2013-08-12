using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Live
{
    public class MethodSmartTagTagger : ITagger<MethodSmartTag>, IDisposable
    {
        private ITextBuffer m_buffer;
        private ITextView m_view;
        private MethodSmartTagTaggerProvider m_provider;
        private bool m_disposed;

        public MethodSmartTagTagger(
            ITextBuffer buffer,
            ITextView view,
            MethodSmartTagTaggerProvider provider)
        {
            m_buffer = buffer;
            m_view = view;
            m_provider = provider;
            m_view.LayoutChanged += OnLayoutChanged;
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    m_view.LayoutChanged -= OnLayoutChanged;
                    m_view = null;
                }

                m_disposed = true;
            }
        }

        #endregion

        public IEnumerable<ITagSpan<MethodSmartTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            ITextSnapshot snapshot = m_buffer.CurrentSnapshot;
            if (snapshot.Length == 0)
                yield break;

            ITextStructureNavigator navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_buffer);

            foreach (var span in spans)
            {
                var code = new string(span.Snapshot.ToCharArray(0, span.Snapshot.Length));

                var tree = SyntaxTree.ParseText(code);
                var ctoken = new CancellationToken();
                var comp = Compilation.Create("some", syntaxTrees: new[] { tree });
                var sem = comp.GetSemanticModel(tree);
                var methWalker = new MethodWalker(sem);
                methWalker.Visit(tree.GetRoot(ctoken));
                if (methWalker.FoundMethods.Any())
                {
                    foreach (var methodSyntax in methWalker.FoundMethods)
                    {
                        var ident = methodSyntax.Identifier;
                        TextExtent extent = navigator.GetExtentOfWord(new SnapshotPoint(snapshot, ident.Span.Start));
                        yield return new TagSpan<MethodSmartTag>(extent.Span, new MethodSmartTag(GetSmartTagActions(extent.Span, methodSyntax)));
                    }
                }
            }
        }

        private ReadOnlyCollection<SmartTagActionSet> GetSmartTagActions(SnapshotSpan span, MethodDeclarationSyntax method)
        {
            List<SmartTagActionSet> actionSetList = new List<SmartTagActionSet>();
            List<ISmartTagAction> actionList = new List<ISmartTagAction>();

            ITrackingSpan trackingSpan = span.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);

            //actionList.Add(new UpperCaseSmartTagAction(trackingSpan));
            //actionList.Add(new LowerCaseSmartTagAction(trackingSpan));

            actionList.Add(new EnableLiveTrackingSmartTagAction(trackingSpan, method));

            SmartTagActionSet actionSet = new SmartTagActionSet(actionList.AsReadOnly());
            actionSetList.Add(actionSet);
            return actionSetList.AsReadOnly();
        }

        public event EventHandler<Microsoft.VisualStudio.Text.SnapshotSpanEventArgs> TagsChanged;

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            ITextSnapshot snapshot = e.NewSnapshot;
            SnapshotSpan span = new SnapshotSpan(snapshot, new Span(0, snapshot.Length));
            if (this.TagsChanged != null)
            {
                this.TagsChanged(this, new SnapshotSpanEventArgs(span));
            }
        }
    }
}
