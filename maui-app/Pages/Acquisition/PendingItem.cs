using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public class PendingItem
{
    public string Hash;
    public string CurrentPath;
    public string? MoveToFolder;
    public PendingItem(string path, string hash, string? moveToFolder)
    {
        Hash = hash;
        CurrentPath = path;
        MoveToFolder = moveToFolder;
    }
    public async Task Save()
    {
        if (File.Exists(CurrentPath))
        {
            if (MoveToFolder is not null)
            {
                ItemId id = IdManager.Register();
                string newPath = Path.Join(MoveToFolder, $"{id}{Path.GetExtension(CurrentPath).ToLower()}");
                CurrentPath.MoveFileTo(newPath);
                _ = await ItemManager.CreateAndSave(newPath, Hash, id);
            }
            else
            {
                _ = await ItemManager.CreateAndSave(CurrentPath, Hash);
            }
        }
    }
}
