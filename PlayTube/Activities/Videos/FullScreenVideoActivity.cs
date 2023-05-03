using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Core.OS;
using PlayTube.Activities.Base;
using PlayTube.Activities.Models;
using PlayTube.Helpers.Utils;
using PlayTubeClient.Classes.Global;

namespace PlayTube.Activities.Videos
{
    //weal fix full page
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class FullScreenVideoActivity : AppCompatActivity
    { 
        private static VideoDataObject VideoData;
        public VideoDataWithEventsLoader VideoDataWithEventsLoader;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this, true);
                 
                var type = Intent?.GetStringExtra("type") ?? "";
                if (type == "RequestedOrientation")
                {
                    //ScreenOrientation.Portrait >>  Make to run your application only in portrait mode
                    //ScreenOrientation.Landscape >> Make to run your application only in LANDSCAPE mode 
                    RequestedOrientation = ScreenOrientation.Landscape;
                }

                SetContentView(Resource.Layout.FullScreenDialogLayout);
                InitBackPressed();

                //VideoActionsController = new VideoController(this, "FullScreen"); 
                //VideoActionsController.PlayFullScreen(VideoData);
               
                //if (Intent?.GetStringExtra("Downloaded") == "Downloaded")
                //    VideoActionsController.DownloadIcon.SetImageDrawable(GetDrawable(Resource.Drawable.ic_checked_red)); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InitBackPressed()
        {
            try
            {
                if (BuildCompat.IsAtLeastT && Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, new BackCallAppBase2(this, "FullScreenVideoActivity"));
                }
                else
                {
                    OnBackPressedDispatcher.AddCallback(new BackCallAppBase1(this, "FullScreenVideoActivity", true));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            { 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        public void BackPressed()
        {
           // VideoActionsController.InitFullscreenDialog("","Close");
            Finish();
        }

        public static void SetVideoData(VideoDataObject videoObject)
        {
            VideoData = videoObject;
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            { 
                if (newConfig.Orientation == Orientation.Landscape)
                {
                }
                else if (newConfig.Orientation == Orientation.Portrait)
                {
                   // VideoActionsController.InitFullscreenDialog("","Close");
                }
                base.OnConfigurationChanged(newConfig);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


    }
}