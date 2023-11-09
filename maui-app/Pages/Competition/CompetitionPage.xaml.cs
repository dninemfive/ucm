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
            LeftItemView.Competition = Competition;
            RightItemView.Competition = Competition;
            Competition.NextItems();
            await UpdateViews();
        }        
    }
    public void Update(CompetitionItemView itemView)
    {
        itemView.WidthRequest = Window.Width / 2;
        itemView.HeightRequest = Height - BottomDock.HeightRequest - CompetitionCreation.HeightRequest;
        itemView.Update();
        Utils.Log($"ItemView HeightRequest {itemView.HeightRequest}");
    }
    private async Task UpdateViews()
    {
        BottomDock.HeightRequest = SkipButton.HeightRequest + Histogram.HeightRequest;
        UpdateChildrenLayout();
        await Competition!.SaveAsync();
        Update(LeftItemView);
        Update(RightItemView);
        await UpdateButtonActivation();
        Histogram.ReplaceData(Competition!.ShownRatings.Select(x => (double)x.TotalRatings), 1);
    }
    private async void UpdateViews(object? sender, EventArgs e) => await UpdateViews();

    private void HistogramModePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Histogram.BinHeightFunction = HistogramModePicker.SelectedIndex switch
        {
            0 => null,
            1 => Competition.Rating.WeightFunction,
            _ => throw new Exception("Selected index out of range.")
        };
        Histogram.UseProportion = HistogramModePicker.SelectedIndex == 1;
        Histogram.Update();
    }

    private void ThresholdEntry_Completed(object sender, EventArgs e)
    {
        Competition!.ThresholdPercentile = double.Parse(ThresholdEntry.Text);
        Utils.Log($"0-rating item count: {Competition.ShownRatings.Where(x => x.TotalRatings == 0).Count()}");
        Histogram.ReplaceData(Competition!.ShownRatings.Select(x => (double)x.TotalRatings), 1);
    }
}