namespace d9.ucm;
public partial class ThumbnailView : ContentView
{
	public ThumbnailView(Item item, double size = 100, object? sortItem = null)
	{
		InitializeComponent();
		HeightRequest = size;
		WidthRequest = size;
		if(item.Thumbnail is not null)
		{
			Thumbnail = item.Thumbnail;
            if (sortItem is not null)
            {
                Label.Text = sortItem.ToString();
				Label.IsVisible = true;
            }
        } 
		else
		{
			Label.IsVisible = true;
			Label.Text = "invalid item";
			Label.VerticalOptions = LayoutOptions.Center;
			Label.HorizontalOptions = LayoutOptions.Center;
			Label.TextColor = Colors.Red;
		}		
		ToolTipProperties.SetText(this, $"{item.Id}\n{item.Path}");
	}
}