namespace d9.ucm;

public partial class NavigationView : ContentView
{
    public class EventArgs : System.EventArgs
    {
        public int ResultPage { get; private set; }
        public EventArgs(int resultPage)
        {
            ResultPage = resultPage;
        }
        public override string ToString() => $"{ResultPage}";
#pragma warning disable IDE0001 // "name can be simplified": intentionally using full name to clarify
        public static implicit operator NavigationView.EventArgs(int z) => new(z);
        public static implicit operator int(NavigationView.EventArgs nvea) => nvea.ResultPage;
    }
    public delegate void EventHandler(NavigationView.EventArgs e);
    public event NavigationView.EventHandler? Navigated;
#pragma warning restore IDE0001
    private int _maxPage = 0;
    public int MaxPage
    {
        get => _maxPage;
        set
        {
            
            _maxPage = value;
            if (CurrentIndex > value)
            {
                CurrentIndex = value;
            }
            CorrectButtonCount();
        }
    }
    private int _maxNumericalButtons = 7;
    public int MaxNumericalButtons
    {
        get => _maxNumericalButtons;
        set
        {
            _maxNumericalButtons = value;
            CorrectButtonCount();
        }
    }
    private int _currentIndex = 0;
    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (value < 0 || value > MaxPage || value == _currentIndex)
                return;
            _currentIndex = value;
            Navigated?.Invoke(value);
            UpdateButtonIndices();
        }
    }
    public NavigationView()
	{
		InitializeComponent();
        Utils.Log($"NavigationView()");
	}
    private void FirstItemButton_Clicked(object sender, System.EventArgs e) => CurrentIndex = 0;
    private void PreviousItemButton_Clicked(object sender, System.EventArgs e) => CurrentIndex--;
    private void NextItemButton_Clicked(object sender, System.EventArgs e) => CurrentIndex++;
    private void LastItemButton_Clicked(object sender, System.EventArgs e) => CurrentIndex = MaxPage;
    public void UpdateButtonActivation()
    {
        Utils.Log($"UpdateButtonActivation()");
        FirstItemButton.IsEnabled = CurrentIndex > 0;
        PreviousItemButton.IsEnabled = CurrentIndex > 0;
        NextItemButton.IsEnabled = CurrentIndex < MaxPage;
        LastItemButton.IsEnabled = CurrentIndex < MaxPage;
        foreach(IView iv in NumericalButtonHolder.Children)
        {
            if (iv is Button b)
                b.IsEnabled = IndexOf(b) != CurrentIndex; 
        }
    }
    public void AddButton(int index)
    {
        Utils.Log($"AddButton({index})");
        Button button = new()
        {
            Text = $"{index + 1}",
            IsEnabled = CurrentIndex != index,
            WidthRequest = 40
        };
        button.Clicked += (sender, e) => CurrentIndex = IndexOf((sender as Button)!);
        NumericalButtonHolder.Add(button);
    }
    private static int IndexOf(Button b) => int.Parse(b.Text) - 1;
    public void CorrectButtonCount()
    {
        
        int initialCt = NumericalButtonHolder.Children.Count, target = Math.Min(MaxPage, MaxNumericalButtons);
        Utils.Log($"CorrectButtonCount({initialCt} -> {target})");
        if (initialCt < target)
        {
            for (int i = NumericalButtonHolder.Children.Count; i < Math.Min(MaxPage, MaxNumericalButtons); i++)
                AddButton(i);
        }
        else if(initialCt > target)
        {
            for (int i = 0; i < initialCt - target; i++)
                NumericalButtonHolder.Children.RemoveAt(0);
        }
        UpdateButtonIndices();
    }
    private void UpdateButtonIndices()
    {
        Utils.Log($"UpdateButtonIndices()");
        int min = _currentIndex - MaxNumericalButtons / 2,
            buttonCt = NumericalButtonHolder.Children.Count;        
        if(min + buttonCt > MaxPage)
        {
            min = MaxPage - buttonCt;
        }
        if (min < 0)
        {
            min = 0;
        }
        int index = min;
        foreach (Button b in NumericalButtonHolder.Children.OfType<Button>())
        {
            b.Text = $"{index + 1}";
            index++;
        }
        UpdateButtonActivation();
    }
}
