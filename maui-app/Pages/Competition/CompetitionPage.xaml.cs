using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace d9.ucm;

public partial class CompetitionPage : ContentPage
{
    public Competition? Competition = null;
    public string CompetitionFileName => $"{MauiProgram.TEMP_SAVE_LOCATION}/competitions/{CompetitionName}.json";
    public CompetitionPage()
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
        LeftItemHolder.Content.Opacity = 0.4;
        LeftRating.Text = "(irrelevant)";
        SelectRight.IsVisible = false;
        SelectLeft.IsVisible = false;
        LeftIrrelevant.IsVisible = false;
    }
    private void RightIrrelevant_Clicked(object sender, EventArgs e)
    {
        Competition!.MarkIrrelevant(Side.Right);
        RightItemHolder.Content.Opacity = 0.4;
        RightRating.Text = "(irrelevant)";
        SelectRight.IsVisible = false;
        SelectLeft.IsVisible = false;
        RightIrrelevant.IsVisible = false;
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
        if(File.Exists(Competition.PathFor(CompetitionName.Text)))
        {
            CreateCompetition.Text = "Load";
        } 
        else
        {
            CreateCompetition.Text = "Create";
        }
    }
    public void UnhideButtons()
    {
        SelectLeft.IsVisible = true;
        SelectRight.IsVisible = true;
        LeftIrrelevant.IsVisible = true;
        RightIrrelevant.IsVisible = true;
    }
    public void UpdateLeftItem()
    {
        if (Competition is null)
            return;
        LeftItemHolder.Content = Competition.Left.View;
        ToolTipProperties.SetText(LeftItemHolder, Competition.Left.Path);
        LeftRating.Text = Competition.RatingOf(Side.Left)?.ToString() ?? "-";
    }
    public void UpdateRightItem()
    {
        if (Competition is null)
            return;
        RightItemHolder.Content = Competition.Right.View;
        ToolTipProperties.SetText(RightItemHolder, Competition.Right.Path);
        RightRating.Text = Competition.RatingOf(Side.Right)?.ToString() ?? "-";
    }
    private async Task UpdateViews()
    {
        await Competition!.SaveAsync();
        UnhideButtons();
        UpdateLeftItem();
        UpdateRightItem();        
    }
}