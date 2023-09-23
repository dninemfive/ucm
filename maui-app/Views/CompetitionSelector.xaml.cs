using CommunityToolkit.Mvvm.ComponentModel;
using d9.utl;
using System.Collections.ObjectModel;

namespace d9.ucm;
public partial class CompetitionSelector : ContentView
{
    public Competition? Competition { get; set; } = null;
    private readonly ObservableCollection<string> _competitions = new();
    // https://stackoverflow.com/a/73597601
    [ObservableProperty]
    public bool allowNewItem;
    public bool CanCreateCompetition
        => allowNewItem && CompetitionName.Text.Length > 0 && !File.Exists(Competition.PathFor(CompetitionName.Text));
    public bool NoItemSelected => Dropdown.SelectedIndex == 0;
    public bool NewItemDialogSelected => allowNewItem && Dropdown.SelectedIndex == Dropdown.Items.Count - 1;
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
        Dropdown.ItemsSource = _competitions;
        
	}
    partial void OnAllowNewItemChanging(bool value)
    {
        if (value)
        {
            _competitions.Add("New item...");
        }
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
        CompetitionSelected?.Invoke(sender, e);
    }
    public event EventHandler? CompetitionSelected;

    private async void Dropdown_SelectedIndexChanged(object sender, EventArgs e)
    {
        Utils.Log($"Dropdown_SelectedIndexChanged({Dropdown.SelectedIndex} {Dropdown.Items.Count})");
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
        CompetitionSelected?.Invoke(sender, e);
    }
}