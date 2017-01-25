using System;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Webkit;

namespace xamarin_ct_workaround
{
    public static class WebViewVersionChecker
    {
        private static readonly Regex WebViewVersionRegex = new Regex("(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)");

        private static readonly int[] AffectedWebViewVersions = { 53, 54 };

        public static bool CurrentWebViewHasCTProblem()
        {
            int webViewMajor = GetWebViewMajorVersion();

            return Array.IndexOf(AffectedWebViewVersions, webViewMajor) != -1;
        }

        private static int GetWebViewMajorVersion()
        {
            var packageInfo = GetWebViewPackageInfo();
            if (packageInfo == null)
                return -1;

            var versionString = packageInfo.VersionName;
            var versionMatch = WebViewVersionRegex.Match(versionString);
            if (versionMatch.Success)
            {
                return int.Parse(versionMatch.Groups[1].Value);
            }
            else
            {
                return -1;
            }
        }

        private static PackageInfo GetWebViewPackageInfo()
        {
            try
            {
                // If we're on K or below, just return null
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                    return null;
                // Force WebView provider to be loaded.
                var cookieManager = CookieManager.Instance;
                var factory = Java.Lang.Class.ForName("android.webkit.WebViewFactory");
                var f = factory.GetDeclaredField("sPackageInfo");
                f.Accessible = true;
                return (PackageInfo)f.Get(null);
            }
            catch (Exception)
            {
                // just say we don't know the version.
                return null;
            }
        }

        public static bool HasPlayStore(Context context)
        {
            try
            {
                context.PackageManager.GetPackageInfo("com.android.vending", PackageInfoFlags.Activities);
            }
            catch (PackageManager.NameNotFoundException)
            {
                return false;
            }

            return true;
        }

        private static string[] UpdatablePackages =
        {
            "com.google.android.webview",
            "com.android.chrome",
            "com.chrome.beta",
            "com.chrome.canary",
            "com.chrome.dev"
        };

        public static bool InvokePlayStoreToUpdateWebView(Context context)
        {
            if (!HasPlayStore(context))
                return false;
            
            var packageInfo = GetWebViewPackageInfo();
            if (packageInfo == null)
                return false;
            
            string webViewPackageName = packageInfo.PackageName;

            if (Array.IndexOf(UpdatablePackages, webViewPackageName) != -1)
            {
                try
                {
                    context.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("market://details?id=" + webViewPackageName)));
                }
                catch (ActivityNotFoundException)
                {
                    // If we reach here, it must mean that something is preventing us from invoking
                    // the Play store, so we'll assume it's unreachable
                    return false;
                }
                return true;
            }

            return false;
        }
    }
}
