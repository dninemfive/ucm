using d9.utl;
using System.Collections.ObjectModel;

namespace d9.ucm;
public partial class CompetitionSelector : ContentView
{
    public Competition? Competition { get; set; } = null;
    private readonly ObservableCollection<string> _competitions = new();
    // https://stackoverflow.com/a/73597601
    public bool AllowNewItem { get; set; }
    public bool CanCreateCompetition
        => AllowNewItem && CompetitionName.Text.Length > 0 && !File.Exists(Competition.PathFor(CompetitionName.Text));
    public bool NoItemSelected => Dropdown.SelectedIndex == 0;
    public bool NewItemDialogSelected => AllowNewItem && Dropdown.SelectedIndex == Dropdown.Items.Count - 1;
	public CompetitionSelector()
	{        
		InitializeComponent();
        _competitions.Add("(no item)");
        foreach(string name in Competition.Names.OrderBy(x => x))
        {
            _competitions.Add(name);
        }
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
        int index = _competitions.Count - 1;
        _competitions.Insert(index, Competition!.Name);
        Dropdown.SelectedItem = index;
        CompetitionSelected?.Invoke(sender, e);
    }
    public event EventHandler? CompetitionSelected;

    private void Dropdown_SelectedIndexChanged(object sender, EventArgs e)
    {
        CreationItems.IsVisible = NewItemDialogSelected;
        if(!NoItemSelected && 
           !NewItemDialogSelected && 
            Dropdown.SelectedIndex >= 0 && 
            Dropdown.SelectedIndex < Dropdown.Items.Count)
        {
            Competition = Competition.Named(Dropdown.SelectedItem as string);
        }
        else
        {
            Competition = null;
        }
        CompetitionSelected?.Invoke(sender, e);
    }

    private void Dropdown_Loaded(object sender, EventArgs e)
    {
        if(AllowNewItem)
        {
            _competitions.Add("New item...");
        }
    }
}