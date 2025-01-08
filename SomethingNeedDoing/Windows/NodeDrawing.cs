using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using SomethingNeedDoing.Interface;
using SomethingNeedDoing.Misc;
using System;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Windows;
internal class NodeDrawing
{
    private readonly Regex incrementalName = new(@"(?<all> \((?<index>\d+)\))$", RegexOptions.Compiled);
    private INode? draggedNode = null;
    private MacroNode? activeMacroNode = null;

    public void DisplayNodeTree()
    {
        DrawHeader();
        DisplayNode(C.RootFolder);
    }

    private void DrawHeader()
    {
        if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "Add macro"))
        {
            var newNode = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
            C.RootFolder.Children.Add(newNode);
            Service.Configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new FolderNode { Name = GetUniqueNodeName("Untitled folder") };
            C.RootFolder.Children.Add(newNode);
            Service.Configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.FileImport, "Import macro from clipboard"))
        {
            var text = Utils.ConvertClipboardToSafeString();
            var node = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
            C.RootFolder.Children.Add(node);

            if (Utils.IsLuaCode(text))
                node.Language = Language.Lua;

            node.Contents = text;
            Service.Configuration.Save();
        }
    }

    private void DisplayNode(INode node)
    {
        using var _ = ImRaii.PushId(node.Name);
        if (node is FolderNode folderNode)
            DisplayFolderNode(folderNode);
        else if (node is MacroNode macroNode)
            DisplayMacroNode(macroNode);
    }

    private void DisplayMacroNode(MacroNode node)
    {
        var flags = ImGuiTreeNodeFlags.Leaf;
        if (node == activeMacroNode)
            flags |= ImGuiTreeNodeFlags.Selected;

        ImGui.TreeNodeEx($"{node.Name}##tree", flags);

        NodeContextMenu(node);
        NodeDragDrop(node);

        if (ImGui.IsItemClicked())
            activeMacroNode = node;

        ImGui.TreePop();
    }

    private void DisplayFolderNode(FolderNode node)
    {
        if (node == C.RootFolder)
            ImGui.SetNextItemOpen(true, ImGuiCond.FirstUseEver);

        var expanded = ImGui.TreeNodeEx($"{node.Name}##tree");

        NodeContextMenu(node);
        NodeDragDrop(node);

        if (expanded)
        {
            foreach (var childNode in node.Children.ToArray())
                DisplayNode(childNode);
            ImGui.TreePop();
        }
    }

    private void NodeContextMenu(INode node)
    {
        if (node == null) return;
        ImGui.OpenPopupOnItemClick($"{node.Name}ContextMenu", ImGuiPopupFlags.MouseButtonRight);
        using var ctx = ImRaii.ContextPopupItem($"{node.Name}ContextMenu");
        if (ctx)
        {
            var name = node.FileName;
            if (ImGui.InputText($"##rename", ref name, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
            {
                node.Name = GetUniqueNodeName(name);
                Service.Configuration.Save();
            }

            if (node is MacroNode macroNode)
                if (ImGuiX.IconButton(FontAwesomeIcon.Play, "Run"))
                    macroNode.RunMacro();

            if (node is FolderNode folderNode)
            {
                if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "Add macro"))
                {
                    var newNode = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
                    folderNode.Children.Add(newNode);
                    Service.Configuration.Save();
                }

                ImGui.SameLine();
                if (ImGuiX.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                {
                    var newNode = new FolderNode { Name = GetUniqueNodeName("Untitled folder") };
                    folderNode.Children.Add(newNode);
                    Service.Configuration.Save();
                }
            }

            if (node != C.RootFolder)
            {
                ImGui.SameLine();
                if (ImGuiX.IconButton(FontAwesomeIcon.Copy, "Copy Name"))
                    ImGui.SetClipboardText(node.Name);

                ImGui.SameLine();
                if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (Service.Configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        Service.Configuration.Save();
                    }
                }

                ImGui.SameLine();
            }
        }
    }

    private string GetUniqueNodeName(string name)
    {
        var nodeNames = Service.Configuration.GetAllNodes()
            .Select(node => node.Name)
            .ToList();

        while (nodeNames.Contains(name))
        {
            var match = incrementalName.Match(name);
            if (match.Success)
            {
                var all = match.Groups["all"].Value;
                var index = int.Parse(match.Groups["index"].Value) + 1;
                name = name[..^all.Length];
                name = $"{name} ({index})";
            }
            else
            {
                name = $"{name} (1)";
            }
        }

        return name.Trim();
    }

    private void NodeDragDrop(INode node)
    {
        if (node != C.RootFolder)
        {
            if (ImGui.BeginDragDropSource())
            {
                draggedNode = node;
                ImGui.TextUnformatted(node.Name);
                ImGui.SetDragDropPayload("NodePayload", IntPtr.Zero, 0);
                ImGui.EndDragDropSource();
            }
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("NodePayload");

            bool nullPtr;
            unsafe
            {
                nullPtr = payload.NativePtr == null;
            }

            var targetNode = node;
            if (!nullPtr && payload.IsDelivery() && draggedNode != null)
            {
                if (!Service.Configuration.TryFindParent(draggedNode, out var draggedNodeParent))
                    throw new Exception($"Could not find parent of node \"{draggedNode.Name}\"");

                if (targetNode is FolderNode targetFolderNode)
                {
                    draggedNodeParent!.Children.Remove(draggedNode);
                    targetFolderNode.Children.Add(draggedNode);
                    Service.Configuration.Save();
                }
                else
                {
                    if (!Service.Configuration.TryFindParent(targetNode, out var targetNodeParent))
                        throw new Exception($"Could not find parent of node \"{targetNode.Name}\"");

                    var targetNodeIndex = targetNodeParent!.Children.IndexOf(targetNode);
                    if (targetNodeParent == draggedNodeParent)
                    {
                        var draggedNodeIndex = targetNodeParent.Children.IndexOf(draggedNode);
                        if (draggedNodeIndex < targetNodeIndex)
                        {
                            targetNodeIndex -= 1;
                        }
                    }

                    draggedNodeParent!.Children.Remove(draggedNode);
                    targetNodeParent.Children.Insert(targetNodeIndex, draggedNode);
                    Service.Configuration.Save();
                }

                draggedNode = null;
            }

            ImGui.EndDragDropTarget();
        }
    }
}
