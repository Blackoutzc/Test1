using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Template
{
    class RichTextBoxSearch
    {
        private RichTextBox _richTextBoxInstance;
        private List<TextRange> _foundRanges;
        private TextRange _currentRange = null;
        private TextRange _lastRange = null;
        private Dictionary<TextRange, object> _rangeProperty;
       public int CurrentIndex
        {
            get;
            private set;
        }
        public string TextToBeFound
        {
            get;
            private set;
        }
        public int AllFoundNums => _foundRanges.Count;
        public RichTextBoxSearch(RichTextBox richTextBox)
        {
            _richTextBoxInstance = richTextBox;
            _foundRanges = new List<TextRange>();
            _rangeProperty = new Dictionary<TextRange, object>();
            CurrentIndex = -1;
        }
      
        public void Search(string targetText)
        {
            if (targetText?.Length > 0)
            {
                TextToBeFound = targetText;
                Reset();
                _FindAllTargetRanges();
            }
        }

        public void Reset()
        {
            _currentRange = null;
            _lastRange = null;
            _foundRanges.Clear();
            CurrentIndex = -1;
            _RestoreRangeProperty();
            _rangeProperty.Clear();
            
        }
        public void Next()
        {
            if (AllFoundNums > 0)
            {
                CurrentIndex++;
                CurrentIndex = CurrentIndex % AllFoundNums;
                _ColorCurrentRange();
               
            }
        }
        public void Pre()
        {
            if (AllFoundNums > 0)
            {
                CurrentIndex--;
                CurrentIndex = CurrentIndex < 0 ? AllFoundNums - 1 : CurrentIndex ;
                _ColorCurrentRange();
               
            }
        }
        private TextRange _FindTextWithinRange(TextRange searchRange, string searchText)
        {
            int offset = searchRange.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (offset < 0)
                return null;  // Not found

            var start = _GetTextPositionAtOffset(searchRange.Start, offset);
            TextRange result = new TextRange(start, _GetTextPositionAtOffset(start, searchText.Length));

            return result;
        }
        private TextPointer _GetTextPositionAtOffset(TextPointer position, int characterCount)
        {
            // Console.WriteLine($"Character count {characterCount}");
            while (position != null)
            {
                var type = position.GetPointerContext(LogicalDirection.Forward);
                string TypeName;
                //Console.WriteLine($"type:{type}");
                var AdjacentElementF = position.GetAdjacentElement(LogicalDirection.Forward);
                if (AdjacentElementF != null)
                {
                    TypeName = AdjacentElementF.GetType().Name;
                    //  Console.WriteLine($"element {TypeName}");
                }
                else
                {
                    TypeName = "";
                }

                if (type == TextPointerContext.Text)
                {
                    //Console.WriteLine("Text in run:" + position.GetTextInRun(LogicalDirection.Forward));
                    int count = position.GetTextRunLength(LogicalDirection.Forward);
                    if (characterCount <= count)
                    {
                        return position.GetPositionAtOffset(characterCount);
                    }
                    characterCount -= count;
                }
                if (type == TextPointerContext.ElementEnd && (TypeName == "Paragraph" || TypeName == "LineBreak"))
                {
                    //Console.WriteLine("Enter Count -- ");
                    characterCount -= 2;
                }
               
                TextPointer nextContextPosition = position.GetNextContextPosition(LogicalDirection.Forward);
                if (nextContextPosition == null)
                    return position;

                position = nextContextPosition;
            }

            return position;
        }
        // find all target ranges and preserve it into a list
        private void _FindAllTargetRanges()
        {
            TextPointer Current = _richTextBoxInstance.Document.ContentStart;
            while (Current != _richTextBoxInstance.Document.ContentEnd)
            {
                TextRange SearchRange = new TextRange(Current, _richTextBoxInstance.Document.ContentEnd);
                TextRange FoundRange = _FindTextWithinRange(SearchRange, TextToBeFound);
                if (FoundRange != null)
                {
                    if (FoundRange.Text == TextToBeFound)
                        _foundRanges.Add(FoundRange);
                    Current = FoundRange.Start.GetPositionAtOffset(1);
                }
                else
                {
                    return;
                }
               
            }
        }

        private void _CollectRangeProperties(TextRange TargetRange)
        {
            if (TargetRange != null)
            {
                _rangeProperty.Clear();
                var start = TargetRange.Start;
                var end = TargetRange.End;
                int targetlength = TargetRange.Text.Length;
                int totallength = 0;
                TextPointer current = start;
                int length = 0;

                while (current.CompareTo(end) < 0)
                {
                    if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        length = current.GetTextRunLength(LogicalDirection.Forward);
                        totallength += length;
                        if (totallength > targetlength)
                        {
                            length = targetlength - totallength + length;
                        }
                        var ad = current.GetAdjacentElement(LogicalDirection.Forward);
                        var tempRange = new TextRange(current, current.GetPositionAtOffset(length, LogicalDirection.Forward));
                        var backgroundProperty = tempRange.GetPropertyValue(Run.BackgroundProperty);
                        _rangeProperty.Add(tempRange, backgroundProperty);
                    }
                    var nextPos = current.GetNextContextPosition(LogicalDirection.Forward);
                    if (nextPos.CompareTo(end) > 0)
                        break;
                    current = nextPos;
                }
            }
            
        }

        private void _RestoreRangeProperty()
        {
            if (_rangeProperty.Count > 0)
            {
                foreach (var item in _rangeProperty)
                {

                    item.Key.ApplyPropertyValue(TextElement.BackgroundProperty, item.Value);
                }
            }
        }

        private void _RestoreLastRange()
        {
            if (_lastRange != null)
            {
                //_lastRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush());
                _RestoreRangeProperty();
            }
        }

        private void _ColorCurrentRange()
        {
            //construct current range according to currentindex
            _currentRange = new TextRange(_foundRanges[CurrentIndex].Start, _foundRanges[CurrentIndex].End);
            //we want to color the current range but we need to restore last range's background property first
            _RestoreLastRange();
            if(_currentRange != null)
            {
                //we save the original background property so that we can restore from
                _CollectRangeProperties(_currentRange);
                //change the background color of current range
                _currentRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
                if(_foundRanges[CurrentIndex].Start.Parent is FrameworkContentElement FE)
                {
                    _BringIntoView(FE);
                }
                _lastRange = _currentRange;
            }
        }

        private void _BringIntoView(FrameworkContentElement element)
        {
            if (element.IsLoaded)
                element.BringIntoView();
            else
            {
                element.Loaded += E_Loaded;
            }
        }
        private void E_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkContentElement FE = (FrameworkContentElement)sender;
            FE.Loaded -= E_Loaded;
            FE.BringIntoView();
        }
    }
}
