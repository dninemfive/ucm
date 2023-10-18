namespace d9.ucm;

public partial class TagSearchView : ContentView
{
	public bool Invert { get; private set; } = false;
	public TagSearchView()
	{
		InitializeComponent();
		ToolTipProperties.SetText(InvertCheckbox, $"When checked, items which do NOT match the search will be shown.");
	}
	public delegate void TagSearch(IEnumerable<SearchToken> tokens, bool invert);
	public event TagSearch? TagSearchedFor;
    private void SearchButtonPressed(object sender, EventArgs e)
    {
		TagSearchedFor?.Invoke(TagSearchBar.Text.Tokenize(), Invert);
    }
    private void InvertCheckbox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
		Invert = InvertCheckbox.IsChecked;
    }
}