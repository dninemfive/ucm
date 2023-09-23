using CurlThin;
using CurlThin.Enums;
using CurlThin.Native;
using CurlThin.SafeHandles;
using d9.utl;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace d9.ucm;

public partial class MainPage : ContentPage
{
	private HttpClient _client;
	public MainPage()
	{
        InitializeComponent();
		string rulePath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\urlrule.json.secret",
			   bookmarksPath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json",
			   keyPath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\Keys\test.key";
        UrlRule rule = JsonSerializer.Deserialize<UrlRule>(File.ReadAllText(rulePath))!;
		List<string> bookmarks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(bookmarksPath))!;
		string[] key = File.ReadAllLines(keyPath);
		_client = new();
		foreach(string s in bookmarks)
		{
			if (rule.Supports(s))
			{
				Text.Text = rule.ApiUrl(s, ("username", key[1]), ("key", key[0]));
			}
            break;
		}
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {

		Text.Text = CurlGet(Text.Text); // await (await _client.GetAsync(Text.Text)).Content.ReadAsStringAsync();
    }
    /// <summary>
    /// https://github.com/stil/CurlThin#get-request
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private string? CurlGet(string url)
	{
		CURLcode code = CurlNative.Init();
		SafeEasyHandle handle = CurlNative.Easy.Init();
		try
		{
			_ = CurlNative.Easy.SetOpt(handle, CURLoption.URL, url);
			MemoryStream stream = new();
			_ = CurlNative.Easy.SetOpt(handle, CURLoption.WRITEFUNCTION, (data, size, nmemb, user) =>
			{
				int length = (int)size * (int)nmemb;
				byte[] buffer = new byte[length];
				Marshal.Copy(data, buffer, 0, length);
				stream.Write(buffer, 0, length);
				return (UIntPtr)length;
			});
            // https://github.com/stil/CurlThin/issues/8
            _ = CurlNative.Easy.SetOpt(handle, CURLoption.CAINFO, CurlResources.CaBundlePath);
            CURLcode result = CurlNative.Easy.Perform(handle);
			return $"Result code: {result}.\n\nResponse body:\n{Encoding.UTF8.GetString(stream.ToArray())}"
				.Replace(">",">\n").Replace(",",",\n").Replace("{", "{\n").Replace("}","\n}\n");
        }
		finally
		{
			handle.Dispose();
			if (code == CURLcode.OK)
				CurlNative.Cleanup();
		}
	}
}

