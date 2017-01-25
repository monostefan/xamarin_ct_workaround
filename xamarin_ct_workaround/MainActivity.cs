using Android.App;
using Android.OS;
using Android.Webkit;

namespace xamarin_ct_workaround
{
    [Activity(Label = "xamarin_ct_workaround", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var webView = new WebView(this);

            SetContentView(webView);

            if (WebViewVersionChecker.CurrentWebViewHasCTProblem())
            {
                var positiveText = WebViewVersionChecker.HasPlayStore(this) ? "Update now" : "I will update my WebView";

                var builder = new AlertDialog.Builder(this)
                    .SetCancelable(false)
                    .SetMessage("Your WebView provider is currently out of date. We recommend that you update to the latest version before continuing.")
                    .SetPositiveButton(positiveText, (sender, e) => WebViewVersionChecker.InvokePlayStoreToUpdateWebView(this))
                    .SetNegativeButton("Cancel", (sender, e) => { });
                builder.Create().Show();
            }

            webView.LoadUrl("https://www.amazon.com");
        }
    }
}