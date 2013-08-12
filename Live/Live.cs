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
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Live
{
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
            return null;
        }
    }
}
