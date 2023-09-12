using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public class ImageItem : IItem
{
    public FileReference File { get; private set; }
    public ItemId Id { get; private set; }
    private Image? _image = null;
    public View View
    {
        get
        {
            _image ??= new() { Source = File.Location };
            return _image;
        }
    }
    public ImageItem(FileReference file, ItemId? id = null)
    {
        File = file;
        Id = IdManager.Register(id);
    }
}
