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
        LeftItemView.IrrelevantButtonClicked += UpdateButtonActivation;
        RightItemView.IrrelevantButtonClicked += UpdateButtonActivation;
        LeftItemView.SelectButtonClicked += UpdateViews;
        RightItemView.SelectButtonClicked += UpdateViews;
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
    private async void Skip_Clicked(object? sender, EventArgs e)
        => await Skip();
    private async Task UpdateButtonActivation()
    {
        bool eitherIrrelevant = LeftItemView.IsIrrelevant || RightItemView.IsIrrelevant;
        LeftItemView.Selectable = !eitherIrrelevant;
        RightItemView.Selectable = !eitherIrrelevant;
        if (LeftItemView.IsIrrelevant && RightItemView.IsIrrelevant)
        {
            await Skip();
            return;
        }
    }
    private async void UpdateButtonActivation(object? sender, EventArgs e) => await UpdateButtonActivation();
    private async void CompetitionCreated(object? sender, EventArgs e)
    {
        ItemViews.IsVisible = Competition is not null;
        BottomDock.IsVisible = Competition is not null;
        if(Competition is not null)
        {
            CompetitionCreation.IsVisible = false;
            Competition.NextItems();            
            await UpdateViews();
        }        
    }
    public void Update(CompetitionItemView itemView, Side side)
    {
        if (Competition is null)
            return;        
        itemView.WidthRequest = Window.Width / 2;
        itemView.HeightRequest = Height - BottomDock.HeightRequest;
        itemView.Competition = Competition;
        itemView.UpdateWith(Competition[side], $"Ratings: {Competition.RatingOf(side)?.TotalRatings ?? 0}");
    }
    private async Task UpdateViews()
    {
        BottomDock.HeightRequest = SkipButton.HeightRequest + Histogram.HeightRequest;
        UpdateChildrenLayout();
        await Competition!.SaveAsync();
        Update(LeftItemView, Side.Left);
        Update(RightItemView, Side.Right);
        await UpdateButtonActivation();
        List<double> data = Competition!.RelevantRatings.Select(x => x.CiUpperBound).ToList();
        //data.AddRange(Competition!.RelevantUnratedItems.Select(x => 0.0));
        Histogram.ReplaceData(data);
    }
    private async void UpdateViews(object? sender, EventArgs e) => await UpdateViews();
}