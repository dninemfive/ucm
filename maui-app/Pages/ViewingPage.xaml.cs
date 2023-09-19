namespace d9.ucm;

public partial class ViewingPage : ContentPage
{
	public ViewingPage()
	{
        InitializeComponent();
		int i = 0;
		foreach(Item item in ItemManager.All.OrderBy(x => x.Id))
		{
			if (++i > 40)
				break;
            ItemsHolder.Add(new Image() { Source = item.Path, HeightRequest = 100, WidthRequest = 100, Aspect = Aspect.AspectFit });
		}
	}
}

