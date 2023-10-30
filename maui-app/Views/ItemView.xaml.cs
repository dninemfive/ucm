using CommunityToolkit.Maui.Core;
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
            UpdateInfo();
		}
	}
	public ItemView()
	{
		InitializeComponent();
        SourceList.HeightRequest = HeightRequest;
	}
	private void UpdateInfo()
	{
        SourceList.Children.Clear();
        if (_item is null)
        {
            ContentHolder.Content = null;
            return;
        }
        ContentHolder.Content = _item!.View;        
        SourceList.Add(_item.InfoLabel);
        foreach (string labelText in _item.ItemSources.Select(x => x.LabelText))
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
    private void Expander_ExpandedChanged(object sender, ExpandedChangedEventArgs e)
    {
		if (ExpanderHeaderText is null)
			return;
		ExpanderHeaderText.Text = e.IsExpanded ? "◀" : "▶";
    }
}