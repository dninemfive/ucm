using d9.utl;
using System.Collections.ObjectModel;

namespace d9.ucm;

public partial class CompetitionSelector : ContentView
{
    public Competition? Competition { get; set; } = null;
    private readonly ObservableCollection<string> _competitions = new();
    public bool AllowNewItem { get; set; } = true;
    public bool CanCreateCompetition
        => AllowNewItem && CompetitionName.Text.Length > 0 && !File.Exists(Competition.PathFor(CompetitionName.Text));
    public bool NoItemSelected => Dropdown.SelectedIndex == 0;
    public bool NewItemDialogSelected => AllowNewItem && Dropdown.SelectedIndex == Dropdown.Items.Count - 1;
	public CompetitionSelector()
	{
		InitializeComponent();
        _competitions.Add("(no item)");
        foreach(string file in Directory.EnumerateFiles(MauiProgram.TEMP_COMP_LOCATION))
        {
            if(Path.GetExtension(file) == ".json")
            {
                _competitions.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        if(AllowNewItem)
            _competitions.Add("New item...");
        Dropdown.ItemsSource = _competitions;
	}
    private void CompetitionName_TextChanged(object sender, TextChangedEventArgs e)
    {
        CreateButton.IsEnabled = CanCreateCompetition;
    }
    private async void CreateCompetition(object sender, EventArgs e)
    {
        if(!CanCreateCompetition)
        {
            Competition = null;
            return;
        }
        Competition = await Competition.LoadOrCreateAsync(CompetitionName.Text);
        Utils.Log($"CreateCompetition() -> {Competition?.Name.PrintNull()}");
        int index = _competitions.Count - 1;
        _competitions.Insert(index, Competition!.Name);
        Dropdown.SelectedItem = index;
        CompetitionCreated?.Invoke(sender, e);
    }
    public event EventHandler? CompetitionCreated;

    private async void Dropdown_SelectedIndexChanged(object sender, EventArgs e)
    {
        Utils.Log($"Dropdown_SelectedIndexChanged({Dropdown.SelectedIndex})");
        CreationItems.IsVisible = NewItemDialogSelected;
        if(!NoItemSelected && 
                !NewItemDialogSelected && 
                Dropdown.SelectedIndex >= 0 && 
                Dropdown.SelectedIndex < Dropdown.Items.Count)
        {
            Competition = await Competition.LoadOrCreateAsync(Dropdown.SelectedItem as string);
        }
        else
        {
            Competition = null;
        }
        CompetitionCreated?.Invoke(sender, e);
    }
}