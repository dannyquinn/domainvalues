//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using DomainValues.Model;
//using DomainValues.Processing;
//using DomainValues.Util;
//using Microsoft.VisualStudio.Text;
//using Microsoft.VisualStudio.Text.Editor;

//namespace DomainValues.Command
//{
//    internal sealed class Formatter
//    {
//        private readonly IWpfTextView _view;

//        public Formatter(IWpfTextView view)
//        {
//            _view = view;
//        }

//        public void CommentSelection()
//        {
//            int start, end;

//            GetSelectionLines(out start, out end);

//            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
//            {
//                while (start <= end)
//                {
//                    ITextSnapshotLine line = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(start);

//                    edit.Insert(line.Start, "#");

//                    start++;
//                }

//                if (edit.HasEffectiveChanges)
//                    edit.Apply();
//            }

//        }

//        public void RemoveCommentSelection()
//        {
//            int start, end;

//            GetSelectionLines(out start,out end);

//            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
//            {
//                while (start <= end)
//                {
//                    ITextSnapshotLine line = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(start);

//                    string text = line.GetText();

//                    if (text.StartsWith("#"))
//                    {
//                        Span span = new Span(line.Start.Position+text.IndexOf('#'),1);

//                        edit.Replace(span, string.Empty);
//                    }

//                    start++;
//                }
//                if (edit.HasEffectiveChanges)
//                    edit.Apply();
//            }
//        }

//        public void FormatDocument()
//        {
//            Format(0, _view.TextBuffer.CurrentSnapshot.LineCount - 1);
//        }

//        public void FormatSelection()
//        {
//            int start, end;

//            GetSelectionLines(out start, out end);

//            Format(start,end);
//        }

//        public void Format(int start, int end)
//        {
//            List<IGrouping<int, ParsedSpan>> tokens = Scanner.GetSpans(_view.TextBuffer.CurrentSnapshot.GetText(), false)
//                .GroupBy(a => a.LineNumber)
//                .ToList();

//            int blockId = 0;
//            Dictionary<int,List<int>> blockRows = new Dictionary<int, List<int>>();

//            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
//            {
//                for (int i = start; i <= end; i++)
//                {       
//                    List<ParsedSpan> lineTokens = tokens.Where(a => a.Key == i).SelectMany(a => a.OrderBy(b => b.Start)).ToList();

//                    if (!lineTokens.Any())
//                        continue;
                    
//                    ParsedSpan lineToken = lineTokens.First();

//                    int ident = 0;

//                    switch (lineToken.Type)
//                    {
//                        case TokenType.Table:
//                            blockId++;
//                            break;
//                        case TokenType.ItemRow:
//                        case TokenType.HeaderRow:
//                            if (!blockRows.ContainsKey(blockId))
//                            {
//                                blockRows.Add(blockId,new List<int>());
//                            }

//                            blockRows[blockId].Add(lineToken.LineNumber);

//                            continue;
//                        case TokenType.Key:
//                        case TokenType.Enum:
//                        case TokenType.Template:
//                        case TokenType.Data:
//                            ident = 1;
//                            break;
//                    }

//                    ITextSnapshotLine line = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineToken.LineNumber);

//                    string startingText = line.GetText().Substring(0,lineToken.Start);

//                    string replacement = new string('\t', ident);

//                    if (startingText != replacement)
//                    {
//                        Span span = new Span(line.Start, lineToken.Start);

//                        edit.Replace(span, replacement);
//                    }

//                    int index = lineToken.Start + lineToken.Text.Length;

//                    bool lastTokenWasEnumDesc = false;

//                    foreach (ParsedSpan token in lineTokens.Skip(1))
//                    {
//                        string wantedText = " ";
                        
//                        switch (token.Type)
//                        {
//                            case TokenType.EnumInit:
//                                wantedText = " = ";
//                                break;
//                            case TokenType.EnumDesc:
//                                wantedText = " [";
//                                break;
//                            case TokenType.EnumMember:
//                                if (lastTokenWasEnumDesc)
//                                {
//                                    wantedText = "] ";
//                                }

//                                break;
//                        }

//                        string actual = line.GetText().Substring(index, token.Start - index);

//                        if (actual!=wantedText)
//                        { 
//                            Span span = new Span(line.Start.Position + index,token.Start-index);

//                            edit.Replace(span, wantedText);
//                        }

//                        lastTokenWasEnumDesc = token.Type == TokenType.EnumDesc;

//                        index = token.Start + token.Text.Length;
//                    }
//                }

//                foreach (List<int> value in blockRows.Values)
//                {
//                    var startLine = start > value.Min() ? start : value.Min();
//                    var endLine = end < value.Max() ? end : value.Max();

//                    AlignRows(edit, startLine,endLine);
//                }

//                if (edit.HasEffectiveChanges)
//                    edit.Apply();
//            }
//        }

//        public void AlignTable()
//        {
//            // Separated from align rows in preparation for the FormatDocument logic.
//            SnapshotPoint point = _view.Caret.Position.BufferPosition;

//            if (!LineQualifies(point) || IsPipeEscaped(point))
//                return;

//            _view.Caret.MoveToPreviousCaretPosition();

//            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
//            {
//                int blockStart = point.GetContainingLine().LineNumber;

//                int blockEnd = blockStart;

//                while (blockStart > 0)
//                {
//                    string lineText = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(--blockStart).GetText().TrimStart();

//                    if (lineText.StartsWith("table", StringComparison.CurrentCultureIgnoreCase))
//                    {
//                        break;
//                    }
//                }

//                while (blockEnd < _view.TextBuffer.CurrentSnapshot.LineCount - 1)
//                {
//                    string lineText = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(++blockEnd).GetText().TrimStart();

//                    if (lineText.StartsWith("table", StringComparison.CurrentCultureIgnoreCase))
//                    {
//                        break;
//                    }
//                }
//                AlignRows(edit, blockStart,blockEnd);

//                if (edit.HasEffectiveChanges)
//                    edit.Apply();
//            }

//            _view.Caret.MoveToNextCaretPosition();

//            _view.Caret.EnsureVisible();
//        }
//        private void AlignRows(ITextEdit edit,int start, int end)
//        {
            

//            List<ITextSnapshotLine> lines = _view.TextBuffer.CurrentSnapshot.Lines
//                .Where(a => a.LineNumber >= start && a.LineNumber <= end && a.GetText().TrimStart().StartsWith("|")).ToList();

//            List<List<Tuple<Span, string,string>>> lineColumns = lines.Select(a => GetColumns(a.Extent)).ToList();

//            int maxColumns = lineColumns.Max(a => a.Count);

//            foreach (ITextSnapshotLine line in lines)
//            {
//                string lineText = line.GetText();

//                int firstPipe = lineText.IndexOf('|');

//                string existingText = lineText.Substring(0, firstPipe);
//                string wantedText = new string('\t', 2);

//                if (existingText.Equals(wantedText))
//                    continue;

//                Span span = new Span(line.Start,firstPipe);

//                edit.Replace(span, wantedText);
//            }

//            for (int column = 0; column < maxColumns; column++)
//            {
//                List<List<Tuple<Span, string,string>>> cols = lineColumns.Where(a => a.Count >= column + 1).ToList();
//                int maxColLen = cols.Max(a => a[column].Item2.Length);

//                foreach (List<Tuple<Span, string,string>> col in cols)
//                {
//                    string currentText = col[column].Item2;

//                    string newText = $" {currentText}{new string(' ', maxColLen - currentText.Length)} ";

//                    if (col[column].Item3 != newText)
//                    {
//                        edit.Replace(col[column].Item1, newText);
//                    }
//                }

//            }
//        }

//        private void GetSelectionLines(out int start, out int end)
//        {
//            if (_view.Selection.IsEmpty)
//            {
//                int lineNumber = _view.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

//                start = end = lineNumber;
//                return;
//            }

//            start = _view.Selection.Start.Position.GetContainingLine().LineNumber;
//            end = _view.Selection.End.Position.GetContainingLine().LineNumber;
//        }

//        private static List<Tuple<Span, string,string>> GetColumns(SnapshotSpan span)
//        {
//            return RegExpr.Columns.Matches(span.GetText()).Cast<Match>()
//                .Select(a => Tuple.Create(new Span(span.Start + a.Index, a.Length), a.Value.Trim(),a.Value))
//                .ToList();
//        }

//        private static bool LineQualifies(SnapshotPoint point)
//        {
//            string lineText = point.GetContainingLine().GetText();

//            return lineText.TrimStart().StartsWith("|") &&
//                   Regex.Matches(lineText, @"\|", RegexOptions.Compiled).Count >= 2;
//        }

//        private static bool IsPipeEscaped(SnapshotPoint point)
//        {
//            ITextSnapshotLine lineText = point.GetContainingLine();

//            return lineText
//                       .GetText()
//                       .Substring(0, point.Position - lineText.Extent.Span.Start - 1)
//                       .Reverse()
//                       .TakeWhile(a => a == 92) // 92 = \
//                       .Count() % 2 == 1;
//        }
//    }
//}
