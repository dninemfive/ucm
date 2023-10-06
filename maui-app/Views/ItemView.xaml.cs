namespace d9.ucm;

public partial class ItemView : ContentView
{
	private IItemViewable? _item = null;
	public IItemViewable? Item
	{
		get => _item;
		set
		{
			_item = value;
            SourceList.Children.Clear();
            if (value is null)
			{
                ContentHolder.Content = null;				
				return;
            }
			ContentHolder.Content = _item!.View;
			foreach(string labelText in _item.ItemSources.Select(x => x.LabelText))
			{
				SourceList.Add(new Label()
				{
					Text = labelText,
					BackgroundColor = Colors.LightGrey,
					TextColor = Colors.Black,
					Margin = new(4)
				});
			}
		}
	}
	public ItemView()
	{
		InitializeComponent();
	}
}