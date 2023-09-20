namespace d9.ucm;

public partial class CollectionPage : ContentPage
{
	public CollectionPage()
	{
        InitializeComponent();
		int i = 0;
		int horizontalItems = 7;
		foreach(Item item in ItemManager.Items.OrderBy(x => x.Id))
		{
			if (++i > 25)
				break;
            ItemsHolder.Add(new ThumbnailView(item, 1920 / horizontalItems - 10, item.Id));
		}
	}
}

