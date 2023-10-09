using d9.utl;

namespace d9.ucm;
public partial class ConsolePage : ContentPage, IConsole
{
	public ConsolePage()
	{
		InitializeComponent();
	}
    public async void Write(object? obj)
    {
        if(!LabelHolder.Any() || LabelHolder.Last() is not Label)
        {
            // todo: get monospaced font
            Label label = new() { Text = $"{obj}" };
            LabelHolder.Add(label);
        }
        else
        {
            Label last = (LabelHolder.Last() as Label)!;
            last.Text += $"{obj}";
        }
        if (_autoScroll)
            await ScrollBar.ScrollToAsync(0, ScrollBar.ScrollSpace(), false);
    }

    public void WriteLine(object? obj)
    {
        Write($"{obj}\n");
    }
    private bool _autoScroll = true;
    private void ScrollBar_Scrolled(object sender, ScrolledEventArgs e)
    {
        _autoScroll = e.ScrollY >= ScrollBar.ScrollSpace();
    }
}