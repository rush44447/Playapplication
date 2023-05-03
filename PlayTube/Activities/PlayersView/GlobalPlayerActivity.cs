 using System;
 using Android.App;
using Android.Content;
using Android.Content.PM;
 using Android.Content.Res;
 using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
 using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Activities.Models;
using PlayTube.Helpers.Utils;
using PlayTubeClient.Classes.Global;

 namespace PlayTube.Activities.PlayersView
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode , LaunchMode = LaunchMode.SingleTask ,ResizeableActivity = true, SupportsPictureInPicture = true)]
    public class GlobalPlayerActivity : BaseActivity 
    {
        #region Variables Basic

        private static GlobalPlayerActivity Instance;
        public static bool OnOpenPage;
        private bool OnStopCalled;

        public VideoDataWithEventsLoader VideoDataWithEventsLoader;
         
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.VideoSliderLayout);
               
                Instance = this;
                OnOpenPage = true;
                InitBackPressed("GlobalPlayerActivity");

                VideoDataWithEventsLoader = new VideoDataWithEventsLoader(this, "GlobalPlayerActivity");
                 
                var VideoData = JsonConvert.DeserializeObject<VideoDataObject>(Intent?.GetStringExtra("VideoObject") ?? "");
                VideoDataWithEventsLoader.LoadDataVideo(VideoData);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                VideoDataWithEventsLoader?.AddOrRemoveEvent(true); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                VideoDataWithEventsLoader?.AddOrRemoveEvent(false);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                OnStopCalled = true;
                base.OnStop();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                VideoDataWithEventsLoader?.OnDestroy();

                OnOpenPage = false;

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            try
            {
                base.OnNewIntent(intent);

                var VideoData = JsonConvert.DeserializeObject<VideoDataObject>(Intent?.GetStringExtra("VideoObject") ?? "");
                VideoDataWithEventsLoader?.NewLoad(VideoData);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions
         
        public static GlobalPlayerActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }
         
        #endregion

        #region Back Pressed

        private bool IsPipModeEnabled = true;
        public void BackPressed()
        {
            try
            {
                if (VideoDataWithEventsLoader.TubePlayerView != null && VideoDataWithEventsLoader.TubePlayerView.FullScreen)
                {
                    VideoDataWithEventsLoader?.TubePlayerView?.ExitFullScreen();
                    return;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.N && PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture) && IsPipModeEnabled)
                {
                    switch (VideoDataWithEventsLoader?.VideoType)
                    {
                        case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                            EnterPipMode();
                            break;
                        case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                            EnterPipMode();
                            //Finish();
                            break;
                    }
                }
                else
                {
                    Finish();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                Finish();
            }
        }

        private void EnterPipMode()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N && PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture))
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        Rational rational = new Rational(450, 250);
                        PictureInPictureParams.Builder builder = new PictureInPictureParams.Builder();
                        builder.SetAspectRatio(rational);
                        EnterPictureInPictureMode(builder.Build());
                    }
                    else
                    {
                        var param = new PictureInPictureParams.Builder().Build();
                        EnterPictureInPictureMode(param);
                    }

                    new Handler(Looper.MainLooper).PostDelayed(CheckPipPermission, 30);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CheckPipPermission()
        {
            try
            {
                IsPipModeEnabled = IsInPictureInPictureMode;
                if (!IsInPictureInPictureMode)
                {
                    BackPressed();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion
         
        #region Permissions && Result

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                switch (requestCode)
                {
                    case 2000 when resultCode == Result.Ok:
                    {
                         VideoDataWithEventsLoader?.ExoController?.RestartPlayAfterShrinkScreen();
                            break;
                    }
                    case 2100 when resultCode == Result.Ok:
                    {
                        VideoDataWithEventsLoader?.TubePlayerView?.ExitFullScreen();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        VideoDataWithEventsLoader?.DownloadVideo();
                        break;
                    case 100:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region PictureInPictur
         
        public override void OnPictureInPictureModeChanged(bool isInPictureInPictureMode, Configuration newConfig)
        {
            try
            {
                VideoDataWithEventsLoader.CoordinatorLayoutView.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;

                switch (VideoDataWithEventsLoader.VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        if (VideoDataWithEventsLoader?.ExoController?.GetControlView() != null)
                            VideoDataWithEventsLoader.ExoController.GetControlView().Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        if (isInPictureInPictureMode)
                            VideoDataWithEventsLoader.TubePlayerView?.PlayerUiController.ShowUi(false);
                        else
                            VideoDataWithEventsLoader.TubePlayerView.PlayerUiController.ShowUi(true);
                        break;
                }

                VideoDataWithEventsLoader.ExoController.HideControls(isInPictureInPictureMode);

                if (VideoDataWithEventsLoader.ExoController?.GetControlView() != null)
                    VideoDataWithEventsLoader.ExoController.GetControlView().Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
                  
                if (isInPictureInPictureMode)
                {
                    // ...
                    switch (VideoDataWithEventsLoader.VideoType)
                    {
                        case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                            //VideoDataWithEventsLoader.VideoActionsController?.OnStop();
                            break;
                        case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                            //VideoDataWithEventsLoader.YoutubePlayer.Play();
                            break;
                    }
                }
                else
                {
                    if (OnStopCalled)
                    {
                        switch (VideoDataWithEventsLoader.VideoType)
                        {
                            case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                                VideoDataWithEventsLoader.OnStop();
                                break;
                            case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                                VideoDataWithEventsLoader.YoutubePlayer.Pause();
                                VideoDataWithEventsLoader.TubePlayerView.PlayerUiController.ShowUi(true);
                                break;
                        }

                        VideoDataWithEventsLoader.FinishActivityAndTask();
                    }
                }

                base.OnPictureInPictureModeChanged(isInPictureInPictureMode, newConfig);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnUserLeaveHint()
        {
            try
            {
                switch (VideoDataWithEventsLoader.VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var param = new PictureInPictureParams.Builder().Build();
                            EnterPictureInPictureMode(param);
                        }
                        base.OnUserLeaveHint();
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}