using d9.utl;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace d9.ucm;

public partial class CompetitionPage : ContentPage
{
    public Competition? Competition => CompetitionCreation.Competition;
    public CompetitionPage()
    {
        InitializeComponent();
        CompetitionCreation.CompetitionCreated += CompetitionCreated;
    }
    private async void Left_Clicked(object sender, EventArgs e)
    {
        Competition!.Choose(Side.Left);
        await UpdateViews();
    }
    private async void Skip_Clicked(object sender, EventArgs e)
    {
        Competition!.NextItems();
        await UpdateViews();
    }
    private async void Right_Clicked(object sender, EventArgs e)
    {
        Competition!.Choose(Side.Right);
        await UpdateViews();
    }
    private void LeftIrrelevant_Clicked(object sender, EventArgs e)
    {
        Competition!.MarkIrrelevant(Side.Left);
        LeftItemView.IsIrrelevant = true;
        UpdateButtonActivation();
    }
    private void RightIrrelevant_Clicked(object sender, EventArgs e)
    {
        Competition!.MarkIrrelevant(Side.Right);
        RightItemView.IsIrrelevant = true;
        UpdateButtonActivation();
    }       
    private void UpdateButtonActivation()
    {
        LeftIrrelevant.IsEnabled = !LeftItemView.IsIrrelevant;
        SelectLeft.IsEnabled = !LeftItemView.IsIrrelevant;
        SelectRight.IsEnabled = !RightItemView.IsIrrelevant;
        RightIrrelevant.IsEnabled = !RightItemView.IsIrrelevant;
    }
    private async void CompetitionCreated(object? sender, EventArgs e)
    {
        if (Competition is null)
        {
            RatingScreen.IsVisible = false;
            return;
        }
        CompetitionCreation.IsVisible = false;
        Utils.Log($"CompetitionCreated({Competition?.Name.PrintNull()})");        
        RatingScreen.IsVisible = true;
        await UpdateViews();
    }
    public void Update(CompetitionItemView itemView, Side side)
    {
        if (Competition is null)
            return;
        itemView.UpdateWith(Competition[side], $"Ratings: {Competition.RatingOf(side)?.TotalRatings}");
    }
    private async Task UpdateViews()
    {
        await Competition!.SaveAsync();
        Update(LeftItemView, Side.Left);
        Update(RightItemView, Side.Right);
        UpdateButtonActivation();
    }
}