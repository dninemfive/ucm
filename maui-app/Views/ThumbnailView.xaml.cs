namespace d9.ucm;
public partial class ThumbnailView : ContentView
{
	public ThumbnailView(Item item, double size = 100, object? sortItem = null, bool isIrrelevant = false)
	{
		InitializeComponent();
		// HeightRequest = size;
		// WidthRequest = size;
		Thumbnail.HeightRequest = size;
		Thumbnail.WidthRequest = size;
		if(item.Thumbnail is not null)
		{
			// Thumbnail = item.Thumbnail;
			Thumbnail.Source = item.LocalPath.Value;
            if (sortItem is not null)
            {
                Label.Text = sortItem.ToString();
				Label.IsVisible = true;
            }
			if (isIrrelevant)
				Thumbnail.Opacity = 0.42;
        } 
		else
		{
			Label.IsVisible = true;
			Label.Text = "invalid item";
			Label.VerticalOptions = LayoutOptions.Center;
			Label.HorizontalOptions = LayoutOptions.Center;
			Label.TextColor = Colors.Red;
		}		
		ToolTipProperties.SetText(this, item);
	}
}