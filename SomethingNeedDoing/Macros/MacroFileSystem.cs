using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.Configuration;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using ImGuiNET;
using OtterGui;
using OtterGui.Classes;
using OtterGui.Filesystem;
using OtterGui.FileSystem.Selector;
using SomethingNeedDoing.Interface;
using SomethingNeedDoing.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;

namespace SomethingNeedDoing.Macros;
public class MacroFileSystem : FileSystem<MacroFile>
{
    private readonly string FilePath = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "MacroFileSystem.json");
    public readonly FileSystemSelector Selector = null!;
    public bool Building { get; private set; }
    public List<MacroFile> Files => Root.GetAllDescendants(ISortMode<MacroFile>.Lexicographical).OfType<Leaf>().Select(x => x.Value).ToList();
    public MacroFileSystem(OtterGuiHandler h)
    {
        try
        {
            var info = new FileInfo(FilePath);
            if (info.Exists)
                Load(info, C.Files, ConvertToIdentifier, ConvertToName);
            Selector = new(this, h);
            BuildFileSystem();
            Changed += OnChange;
            EzConfig.OnSave += Save;
        }
        catch (Exception e) { e.Log(); }
    }

    public void Dispose()
    {
        EzConfig.OnSave -= Save;
        Changed -= OnChange;
    }

    public void DoAdd(MacroFile file, string name)
    {
        PluginLog.Debug($"Adding {file.ID}");
        CreateLeaf(Root, name, file);
        C.Files.Add(file);
        file.Create();
        BuildFileSystem();
    }

    public void DoDelete(MacroFile file)
    {
        PluginLog.Debug($"Deleting {file.ID}");
        file.Delete();
        C.Files.Remove(file);
        if (TryFindLeaf(file, out var leaf))
            Delete(leaf);
        BuildFileSystem();
    }

    public bool TryFindLeaf(MacroFile file, [NotNullWhen(returnValue: true)] out Leaf? leaf)
    {
        leaf = Root.GetAllDescendants(ISortMode<MacroFile>.Lexicographical).OfType<Leaf>().FirstOrDefault(l => l.Value == file);
        return leaf != null;
    }

    public bool TryFindLeaf(Func<MacroFile, bool> predicate, [NotNullWhen(returnValue: true)] out Leaf? leaf)
    {
        leaf = Root.GetAllDescendants(ISortMode<MacroFile>.Lexicographical).OfType<Leaf>().FirstOrDefault(l => predicate(l.Value));
        return leaf != null;
    }

    public bool TryFindMacroByName(string name, [NotNullWhen(returnValue: true)] out MacroFile? file)
    {
        file = Root.GetAllDescendants(ISortMode<MacroFile>.Lexicographical).OfType<Leaf>().FirstOrDefault(l => l.Name == name)?.Value;
        return file != null && !file.IsNull();
    }

    //public bool TryGetPathByID(Guid id, [NotNullWhen(returnValue: true)] out string? path)
    //{
    //    if (TryFindLeaf(C.Files.FirstOrDefault(x => x.GUID == id), out var leaf))
    //    {
    //        path = leaf.FullName();
    //        return true;
    //    }
    //    path = default;
    //    return false;
    //}

    private void OnChange(FileSystemChangeType type, IPath changedObject, IPath? previousParent, IPath? newParent)
    {
        // logic for other change types could be handled here, but it's easier to do elsewhere since we're dealing with the actual file and don't have to find it from a path.
        switch (type)
        {
            case FileSystemChangeType.ObjectMoved:
                // changedObject has the new location, the OS filesystem must be updated accordingly. Find the file in the vfs at the new location and update it in the rfs.
                var prev = previousParent.GetFilePathFromFolder(changedObject.Name);
                var dest = changedObject.GetOSPath();
                if (TryFindLeaf(l => l.Path == dest, out var leaf))
                    leaf.Value.Move(prev, dest);
                else
                    PluginLog.Error($"Unable to find file @ {dest}, real filesystem has not been updated to match the virtual filesystem.");
                break;
        }
        Save();
    }

    private string ConvertToName(MacroFile file)
    {
        PluginLog.Debug($"Request conversion of {file.Name} {file.ID} to name");
        return $"Unnamed " + file.ID;
    }

    private string ConvertToIdentifier(MacroFile file)
    {
        PluginLog.Debug($"Request conversion of {file.Name} {file.ID} to identifier");
        return file.ID;
    }

    public void Save()
    {
        try
        {
            using var FileStream = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var StreamWriter = new StreamWriter(FileStream);
            SaveToFile(StreamWriter, SaveConverter, true);
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Error saving {nameof(MacroFileSystem)}:");
            ex.Log();
        }
    }

    private (string, bool) SaveConverter(MacroFile file, string arg2)
    {
        PluginLog.LogVerbose($"Saving {file.Name} {file.ID}");
        return (file.ID, true);
    }

    public void BuildFileSystem()
    {
        Building = true;
        //foreach (var f1 in C.Files)
        //{
        //    if (TryFindLeaf(x => x.Path == f1.Path, out var leaf))
        //    {

        //    }
        //}
        C.Files = Files;
        WipeFileSystem();
        BuildDirectories(new DirectoryInfo(C.RootFolderPath), Root);
        C.Save();
        Building = false;
    }

    public void WipeFileSystem()
    {
        try
        {
            foreach (var x in Root.GetLeaves().ToList())
                Delete(x);
            foreach (var x in Root.GetSubFolders().ToList())
                Delete(x);
        }
        catch (Exception e) { e.Log(); }
    }

    private void BuildDirectories(DirectoryInfo directoryInfo, Folder virtualDirectory)
    {
        try
        {
            foreach (var childInfo in directoryInfo.GetDirectories())
            {
                var (virtualChild, _) = FindOrCreateFolder(virtualDirectory, childInfo.Name);
                BuildDirectories(childInfo, virtualChild);
            }

            foreach (var childFile in directoryInfo.GetFiles())
                CreateLeaf(virtualDirectory, childFile.Name, new MacroFile { File = childFile });
        }
        catch (UnauthorizedAccessException e) { e.LogWarning(); }
        catch (Exception e) { e.Log(); }
    }

    public class FileSystemSelector : FileSystemSelector<MacroFile, FileSystemSelector.State>
    {
        private string NewName = "";
        private string ClipboardText = null;
        private MacroFile CloneStatus = null;
        public override ISortMode<MacroFile> SortMode => ISortMode<MacroFile>.FoldersFirst;

        public FileSystemSelector(MacroFileSystem fs, OtterGuiHandler h) : base(fs, Svc.KeyState, h.Logger, (e) => e.Log())
        {
            AddButton(NewMacroButton, 0);
            AddButton(ImportButton, 10);
            AddButton(RebuildDirectoryButton, 20);
            AddButton(DeleteButton, 1000);
        }

        public void DrawSelected()
        {
            using var child = ImRaii.Child("##Panel", -Vector2.One, true);
            if (!child || Selected == null) return;
            ImGui.TextUnformatted("Macro Editor");

            using var disabled = ImRaii.Disabled(Service.MacroManager.State == LoopState.Running);

            if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Run"))
                Selected.Run();

            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            var lang = Selected.Language;
            if (ImGuiEx.EnumCombo("Language", ref lang))
                Selected.ChangeExtension(lang);

            ImGui.SameLine();
            var buttonSize = ImGuiHelpers.GetButtonSize(FontAwesomeIcon.FileImport.ToIconString());
            ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - buttonSize.X - ImGui.GetStyle().WindowPadding.X);
            if (ImGuiX.IconButton(FontAwesomeIcon.FileImport, "Import from clipboard"))
                Selected.Write(Utils.ConvertClipboardToSafeString());

            ImGui.SetNextItemWidth(-1);
            using var font = ImRaii.PushFont(UiBuilder.MonoFont, !C.DisableMonospaced);

            if (Selected.Exists)
            {
                var contents = Selected.Contents;
                if (ImGui.InputTextMultiline($"##{Selected.Name}-editor", ref contents, 1_000_000, new Vector2(-1, -1)))
                    Selected.Write(contents);
            }
        }

        protected override uint CollapsedFolderColor => ImGuiColors.DalamudViolet.ToUint();
        protected override uint ExpandedFolderColor => CollapsedFolderColor;

        protected override void DrawLeafName(Leaf leaf, in State state, bool selected)
        {
            var flag = selected ? ImGuiTreeNodeFlags.Selected | LeafFlags : LeafFlags;
            using var _ = ImRaii.TreeNode(leaf.Name + $"                                                       ", flag);
        }

        private void RebuildDirectoryButton(Vector2 vector)
        {
            if (!ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Recycle.ToIconString(), vector, "Rebuild Directory", false, true)) return;
            Service.FS.BuildFileSystem();
        }

        private void ImportButton(Vector2 size)
        {
            if (!ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.FileImport.ToIconString(), size, "Try to import a macro from your clipboard.", false, true))
                return;

            try
            {
                CloneStatus = null;
                ClipboardText = Paste();
                ImGui.OpenPopup("##NewMacro");
            }
            catch
            {
                Notify.Error("Could not import data from clipboard.");
            }
        }

        private void DeleteButton(Vector2 vector) => DeleteSelectionButton(vector, new DoubleModifier(ModifierHotkey.Control), "macro", "macros", Service.FS.DoDelete);

        private void NewMacroButton(Vector2 size)
        {
            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), size, "Create a new macro", false, true))
            {
                ClipboardText = null;
                CloneStatus = null;
                ImGui.OpenPopup("##NewMacro");
            }
        }

        private void DrawNewMacroPopup()
        {
            if (!ImGuiUtil.OpenNameField("##NewMacro", ref NewName)) return;

            if (NewName == "")
            {
                Notify.Error($"Name can not be empty!");
                return;
            }

            if (ClipboardText != null)
            {
                try
                {
                    var newFile = EzConfig.DefaultSerializationFactory.Deserialize<MacroFile>(ClipboardText);
                    if (!newFile?.IsNull() ?? false)
                        Service.FS.DoAdd(newFile!, NewName);
                    else
                        Notify.Error($"Invalid clipboard data");
                }
                catch (Exception e)
                {
                    e.LogVerbose();
                    Notify.Error($"Error: {e.Message}");
                }
            }
            else if (CloneStatus != null) { }
            else
            {
                try
                {
                    Service.FS.DoAdd(new MacroFile() { File = new FileInfo(NewName) }, NewName);
                }
                catch (Exception e)
                {
                    e.LogVerbose();
                    Notify.Error($"This name already exists!");
                }
            }

            NewName = string.Empty;
        }

        protected override void DrawPopups() => DrawNewMacroPopup();

        public record struct State { }
        protected override bool ApplyFilters(IPath path) => FilterValue.Length > 0 && !path.FullName().Contains(FilterValue, StringComparison.OrdinalIgnoreCase);
    }
}

public static class IPathExtensions
{
    /// <remarks> Meant for passing in an IPath that is a file, not folder, to generate an OS path. Swaps hardcoded `/` separator to OS default.</remarks>
    public static string GetOSPath(this FileSystem<MacroFile>.IPath path) => Path.Combine(C.RootFolderPath, path.FullName().Replace('/', Path.DirectorySeparatorChar));

    /// <remarks> Meant for passing in a parent + name to generate an OS path. Swaps hardcoded `/` separator to OS default.</remarks>
    public static string GetFilePathFromFolder(this FileSystem<MacroFile>.IPath? path, string fileName) => path != null ? $"{path.GetOSPath()}{Path.DirectorySeparatorChar}{fileName}" : $"{C.RootFolderPath}{Path.DirectorySeparatorChar}{fileName}";
}
