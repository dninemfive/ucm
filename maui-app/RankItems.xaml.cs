using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace d9.ucm;

public partial class RankItems : ContentPage
{
    public Competition? Competition = null;
    public RankItems()
    {
        InitializeComponent();
    }
    private void Left_Clicked(object sender, EventArgs e)
    {

    }
    private void Skip_Clicked(object sender, EventArgs e)
    {
        Competition.NextItems();
    }
    private void Right_Clicked(object sender, EventArgs e)
    {

    }
    private void LeftIrrelevant_Clicked(object sender, EventArgs e)
    {

    }

    private void RightIrrelevant_Clicked(object sender, EventArgs e)
    {

    }
}

