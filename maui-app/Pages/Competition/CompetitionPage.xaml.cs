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
        CompetitionCreation.CompetitionSelected += CompetitionCreated;
    }
    private async void Left_Clicked(object sender, EventArgs e)
    {
        Competition!.Choose(Side.Left);
        await UpdateViews();
    }
    private async Task Skip()
    {
        Competition!.NextItems();
        await UpdateViews();
    }
    private async void Skip_Clicked(object sender, EventArgs e)
        => await Skip();
    private async void Right_Clicked(object sender, EventArgs e)
    {
        Competition!.Choose(Side.Right);
        await UpdateViews();
    }
    private async void LeftIrrelevant_Clicked(object sender, EventArgs e)
    {
        Competition!.MarkIrrelevant(Side.Left);
        LeftItemView.IsIrrelevant = true;
        if (LeftItemView.IsIrrelevant && RightItemView.IsIrrelevant)
        {
            await Skip();
            return;
        }
        UpdateButtonActivation();
    }
    private async void RightIrrelevant_Clicked(object sender, EventArgs e)
    {
        Competition!.MarkIrrelevant(Side.Right);
        RightItemView.IsIrrelevant = true;
        if (LeftItemView.IsIrrelevant && RightItemView.IsIrrelevant)
        {
            await Skip();
            return;
        }
        UpdateButtonActivation();
    }       
    private void UpdateButtonActivation()
    {
        LeftIrrelevant.IsEnabled = !LeftItemView.IsIrrelevant;
        SelectLeft.IsEnabled = !(LeftItemView.IsIrrelevant || RightItemView.IsIrrelevant);
        SelectRight.IsEnabled = !(LeftItemView.IsIrrelevant || RightItemView.IsIrrelevant);
        RightIrrelevant.IsEnabled = !RightItemView.IsIrrelevant;
    }
    private async void CompetitionCreated(object? sender, EventArgs e)
    {
        if (Competition is null)
        {
            RatingScreen.IsVisible = false;
            Histogram.IsVisible = false;
            return;
        }
        CompetitionCreation.IsVisible = false;
        RatingScreen.IsVisible = true;
        Histogram.IsVisible = true;
        Competition.NextItems();
        await UpdateViews();
    }
    public void Update(CompetitionItemView itemView, Side side)
    {
        if (Competition is null)
            return;
        itemView.UpdateWith(Competition[side], $"Ratings: {Competition.RatingOf(side)?.TotalRatings ?? 0}");
    }
    private async Task UpdateViews()
    {
        await Competition!.SaveAsync();
        Update(LeftItemView, Side.Left);
        Update(RightItemView, Side.Right);
        UpdateButtonActivation();
        List<double> data = Competition!.Ratings.Select(x => x.Value.TotalRatings).Select(x => (double)x).ToList();
        data.AddRange(Competition!.RelevantUnratedItems.Select(x => 0.0));
        Histogram.ReplaceData(data);
    }
}