#if !DV_LEGACY

using Community.VisualStudio.Toolkit;
using DomainValues.Shared.Common;
using DomainValues.Shared.Processing;
using EnvDTE;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project = Community.VisualStudio.Toolkit.Project;
using Task = System.Threading.Tasks.Task;

namespace DomainValues.Shared.Command
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(AlignTable))]
    [ContentType(DvContent.Id)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class SaveChildItems : IChainedCommandHandler<SaveCommandArgs>
    {
        [Import]
        internal ITextDocumentFactoryService textDocumentFactory = null;

        public string DisplayName => nameof(SaveChildItems);

        public void ExecuteCommand(SaveCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            nextCommandHandler();

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (textDocumentFactory.TryGetTextDocument(args.SubjectBuffer, out var document))
                {
                    await ProcessChildItemsAsync(args.SubjectBuffer.CurrentSnapshot.GetText(), document.FilePath);
                }
            }).FileAndForget(nameof(SaveChildItems));
        }

        public CommandState GetCommandState(SaveCommandArgs args, Func<CommandState> nextCommandHandler)
        {
            return CommandState.Available;
        }

        internal async Task ProcessChildItemsAsync(string documentText, string filePath)
        {
            var physicalFile = await PhysicalFile.FromFileAsync(filePath);

            var spans = Scanner.GetSpans(documentText, true);

            var generator = SpansToContent.Convert(spans);

            await CreateSqlContentAsync(physicalFile, generator);

            await CreateEnumContentAsync(physicalFile, generator);
        }

        internal async Task CreateSqlContentAsync(PhysicalFile physicalFile, ContentGenerator generator)
        {
            var sqlPath = Path.ChangeExtension(physicalFile.FullPath, ".dv.sql");

            var solution = physicalFile.GetSolution();

            var relativePath = physicalFile.FullPath.Replace(Path.GetDirectoryName(solution.FullPath), string.Empty);

            var sqlContent = generator.GetSqlCode(relativePath);

            var file = await WriteFileAsync(sqlContent, sqlPath, physicalFile.ContainingProject);

            if (file == null)
            {
                return;
            }

            await file.SetAsChildItemAsync(physicalFile);

            if (!string.IsNullOrWhiteSpace(generator.CopySql))
            {
                var targetItem = FindSolutionItem(solution, generator.CopySql.Split('\\').ToArray());

                if (targetItem is PhysicalFolder folder)
                {
                    var copyPath = Path.Combine(folder.FullPath, Path.GetFileName(sqlPath));

                    var copyFile = await WriteFileAsync(sqlContent, copyPath, folder.ContainingProject);

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    copyFile.SetProperty("BuildAction", "None");
                }
            }

            await physicalFile.ContainingProject.SaveAsync();
        }

        internal async Task CreateEnumContentAsync(PhysicalFile physicalFile, ContentGenerator generator)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var language = physicalFile.AsProjectItem().ContainingProject.CodeModel.Language;

            var codePath = Path.ChangeExtension(physicalFile.FullPath,
                CodeModelLanguageConstants.vsCMLanguageVB == language
                    ? ".dv.vb"
                    : ".dv.cs"
                );

            var codeProvider = CodeModelLanguageConstants.vsCMLanguageVB == language
                ? CodeDomProvider.CreateProvider("VisualBasic")
                : CodeDomProvider.CreateProvider("C#");

            var fileNamespace = GenerateNamespace(physicalFile);

            var enumContent = generator.GetEnumCode(codeProvider, fileNamespace);

            if (string.IsNullOrWhiteSpace(enumContent))
            {
                var vsFile = await PhysicalFile.FromFileAsync(codePath);

                if (vsFile != null)
                {
                    await vsFile.TryRemoveAsync();
                }

                return;
            }

            var file = await WriteFileAsync(enumContent, codePath, physicalFile.ContainingProject);

            if (file == null)
            {
                return;
            }

            await file.SetAsChildItemAsync(physicalFile);

            await physicalFile.ContainingProject.SaveAsync();
        }

        internal SolutionItem FindSolutionItem(SolutionItem solutionItem, string[] paths)
        {
            var item = solutionItem.Children.SingleOrDefault(a => a.Text.Equals(paths[0], StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                return null;
            }

            if (paths.Length > 1)
            {
                return FindSolutionItem(item, paths.Skip(1).ToArray());
            }

            return item;
        }

        internal async Task<PhysicalFile> WriteFileAsync(string content, string path, Project project)
        {
            using (var sw = new StreamWriter(path))
            {
                await sw.WriteAsync(content);
            }

            var file = await PhysicalFile.FromFileAsync(path);

            if (file == null)
            {
                file = (await project.AddExistingFilesAsync(path)).Single();
            }

            return file;
        }

        private string GenerateNamespace(SolutionItem item)
        {
            switch (item?.Type)
            {
                case null:
                    return string.Empty;
                case SolutionItemType.Project:
                    return item.Text;
                case SolutionItemType.PhysicalFolder:
                    return $"{GenerateNamespace(item.Parent)}.{item.Text}";
                default:
                    return GenerateNamespace(item.Parent);
            }
        }
    }
}

#endif