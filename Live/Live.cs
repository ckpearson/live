using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Live
{
    public class TestSmartTag : SmartTag
    {
        public TestSmartTag(ReadOnlyCollection<SmartTagActionSet> actionSets)
            : base(SmartTagType.Factoid, actionSets)
        {

        }
    }

    public class UpperCaseSmartTagAction : ISmartTagAction
    {
        private ITrackingSpan m_span;
        private string m_upper;
        private string m_display;
        private ITextSnapshot m_snapshot;

        public UpperCaseSmartTagAction(ITrackingSpan span)
        {
            m_span = span;
            m_snapshot = span.TextBuffer.CurrentSnapshot;
            m_upper = span.GetText(m_snapshot).ToUpper();
            m_display = "Convert to upper case";
        }

        public ReadOnlyCollection<SmartTagActionSet> ActionSets
        {
            get { return null; ; }
        }

        public string DisplayText
        {
            get { return m_display; ; }
        }

        public System.Windows.Media.ImageSource Icon
        {
            get { return null; ; }
        }

        public void Invoke()
        {
            m_span.TextBuffer.Replace(m_span.GetSpan(m_snapshot), m_upper);
        }

        public bool IsEnabled
        {
            get { return true; }
        }
    }

    public class LowerCaseSmartTagAction : ISmartTagAction
    {
        private ITrackingSpan m_span;
        private string m_lower;
        private string m_display;
        private ITextSnapshot m_snapshot;

        public LowerCaseSmartTagAction(ITrackingSpan span)
        {
            m_span = span;
            m_snapshot = span.TextBuffer.CurrentSnapshot;
            m_lower = span.GetText(m_snapshot).ToLower();
            m_display = "Convert to lower case";
        }

        public ReadOnlyCollection<SmartTagActionSet> ActionSets
        {
            get { return null; ; }
        }

        public string DisplayText
        {
            get { return m_display; ; }
        }

        public System.Windows.Media.ImageSource Icon
        {
            get { return null; ; }
        }

        public void Invoke()
        {
            m_span.TextBuffer.Replace(m_span.GetSpan(m_snapshot), m_lower);
        }

        public bool IsEnabled
        {
            get { return true; }
        }
    }

    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [Order(Before = "default")]
    [TagType(typeof(SmartTag))]
    public class TestSmartTaggerProvider : IViewTaggerProvider
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
                return new TestSmartTagTagger(buffer, textView, this) as ITagger<T>;
            }
            else
            {
                return null;
            }
        }
    }

    public class TodoTagGlyphFactory : IGlyphFactory
    {
        const double m_glyphSize = 16.0;

        public System.Windows.UIElement GenerateGlyph(Microsoft.VisualStudio.Text.Formatting.IWpfTextViewLine line, IGlyphTag tag)
        {
            if (tag == null || !(tag is TodoTag))
            {
                return null;
            }

            //var tg = (TodoTag)tag;

            //var syn = tg._roslynLocalSymbol.DeclaringSyntaxNodes.First() as VariableDeclaratorSyntax ;
            //if (syn == null)
            //    return null;

            //var dec = syn.ChildNodes();

            var el = new Ellipse();
            el.Fill = Brushes.LightBlue;
            el.StrokeThickness = 2;
            el.Stroke = Brushes.DarkBlue;
            el.Height = m_glyphSize;
            el.Width = m_glyphSize;

            return el;
        }
    }

    [Export(typeof(IGlyphFactoryProvider))]
    [Name("TodoGlyph")]
    [Order(After= "VsTextMarker")]
    [ContentType("code")]
    [TagType(typeof(TodoTag))]
    public class TagGlyphFactoryProvider : IGlyphFactoryProvider
    {
        public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
        {
            return new TodoTagGlyphFactory();
        }
    }

    public class TodoTag : IGlyphTag
    {
        public readonly Symbol _roslynLocalSymbol;

        public TodoTag(Symbol symbol)
        {
            _roslynLocalSymbol = symbol;
        }
    }

    [Export(typeof(ITaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(TodoTag))]
    public class TodoTaggerProvider : ITaggerProvider
    {

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new TestSmartTagTagger(buffer, null, null) as ITagger<T>;
        }
    }




    public class TestSmartTagTagger : ITagger<TestSmartTag>, IDisposable, ITagger<TodoTag>
    {
        private ITextBuffer m_buffer;
        private ITextView m_view;
        private TestSmartTaggerProvider m_provider;
        private bool m_disposed;

        public TestSmartTagTagger(ITextBuffer buffer, ITextView view, TestSmartTaggerProvider provider)
        {
            m_buffer = buffer;
            m_view = view;
            m_provider = provider;
            if (m_view != null)
             m_view.LayoutChanged += OnLayoutChanged;
        }

        // Searches for method declarations given a root node
        private IEnumerable<MethodDeclarationSyntax> GetMethodNode(CommonSyntaxNode node)
        {
            var children = node.ChildNodes();
            foreach (var child in children)
            {
                if (child as MethodDeclarationSyntax != null)
                {
                    // This is a method declaration
                    yield return (MethodDeclarationSyntax)child;
                }

                // Return all the further declarations found down from this node and recursively until end of the branch
                var res = GetMethodNode(child);
                foreach (var c in res)
                {
                    yield return c;
                }
            }
        }

        public IEnumerable<ITagSpan<TestSmartTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            ITextSnapshot snapshot = m_buffer.CurrentSnapshot;
            if (snapshot.Length == 0)
                yield break;

            ITextStructureNavigator navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_buffer);

            foreach (var span in spans)
            {
                // Grab the code from vstudio editor
                var code = new string(span.Snapshot.ToCharArray(0, snapshot.Length));

                // Parse syntax into roslyn
                var tree = SyntaxTree.ParseText(code);


                var ctoken = new CancellationToken();   // Not used, but roslyn likes them
                var root = (CommonSyntaxNode)tree.GetRoot(ctoken);  // Grab root node (usually the first using statement)

                // Pull out all the method declarations roslyn could find
                var methods = GetMethodNode(root);

                foreach (var meth in methods)
                {
                    // Create compilation unit to access semantic model
                    var comp = Compilation.Create("meth", syntaxTrees: new[] { meth.SyntaxTree });
                    var model = comp.GetSemanticModel(meth.SyntaxTree);

                    // Set up dummy walker which just pulls out variables
                    var walker = new Walker(model);
                    walker.Visit(meth); // Visit the method
                    if (walker.Results.Any())
                    {
                        foreach (var local in walker.Results)
                        {
                            // Put a smart tag on the local variable identifiers
                            var name = local.Name;
                            var ext = navigator.GetExtentOfWord(new SnapshotPoint(snapshot, local.DeclaringSyntaxNodes.First().Span.Start));
                            yield return new TagSpan<TestSmartTag>(ext.Span, new TestSmartTag(GetSmartTagActions(ext.Span)));
                        }
                    }
                    
                    // Put a smart tag on the method identifiers
                    var ident = meth.Identifier;
                    TextExtent extent = navigator.GetExtentOfWord(new SnapshotPoint(snapshot, ident.Span.Start));
                    yield return new TagSpan<TestSmartTag>(extent.Span, new TestSmartTag(GetSmartTagActions(extent.Span)));
                }

                // DEMO CODE THAT CAME WITH TEMPLATE
                //ITextCaret caret = m_view.Caret;
                //SnapshotPoint point;
                //if (caret.Position.BufferPosition > 0)
                //    point = caret.Position.BufferPosition - 1;
                //else
                //    yield break;

                //TextExtent extent = navigator.GetExtentOfWord(point);
                //if (extent.IsSignificant)
                //    yield return new TagSpan<TestSmartTag>(extent.Span, new TestSmartTag(GetSmartTagActions(extent.Span)));
                //else yield break;
            }
        }

        public class Walker : SyntaxWalker
        {
            private SemanticModel m_model;

            public Walker(SemanticModel model)
            {
                m_model = model;
                Results = new List<Symbol>();
            }

            public List<Symbol> Results { get; private set; }

            public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                Results.Add(m_model.GetDeclaredSymbol(node));
            }
        }

        private ReadOnlyCollection<SmartTagActionSet> GetSmartTagActions(SnapshotSpan span)
        {
            List<SmartTagActionSet> actionSetList = new List<SmartTagActionSet>();
            List<ISmartTagAction> actionList = new List<ISmartTagAction>();

            ITrackingSpan trackingSpan = span.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);
            actionList.Add(new UpperCaseSmartTagAction(trackingSpan));
            actionList.Add(new LowerCaseSmartTagAction(trackingSpan));
            SmartTagActionSet actionSet = new SmartTagActionSet(actionList.AsReadOnly());
            actionSetList.Add(actionSet);
            return actionSetList.AsReadOnly();
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            ITextSnapshot snapshot = e.NewSnapshot;
            if (!snapshot.GetText().ToLower().Equals(e.OldSnapshot.GetText().ToLower()))
            {
                SnapshotSpan span = new SnapshotSpan(snapshot, new Span(0, snapshot.Length));
                EventHandler<SnapshotSpanEventArgs> handler = this.TagsChanged;
                if (handler != null)
                {
                    handler(this, new SnapshotSpanEventArgs(span));
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.m_disposed)
            {
                if (disposing)
                {
                    if (m_view != null)
                    {
                        m_view.LayoutChanged -= OnLayoutChanged;
                        m_view = null;
                    }
                }

                m_disposed = true;
            }
        }

        IEnumerable<ITagSpan<TodoTag>> ITagger<TodoTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            ITextSnapshot snapshot = m_buffer.CurrentSnapshot;
            if (snapshot.Length == 0)
                yield break;

            //ITextStructureNavigator navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_buffer);

            foreach (var span in spans)
            {
                // Grab the code from vstudio editor
                var code = new string(span.Snapshot.ToCharArray(0, snapshot.Length));

                // Parse syntax into roslyn
                var tree = SyntaxTree.ParseText(code);


                var ctoken = new CancellationToken();   // Not used, but roslyn likes them
                var root = (CommonSyntaxNode)tree.GetRoot(ctoken);  // Grab root node (usually the first using statement)

                // Pull out all the method declarations roslyn could find
                var methods = GetMethodNode(root);

                foreach (var meth in methods)
                {
                    // Create compilation unit to access semantic model
                    var comp = Compilation.Create("meth", syntaxTrees: new[] { meth.SyntaxTree });
                    var model = comp.GetSemanticModel(meth.SyntaxTree);

                    // Set up dummy walker which just pulls out variables
                    var walker = new Walker(model);
                    walker.Visit(meth); // Visit the method
                    if (walker.Results.Any())
                    {
                        foreach (var local in walker.Results)
                        {
                            // Put a smart tag on the local variable identifiers
                            var name = local.Name;
                            //var ext = navigator.GetExtentOfWord(new SnapshotPoint(snapshot, local.DeclaringSyntaxNodes.First().Span.Start));
                            //yield return new TagSpan<TestSmartTag>(ext.Span, new TestSmartTag(GetSmartTagActions(ext.Span)));
                            yield return new TagSpan<TodoTag>(new SnapshotSpan(snapshot, local.DeclaringSyntaxNodes.First().Span.Start, local.DeclaringSyntaxNodes.First().Span.Length), new TodoTag(local));
                        }
                    }

                    //// Put a smart tag on the method identifiers
                    //var ident = meth.Identifier;
                    //TextExtent extent = navigator.GetExtentOfWord(new SnapshotPoint(snapshot, ident.Span.Start));
                    //yield return new TagSpan<TestSmartTag>(extent.Span, new TestSmartTag(GetSmartTagActions(extent.Span)));
                }
            }
        }
    }
}
