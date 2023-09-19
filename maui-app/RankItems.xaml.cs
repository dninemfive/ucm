using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace d9.ucm;

public partial class RankItems : ContentPage
{
    public Competition? Competition = null;
    public string CompetitionFileName => $"{MauiProgram.TEMP_SAVE_LOCATION}/competitions/{CompetitionName}.json";
    public RankItems()
    {
        InitializeComponent();
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
        LeftItemHolder.Content.Opacity = 0.8;
        RightRating.Text = "(irrelevant)";
        SelectRight.IsVisible = false;
        SelectLeft.IsVisible = false;
    }
    private void RightIrrelevant_Clicked(object sender, EventArgs e)
    {
        Competition!.MarkIrrelevant(Side.Right);
        LeftItemHolder.Content.Opacity = 0.8;
        LeftRating.Text = "(irrelevant)";
        SelectRight.IsVisible = false;
        SelectLeft.IsVisible = false;
    }
    private async void CreateCompetition_Clicked(object sender, EventArgs e)
    {
        Competition = await Competition.LoadOrCreateAsync(CompetitionName.Text);
        CompetitionCreation.IsVisible = false;
        RatingScreen.IsVisible = true;
        await UpdateViews();
    }
    private void CompetitionName_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(File.Exists(CompetitionName.Text))
        {
            CreateCompetition.Text = "Load";
        } 
        else
        {
            CreateCompetition.Text = "Create";
        }
    }
    private async Task UpdateViews()
    {
        await Competition!.SaveAsync();
        SelectLeft.IsVisible = true;
        SelectRight.IsVisible = true;
        LeftItemHolder.Content = Competition.Left.View;
        LeftRating.Text = Competition.RatingOf(Side.Left)?.ToString() ?? "0/0";
        LeftPath.Text = Competition.Left.Path;
        RightItemHolder.Content = Competition.Right.View;
        RightRating.Text = Competition.RatingOf(Side.Right)?.ToString() ?? "0/0";
        RightPath.Text = Competition.Right.Path;
    }
}