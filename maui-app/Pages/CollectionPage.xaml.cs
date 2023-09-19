namespace d9.ucm;

public partial class CollectionPage : ContentPage
{
	public CollectionPage()
	{
        InitializeComponent();
		int i = 0;
		foreach(Item item in ItemManager.All.OrderBy(x => x.Id))
		{
			if (++i > 45)
				break;
            ItemsHolder.Add(new ThumbnailView(item, sortItem: item.Id));
		}
	}
}

