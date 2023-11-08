using CommunityToolkit.Maui.Core;
namespace d9.ucm;
public partial class IItemView : ContentView
{
	private IItemViewable? _iitem = null;
	public IItemViewable? IItem
	{
		get => _iitem;
		set
		{
			_iitem = value;
            UpdateInfo();
		}
	}
	public IItemView()
	{
		InitializeComponent();
        SourceList.HeightRequest = HeightRequest;
	}
	private void UpdateInfo()
	{
        SourceList.Children.Clear();
        if (_iitem is null)
        {
            ContentHolder.Content = null;
            return;
        }
        ContentHolder.Content = _iitem!.View;        
        SourceList.Add(_iitem.InfoLabel);
        foreach (string labelText in _iitem.ItemSources.Select(x => x.LabelText))
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
    public T? IItemAs<T>() where T : class, IItemViewable
        => IItem as T;
}