using Community.VisualStudio.Toolkit;
using DomainValues.Shared.Model;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Solution = Community.VisualStudio.Toolkit.Solution;
using Task = System.Threading.Tasks.Task;

namespace DomainValues.Shared.Common
{
    public static class Extensions
    {
        public static (int start, int end) GetSelectionLineBounds(this ITextView textView)
        {
            if (textView.Selection.IsEmpty)
            {
                var lineNo = textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

                return (lineNo, lineNo);
            }

            var start = textView.Selection.Start.Position.GetContainingLine().LineNumber;
            var end = textView.Selection.End.Position.GetContainingLine().LineNumber;

            return (start, end);
        }

        public static ProjectItem AsProjectItem(this PhysicalFile physicalFile)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            physicalFile.GetItemInfo(out var hierarchy, out var itemId, out var _);

            hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out var objProjectItem);

            return objProjectItem as ProjectItem;
        }

        public static async Task SetAsChildItemAsync(this PhysicalFile file, PhysicalFile parent)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var projectItem = parent.AsProjectItem();

            if (projectItem.ContainingProject.Properties.Item("TargetFrameworkMoniker").Value.ToString().Contains(".NETFramework"))
            {
                projectItem.ProjectItems.AddFromFile(file.FullPath);
            }
            else
            {
                await file.TrySetAttributeAsync("DependentUpon", parent.Text);
            }
        }

        public static void SetProperty(this PhysicalFile physicalFile, string propertyName, string propertyValue)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectItem = physicalFile.AsProjectItem();

            projectItem.Properties.Item(propertyName).Value = propertyValue;
        }

        public static Solution GetSolution(this SolutionItem solutionItem)
        {
            while (true)
            {
                if (solutionItem.Type == SolutionItemType.Solution)
                {
                    return solutionItem as Solution;
                }

                solutionItem = solutionItem.Parent;

                if (solutionItem == null)
                {
                    return null;
                }
            }
        }

        internal static TextSpan GetTextSpan(this string source) => new TextSpan(source);

        internal static Regex Columns = new Regex(@"(?<=(^|[^\\])\|)[^\|\\]*(?:\\.[^\|\\]*)*(?=\|)", RegexOptions.Compiled);

        internal static IEnumerable<List<ParsedSpan>> GetStatementBlocks(this List<ParsedSpan> source)
        {
            if (!source.Any())
                yield break;

            List<int> range = source
                .Where(a => a.Type == TokenType.Table)
                .Select(a => a.LineNumber)
                .ToList();

            range.Add(source.Max(a => a.LineNumber) + 1);

            for (int i = 0; i < range.Count - 1; i++)
            {
                int start = range[i];
                int end = range[i + 1];

                yield return source.Where(a => a.LineNumber >= start && a.LineNumber < end).ToList();
            }

        }

        internal static IEnumerable<string> GetColumns(this string source)
        {
            return Extensions.Columns.Matches(source).Cast<Match>()
                 .Select(a => a.Value
                    .Trim()
                    .Replace("\\\\\\|", "\\\0")
                    .Replace("\\|", "|")
                    .Replace("\\\0", "\\|")
                );
        }
    }
}
