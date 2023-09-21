namespace d9.ucm;

public partial class CompetitionSelector : ContentView
{
    public Competition? Competition { get; set; } = null;
	public CompetitionSelector()
	{
		InitializeComponent();
	}
    private void CompetitionName_TextChanged(object sender, TextChangedEventArgs e)
    {
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
        Competition = await Competition.LoadOrCreateAsync(CompetitionName.Text);
        CompetitionCreated?.Invoke(sender, e);
    }
    public event EventHandler? CompetitionCreated;
}