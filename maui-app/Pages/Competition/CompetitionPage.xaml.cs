﻿using d9.utl;
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
        await UpdateButtonActivation();
        List<double> data = Competition!.Ratings.Select(x => x.Value.TotalRatings).Select(x => (double)x).ToList();
        data.AddRange(Competition!.RelevantUnratedItems.Select(x => 0.0));
        Histogram.ReplaceData(data);
    }
}