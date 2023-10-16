namespace d9.ucm.Views;

public partial class TagSelector : ContentView
{
	private string _search;
	public bool Invert { get; private set; } = false;
	public bool Matches
	{
		get
		{
			return true;
		}
	}
	public TagSelector()
	{
		InitializeComponent();
	}

    private void Entry_Completed(object sender, EventArgs e)
    {

    }
}