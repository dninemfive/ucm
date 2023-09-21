namespace d9.ucm;

public partial class CompetitionSelector : ContentView
{
    public Competition? Competition { get; set; } = null;
    public bool HasValidName => CreateButton.Text.Length > 0;
	public CompetitionSelector()
	{
		InitializeComponent();
	}
    private void CompetitionName_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!HasValidName)
        {
            CreateButton.Text = "Invalid name!";
            CreateButton.IsEnabled = false;
        }
        CreateButton.IsEnabled = true;
        if (File.Exists(Competition.PathFor(CompetitionName.Text)))
        {
            CreateButton.Text = "Load";
        }
        else
        {
            CreateButton.Text = "Create";
        }
    }
    private async void CreateCompetition(object sender, EventArgs e)
    {
        if(!HasValidName)
        {
            Competition = null;
            return;
        }
        Competition = await Competition.LoadOrCreateAsync(CompetitionName.Text);
        CompetitionCreated?.Invoke(sender, e);
    }
    public event EventHandler? CompetitionCreated;
}