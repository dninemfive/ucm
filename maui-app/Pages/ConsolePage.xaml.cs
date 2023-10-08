using d9.utl;

namespace d9.ucm;
public partial class ConsolePage : ContentPage, IConsole
{
	public ConsolePage()
	{
		InitializeComponent();
	}
    public void Write(object? obj)
    {
        if(!LabelHolder.Any() || LabelHolder.Last() is not Label)
        {
            Label label = new() { Text = $"{obj}" };
            LabelHolder.Add(label);
        }
        else
        {
            Label last = (LabelHolder.Last() as Label)!;
            last.Text += $"{obj}";
        } 
    }

    public void WriteLine(object? obj)
    {
        Write($"{obj}\n");
    }
}