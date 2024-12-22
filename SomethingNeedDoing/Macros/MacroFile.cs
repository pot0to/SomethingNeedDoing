using Lumina.Data.Files;
using Newtonsoft.Json;
using SomethingNeedDoing.Macros.Exceptions;
using SomethingNeedDoing.Misc;
using System;
using System.IO;

namespace SomethingNeedDoing.Macros;
public class MacroFile
{
    internal string ID => GUID.ToString();
    public string Name => P.OtterGuiHandler.MacroFileSystem.FindLeaf(this, out var l) ? l.FullName() : string.Empty;

    public Guid GUID = Guid.NewGuid();
    public string Gist = string.Empty;
    public bool CraftingLoop = false;
    public int CraftLoopCount = 0;
    public bool UseInARPostProcess = false;

    [JsonIgnore] public required FileInfo File;
    [JsonIgnore] internal string Path => System.IO.Path.Combine(C.RootFolderPath, Name);
    [JsonIgnore] internal string RelativePath => System.IO.Path.GetRelativePath(Service.Configuration.RootFolderPath, Path);
    [JsonIgnore] internal bool HasRelativePath => RelativePath.Contains(System.IO.Path.DirectorySeparatorChar);
    [JsonIgnore] internal string FileSystemRelativePath => RelativePath.Replace(System.IO.Path.DirectorySeparatorChar, '/').Replace(@"\\", "/");
    [JsonIgnore] internal bool Exists => System.IO.File.Exists(Path);
    [JsonIgnore] internal Language Language => System.IO.Path.GetExtension(Path).FileExtensionToLanguage();
    [JsonIgnore] internal string Contents => System.IO.File.ReadAllText(Path);

    public void SetAsPostProcessMacro(bool state)
    {
        if (state)
        {
            foreach (var f in C.Files)
                f.UseInARPostProcess = false;
            UseInARPostProcess = true;
        }
        else UseInARPostProcess = false;
    }

    public void ChangeExtension(Language language) => System.IO.File.Move(Path, System.IO.Path.ChangeExtension(Path, language.LanguageToFileExtension()));
    public void Write(string text) => System.IO.File.WriteAllText(Path, text);
    public void Create()
    {
        if (!System.IO.Path.HasExtension(Path))
            System.IO.File.Create(Path + ".txt").Dispose();
        else
            System.IO.File.Create(Path).Dispose();
    }
    public void Delete()
    {
        if (Exists) System.IO.File.Delete(Path);
    }

    public void Run()
    {
        try
        {
            Service.MacroManager.EnqueueMacro(this);
        }
        catch (MacroSyntaxError ex)
        {
            Service.ChatManager.PrintError($"{ex.Message}");
        }
        catch (Exception ex)
        {
            Service.ChatManager.PrintError($"Unexpected error");
            Svc.Log.Error(ex, "Unexpected error");
        }
    }
}

public static class MacroFileExtensions
{
    public static bool IsNull(this MacroFile file)
    {
        if (file == null) return true;
        if (file.Name == null) return true;
        return false;
    }
}
