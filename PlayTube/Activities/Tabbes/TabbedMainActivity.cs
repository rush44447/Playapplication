﻿//###############################################################
// Author >> Elin Doughouz
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Google.Android.Material.Dialog;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.CardView.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Content;
using AndroidX.SlidingPaneLayout.Widget;
using Com.Sothree.Slidinguppanel;
using Google.Android.Material.AppBar;
using Newtonsoft.Json;
using PlayTube.Activities.Channel;
using PlayTube.Activities.Chat;
using PlayTube.Activities.Comments;
using PlayTube.Activities.Live.Page;
using PlayTube.Activities.Live.Utils;
using PlayTube.Activities.Models;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Playlist;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.SettingsPreferences.General;
using PlayTube.Activities.Tabbes.Fragments;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Helpers.Views;
using PlayTube.Service;
using PlayTube.SQLite;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.Classes.Messages;
using PlayTubeClient.Classes.Playlist;
using PlayTubeClient.Classes.Video;
using PlayTubeClient.RestCalls;
using Q.Rorbin.Badgeview;
using Console = System.Console;
using Constants = PlayTube.Activities.Live.Page.Constants;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using PlayTube.Activities.Search;
using AndroidX.Core.OS;
using AndroidX.Lifecycle;
using PlayTube.Activities.Base;
using YouTubePlayerAndroidX.Player;
using Orientation = Android.Content.Res.Orientation;
using Com.Google.Android.Exoplayer2;

namespace PlayTube.Activities.Tabbes
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale, SupportsPictureInPicture = true)]
    public class TabbedMainActivity : AppCompatActivity, SlidingPaneLayout.IPanelSlideListener, SlidingUpPanelLayout.IPanelSlideListener, View.IOnTouchListener, View.IOnClickListener
    {
        #region Variables Basic

        private static TabbedMainActivity Instance;
        public SlidingUpPanelLayout SlidingUpPanel;
        private AppBarLayout AppBarLayoutView;
        public HomeFragment HomeFragment;
        public TrendingFragment TrendingFragment;
        public LibraryFragment LibraryFragment;  
        public MyChannelFragment MyChannelFragment;
        public UserChannelFragment UserChannelFragment;
        public VideoDataWithEventsLoader VideoDataWithEventsLoader;
        private CoordinatorLayout CoordinatorLayoutView;
        public LibrarySynchronizer LibrarySynchronizer;
        private PowerManager.WakeLock Wl;
        private readonly Handler ExitHandler = new Handler(Looper.MainLooper);
        private bool RecentlyBackPressed;
        public CustomNavigationController FragmentBottomNavigator;
        private FrameLayout NavigationTabBar;
        private CardView VideoButtomStyle;
        private FrameLayout MainVideoRoot;
        public TextView VideoTitleText, VideoChannelText;
        private CustomTouchLayout VideoSmallFrameLayout;
        private ImageView IconPlayPlayer, IconClosePlayer;
        private LinearLayout IconPlayerLayout;
        private float X, Dx;
        private LinearLayout VideoTextContainer;
        private bool IsBottomVideoViewShowing;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);

                Task.Factory.StartNew(() => MainApplication.GetInstance()?.SecondRunExcite());
                Methods.App.FullScreenApp(this);

                Delegate.SetLocalNightMode(AppTools.IsTabDark() ? AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo);
                SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.TabbedMainLayout);
                 
                Instance = this;

                VideoDataWithEventsLoader = new VideoDataWithEventsLoader(this, "TabbedMainActivity");

                //Get Value And Set Toolbar
                InitComponent(); 
                //SetupFloatingActionMenus();
                SetupBottomNavigationView();
                InitBackPressed();

                Task.Factory.StartNew(GetGeneralAppData); 

                if (UserDetails.IsLogin)
                    SetService();

                GetDataOneSignal();
                   
                if (AppSettings.EnablePictureToPictureMode)
                {
                    var pipIsChecked = AppTools.CheckPictureInPictureAllowed(this);

                    if (PackageManager != null && (!PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture) || !pipIsChecked))
                    {
                        var intent = new Intent("android.settings.PICTURE_IN_PICTURE_SETTINGS", Android.Net.Uri.Parse("package:" + PackageName));
                        StartActivityForResult(intent, 8520);
                    }
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                ToggleKeepSceenOnFeature(false);
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
                ToggleKeepSceenOnFeature(false);
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
                base.OnStop();

                //Called onNewIntent
                VideoDataWithEventsLoader?.OnStop(); 
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
                
                base.OnDestroy(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                switch (newConfig.Orientation)
                {
                    case Orientation.Landscape:
                        VideoDataWithEventsLoader?.UpdateMainRootDefaultLandscapeSize();
                        break;
                    case Orientation.Portrait:
                        VideoDataWithEventsLoader?.UpdateMainRootDefaultSize();
                        break;
                }
                 
                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        MainSettings.ApplyTheme(MainSettings.LightMode);
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                        break;
                }

                SetTheme(AppTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                VideoDataWithEventsLoader?.TubePlayerView?.PlayerUiController?.Menu?.Dismiss();
                base.OnConfigurationChanged(newConfig);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
               //var mainContainerView = FindViewById<RelativeLayout>(Resource.Id.MainContainerView);
                CoordinatorLayoutView = FindViewById<CoordinatorLayout>(Resource.Id.parent);
                NavigationTabBar = FindViewById<FrameLayout>(Resource.Id.buttomnavigationBar);
                VideoSmallFrameLayout = FindViewById<CustomTouchLayout>(Resource.Id.Vcontainer);
                MainVideoRoot = FindViewById<FrameLayout>(Resource.Id.Mainroot);
                VideoTitleText = FindViewById<TextView>(Resource.Id.videoTitileText);
                VideoChannelText = FindViewById<TextView>(Resource.Id.videoChannelText);

                IconPlayerLayout = FindViewById<LinearLayout>(Resource.Id.iconPlayerLayout);
                IconPlayPlayer = FindViewById<ImageView>(Resource.Id.iconPlayPlayer);
                IconClosePlayer = FindViewById<ImageView>(Resource.Id.iconClosePlayer);
                IconPlayPlayer.ImageTintList = ColorStateList.ValueOf(AppTools.IsTabDark() ? Color.White : Color.Black);
                IconClosePlayer.ImageTintList = ColorStateList.ValueOf(AppTools.IsTabDark() ? Color.White : Color.Black);

                VideoButtomStyle = FindViewById<CardView>(Resource.Id.VideoButtomStyle);
                VideoTextContainer = FindViewById<LinearLayout>(Resource.Id.videoTextcontainer);
                AppBarLayoutView = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                SlidingUpPanel = FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);
                SlidingUpPanel.AddPanelSlideListener(this);
               
                IconPlayPlayer.Click += IconPlayPlayerOnClick;
                IconClosePlayer.Click += IconClosePlayerOnClick;

                SlidingUpPanel.Tag = "Hidden";
                //AppBarLayoutView.AddOnOffsetChangedListener(this);

                LibrarySynchronizer = new LibrarySynchronizer(this);


                VideoButtomStyle.SetOnClickListener(this);
                VideoButtomStyle.SetOnTouchListener(this);

                if (AppSettings.PlayerTheme == PlayerTheme.Theme1)
                {
                    IconPlayerLayout.Visibility = ViewStates.Gone;
                    var pix = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Resources.DisplayMetrics);
                    Methods.SetMargin(VideoButtomStyle, pix, 0, pix, 0);
                    VideoButtomStyle.Radius = TypedValue.ApplyDimension(ComplexUnitType.Dip, 11, Resources.DisplayMetrics);
                }
                else if (AppSettings.PlayerTheme == PlayerTheme.Theme2)
                {
                    IconPlayerLayout.Visibility = ViewStates.Visible;
                    //var pix = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics);
                    Methods.SetMargin(VideoButtomStyle, 0, 0, 0, 0);
                    VideoButtomStyle.Radius = TypedValue.ApplyDimension(ComplexUnitType.Dip, 0, Resources.DisplayMetrics);
                   // VideoSmallFrameLayout.SetMinimumWidth((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 120, Resources.DisplayMetrics));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitBackPressed()
        {
            try
            {
                if (BuildCompat.IsAtLeastT && Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, new BackCallAppBase2(this, "TabbedMainActivity"));
                }
                else
                {
                    OnBackPressedDispatcher.AddCallback(new BackCallAppBase1(this, "TabbedMainActivity", true));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static TabbedMainActivity GetInstance()
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
         
        public void SetToolBar(Toolbar toolbar, string title, TextView toolbarTitle = null, bool showIconBack = true)
        {
            try
            {
                if (toolbar != null)
                {
                    if (!string.IsNullOrEmpty(title))
                        if (toolbarTitle != null)
                        {
                            toolbar.Title = " ";

                            toolbarTitle.Text = title;
                            toolbarTitle.Visibility = ViewStates.Visible;
                        }
                        else
                            toolbar.Title = title;

                    toolbar.SetTitleTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(showIconBack);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    if (showIconBack)
                    { 
                        var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                        icon?.SetTint(AppTools.IsTabDark() ? Color.White : Color.Black);
                        SupportActionBar.SetHomeAsUpIndicator(icon);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    FragmentNavigatorBack();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Events

        public void ChatButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(LastChatActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void NotificationButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                NotificationFragment notificationFragment = new NotificationFragment();
                FragmentBottomNavigator.DisplayFragment(notificationFragment);

                ShowOrHideBadgeViewNotifications();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void SearchButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchFragment searchFragment = new SearchFragment();
                FragmentBottomNavigator.DisplayFragment(searchFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        public void ProfileButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (UserDetails.IsLogin)
                {
                    MyChannelFragment = new MyChannelFragment();
                    FragmentBottomNavigator.DisplayFragment(MyChannelFragment);
                }
                else
                {
                    PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Start_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ShowUserChannelFragment(UserDataObject userDataHandler, string userid)
        {
            try
            {
                if (SlidingUpPanel != null && (SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
                {
                    ShowVideoButtomStyle();
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                
                if (userid != UserDetails.UserId)
                {
                    Bundle bundle = new Bundle();

                    if (userDataHandler != null)
                        bundle.PutString("Object", JsonConvert.SerializeObject(userDataHandler));

                    UserChannelFragment = new UserChannelFragment { Arguments = bundle }; 
                    FragmentBottomNavigator.DisplayFragment(UserChannelFragment);
                }
                else
                {
                    if (UserDetails.IsLogin)
                    {
                        MyChannelFragment = new MyChannelFragment();
                        FragmentBottomNavigator.DisplayFragment(MyChannelFragment);
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Start_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowReplyCommentFragment(dynamic comment, string type)
        {
            try
            {
                ReplyCommentBottomSheet replyFragment = new ReplyCommentBottomSheet();
                Bundle bundle = new Bundle();

                bundle.PutString("Type", type);
                bundle.PutString("Object", JsonConvert.SerializeObject(comment));
                replyFragment.Arguments = bundle;

                replyFragment.Show(SupportFragmentManager, replyFragment.Tag);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
             
        #region Live

        //Go Live
        public void GoLiveOnClick()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    OpenDialogLive();
                }
                else
                {
                    if (PermissionsController.CheckPermissionStorage() &&
                        ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessMediaLocation) == Permission.Granted &&
                        ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted &&
                        ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) == Permission.Granted &&
                        ContextCompat.CheckSelfPermission(this, Manifest.Permission.ModifyAudioSettings) == Permission.Granted)
                    {
                        OpenDialogLive();
                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.WriteExternalStorage,
                            Manifest.Permission.Camera,
                            Manifest.Permission.AccessMediaLocation,
                            Manifest.Permission.RecordAudio,
                            Manifest.Permission.ModifyAudioSettings,
                        }, 235);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void OpenDialogLive()
        {
            try
            { 
                new LiveUtil(this).GoLiveOnClick();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
          
        public void EditPlaylistOnClick(PlayListVideoObject playListVideoObject)
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditPlaylistActivity));
                intent.PutExtra("Item", JsonConvert.SerializeObject(playListVideoObject));
                intent.PutExtra("IdList", playListVideoObject.ListId);
                StartActivityForResult(intent, 528);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Set Tab

        private void SetupBottomNavigationView()
        {
            try
            {
                FragmentBottomNavigator = new CustomNavigationController(this);

                HomeFragment = new HomeFragment();
                TrendingFragment = new TrendingFragment();
                LibraryFragment = new LibraryFragment(); 

                FragmentBottomNavigator.FragmentListTab0.Add(HomeFragment);
                FragmentBottomNavigator.FragmentListTab1.Add(TrendingFragment);
                 
                //FragmentBottomNavigator.FragmentListTab2.Add();
                 
                FragmentBottomNavigator.FragmentListTab3.Add(LibraryFragment);
                
                FragmentBottomNavigator.ShowFragment0(); 
            }
            catch (Exception e)
            {
                FragmentBottomNavigator.ShowFragment0();
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void CenterButtonOnClick()
        {
            try
            {
                AddOptionVideoBottomSheet bottomSheet = new AddOptionVideoBottomSheet();
                bottomSheet.Show(SupportFragmentManager, bottomSheet.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

       
        #endregion

        public void StartPlayVideo(VideoDataObject videoObject)
        {
            try
            {
                VideoDataWithEventsLoader?.RestrictedVideoPlayerFragment.HideRestrictedInfo(true);
                VideoDataWithEventsLoader?.UpdateMainRootDefaultSize();

                if (ListUtils.ArrayListPlay.Count > 0 && ListUtils.ArrayListPlay.Contains(videoObject))
                    ListUtils.ArrayListPlay.Remove(videoObject);
                
                if (AppSettings.EnablePictureToPictureMode && UserDetails.PipIsChecked)
                {
                    if (GlobalPlayerActivity.OnOpenPage)
                    {
                        GlobalPlayerActivity.OnOpenPage = false;
                        GlobalPlayerActivity.GetInstance()?.VideoDataWithEventsLoader?.FinishActivityAndTask();
                    }
                    var intent = new Intent(this, typeof(GlobalPlayerActivity));
                    intent.PutExtra("VideoObject", JsonConvert.SerializeObject(videoObject));
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask);
                    intent.AddCategory(Intent.CategoryDefault);
                    intent.AddFlags(ActivityFlags.ReorderToFront);
                    intent.AddFlags(ActivityFlags.SingleTop);
                    StartActivityForResult(intent, 5000);
                    return;
                }
               
                VideoDataWithEventsLoader?.LoadDataVideo(videoObject); 
                 
                if (SlidingUpPanel != null && SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Collapsed || SlidingUpPanel?.GetPanelState() == SlidingUpPanelLayout.PanelState.Hidden || SlidingUpPanel?.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded)
                {
                    HideVideoButtomStyle();
                    SlidingUpPanel?.SetPanelState(SlidingUpPanelLayout.PanelState.Expanded);

                    VideoDataWithEventsLoader.ExoController.ExoBackButton.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                    VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag = "Open"; 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            } 
        }

        public void StopFragmentVideo()
        {
            try
            {
                if (SlidingUpPanel != null && SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Collapsed)
                {

                    VideoDataWithEventsLoader?.GlobalVideosRelease("All");

                    // ..screen will stay on during this section..
                    Wl?.Release();
                    Wl = null;

                    ListUtils.LessonList = new ObservableCollection<VideoDataObject>();

                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);
                }
                else
                {
                    if (!AppSettings.EnablePictureToPictureMode || !UserDetails.PipIsChecked || !GlobalPlayerActivity.OnOpenPage) return;
                    GlobalPlayerActivity.OnOpenPage = false;
                    GlobalPlayerActivity.GetInstance()?.VideoDataWithEventsLoader?.FinishActivityAndTask();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        private void IconClosePlayerOnClick(object sender, EventArgs e)
        {
            try
            {
                HideVideoButtomStyle();

                VideoDataWithEventsLoader?.GlobalVideosRelease("All");

                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null;
                ListUtils.LessonList = new ObservableCollection<VideoDataObject>();

                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconPlayPlayerOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (VideoDataWithEventsLoader.VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        if (VideoDataWithEventsLoader.PlayerView?.Player != null && VideoDataWithEventsLoader.PlayerView.Player.PlayWhenReady)
                        {
                            IconPlayPlayer.SetImageResource(Resource.Drawable.pif_video_player_play_arrow);
                            VideoDataWithEventsLoader.PlayerView.Player.PlayWhenReady = false;
                        }
                        else
                        {
                            IconPlayPlayer.SetImageResource(Resource.Drawable.exo_icon_pause);
                            VideoDataWithEventsLoader.PlayerView.Player.PlayWhenReady = true;
                        }

                        VideoDataWithEventsLoader.PlayerView.HideController();
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        if (VideoDataWithEventsLoader.YouTubePlayerEvents != null && VideoDataWithEventsLoader.YoutubePlayer != null && VideoDataWithEventsLoader.YouTubePlayerEvents.IsPlaying)
                        {
                            IconPlayPlayer.SetImageResource(Resource.Drawable.pif_video_player_play_arrow);
                            VideoDataWithEventsLoader.YoutubePlayer?.Pause();
                        }
                        else
                        {
                            IconPlayPlayer.SetImageResource(Resource.Drawable.exo_icon_pause);
                            VideoDataWithEventsLoader.YoutubePlayer?.Play();
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Event Back

        public void BackIcon_Click()
        {
            try
            {
                if (SlidingUpPanel != null && (SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
                {
                    ShowVideoButtomStyle();
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                else if (SlidingUpPanel != null && SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    VideoButtomStyle?.Animate()?.TranslationX(-VideoButtomStyle.Width)?.TranslationY(0)?.SetDuration(300);
                    VideoDataWithEventsLoader?.GlobalVideosRelease("All");
                    StopFragmentVideo();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void BackPressed()
        {
            try
            {
                if (VideoDataWithEventsLoader?.TubePlayerView != null && VideoDataWithEventsLoader.TubePlayerView.FullScreen)
                {
                    VideoDataWithEventsLoader?.TubePlayerView?.ExitFullScreen();
                    return;
                }

                if (SlidingUpPanel != null && (SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded || SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Anchored))
                {
                    ShowVideoButtomStyle();
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
                else
                {
                    if (FragmentBottomNavigator.GetCountFragment() > 0)
                    {
                        FragmentNavigatorBack();
                    }
                    else
                    {
                        if (RecentlyBackPressed)
                        {
                            ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                            RecentlyBackPressed = false;
                            MoveTaskToBack(true);
                            Finish();
                        }
                        else
                        {
                            RecentlyBackPressed = true;
                            Toast.MakeText(this, GetString(Resource.String.press_again_exit), ToastLength.Long)?.Show();
                            ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void FragmentNavigatorBack()
        {
            try
            {
                FragmentBottomNavigator.OnBackStackClickFragment();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Listener Panel Layout

        private void ToggleKeepSceenOnFeature(bool keepScreenOn)
        {
            if (keepScreenOn)
            {
                SetOnWakeLock();
                AddFlagsWakeLock();
            }
            else
            {
                SetOffWakeLock();
                ClearFlagsWakeLock();
            }

            VideoDataWithEventsLoader?.ExoController?.ToggleExoPlayerKeepScreenOnFeature(keepScreenOn);
        }

        public void OnPanelClosed(View panel)
        {

        }

        public void OnPanelOpened(View panel)
        {

        }

        public void OnPanelSlide(View panel, float slideOffset)
        {
            try
            {
                NavigationTabBar.Alpha = 1 - slideOffset;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnPanelStateChanged(View p0, SlidingUpPanelLayout.PanelState p1, SlidingUpPanelLayout.PanelState p2)
        {
            try
            {
                if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag?.ToString() == "Close")
                    {
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag = "Open";
                        VideoDataWithEventsLoader.ExoController.ExoTopLayout.SetPadding(3, 5, 3, 0);

                    }

                    HideVideoButtomStyle();
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Hidden && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (VideoDataWithEventsLoader.ExoController.ExoBackButton != null && VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag?.ToString() == "Open")
                    {
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag = "Close";
                        HideVideoButtomStyle();
                        NavigationTabBar.Visibility = ViewStates.Gone;
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Anchored)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Expanded)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Hidden)
                {
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Expanded)
                {
                    if (VideoDataWithEventsLoader.ExoController.ExoBackButton != null && VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag?.ToString() == "Close")
                    {
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag = "Open";
                        NavigationTabBar.Visibility = ViewStates.Gone;
                    }

                    ToggleKeepSceenOnFeature(true);
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Hidden)
                {
                    //Toast.MakeText(this, "p1 Anchored + Anchored ", ToastLength.Short)?.Show();
                    ShowVideoButtomStyle();
                }

                if (p1 == SlidingUpPanelLayout.PanelState.Collapsed && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (VideoDataWithEventsLoader.ExoController.ExoBackButton != null && VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag?.ToString() == "Open")
                    {
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.SetImageResource(Resource.Drawable.icon_close_vector);
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag = "Close";
                        VideoDataWithEventsLoader.ExoController.ExoTopLayout.SetPadding(3, 25, 3, 0);
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }
                }

                if (p1 == SlidingUpPanelLayout.PanelState.Dragging && p2 == SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    if (VideoDataWithEventsLoader.ExoController.ExoBackButton != null && VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag?.ToString() == "Open")
                    {
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.SetImageResource(Resource.Drawable.icon_close_vector);
                        VideoDataWithEventsLoader.ExoController.ExoBackButton.Tag = "Close";
                        VideoDataWithEventsLoader.ExoController.ExoTopLayout.SetPadding(3, 25, 3, 0);
                        ShowVideoButtomStyle();
                        NavigationTabBar.Visibility = ViewStates.Visible;
                    }

                    ToggleKeepSceenOnFeature(false);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion  

        #region Get Notifications

        private async Task GetNotifications()
        {
            if (!UserDetails.IsLogin) return;
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Notifications.GetCountNotificationsAsync();
                if (apiStatus == 200)
                {
                    if (respond is GetNotificationsObject result)
                    {
                        var count = result.Notifications.Count;
                        if (count != 0)
                        {
                            ShowOrHideBadgeViewNotifications(count, true);
                        }
                        else
                        {
                            ShowOrHideBadgeViewNotifications();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respond);
            }
        }

        private QBadgeView Badge; 
        public void ShowOrHideBadgeViewNotifications(int countNotifications = 0, bool show = false)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        if (show)
                        {
                            if (HomeFragment.NotificationButton != null)
                            {
                                int gravity = (int)(GravityFlags.End | GravityFlags.Bottom);
                                Badge = new QBadgeView(this);
                                Badge.BindTarget(HomeFragment.NotificationButton);
                                Badge.SetBadgeNumber(countNotifications);
                                Badge.SetBadgeGravity(gravity);
                                Badge.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                Badge.SetGravityOffset(0, true);
                            }
                            if (TrendingFragment.NotificationButton != null)
                            {
                                int gravity = (int)(GravityFlags.End | GravityFlags.Bottom);
                                Badge = new QBadgeView(this);
                                Badge.BindTarget(TrendingFragment.NotificationButton);
                                Badge.SetBadgeNumber(countNotifications);
                                Badge.SetBadgeGravity(gravity);
                                Badge.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                Badge.SetGravityOffset(0, true);
                            }
                        }
                        else
                        {
                            Badge?.BindTarget(HomeFragment.NotificationButton).Hide(true);
                            Badge?.BindTarget(TrendingFragment.NotificationButton).Hide(true);
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region VideoButtom
         
        public void OnClick(View v)
        {
            try
            {
                if (v.Id == VideoButtomStyle.Id)
                {
                    if (SlidingUpPanel != null && SlidingUpPanel.GetPanelState() != SlidingUpPanelLayout.PanelState.Expanded)
                    {
                        HideVideoButtomStyle();
                        SlidingUpPanel?.SetPanelState(SlidingUpPanelLayout.PanelState.Expanded);
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public bool OnTouch(View v, MotionEvent e)
        {
            try
            {
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        X = e.RawX;
                        Dx = X - v.GetX();
                        return false;
                    case MotionEventActions.Move:
                        VideoTextContainer.Alpha = 1 - Math.Abs(1 / e.RawX * 100);
                        VideoButtomStyle.SetX(e.RawX - Dx);
                        return false;
                    case MotionEventActions.Up:
                        {
                            if (e.RawX - Dx > v.Width / 2)
                            {
                                VideoButtomStyle?.Animate()?.TranslationX(v.Width)?.TranslationY(0)?.Alpha(0)?.SetDuration(300);
                                VideoDataWithEventsLoader?.GlobalVideosRelease("All");

                                ListUtils.LessonList = new ObservableCollection<VideoDataObject>();
                            }
                            else if (Math.Abs(e.RawX - Dx) > v.Width / 2)
                            {
                                VideoButtomStyle?.Animate()?.TranslationX(-v.Width)?.TranslationY(0)?.Alpha(0)?.SetDuration(300);
                                VideoDataWithEventsLoader?.GlobalVideosRelease("All");

                                ListUtils.LessonList = new ObservableCollection<VideoDataObject>();
                            }
                            else if (Math.Abs((int)e.RawX - (int)Dx) < 30) //Click Event 
                            {
                                v.PerformClick();
                                return false;
                            }
                            else
                            {
                                VideoButtomStyle?.Animate()?.TranslationX(0)?.TranslationY(0)?.Alpha(1)?.SetDuration(300);
                            }
                            return true;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
            return false;
        }
         
        private void ShowVideoButtomStyle()
        {
            try
            {
                if (VideoDataWithEventsLoader?.VideoData != null && (VideoDataWithEventsLoader?.VideoData.VideoType == "Vimeo" || VideoDataWithEventsLoader?.VideoData.VideoType == "Twitch" || VideoDataWithEventsLoader?.VideoData.VideoType == "Daily" || VideoDataWithEventsLoader?.VideoData.VideoType == "Ok" || VideoDataWithEventsLoader?.VideoData.VideoType == "Facebook"))
                {
                    VideoDataWithEventsLoader?.GlobalVideosRelease("Other");
                    return;
                }

                if (SlidingUpPanel.Tag?.ToString() == "Hidden")
                {
                    if (VideoButtomStyle.Alpha != 1)
                        VideoButtomStyle.Alpha = 1;

                    if (VideoButtomStyle.Visibility != ViewStates.Visible)
                        VideoButtomStyle.Visibility = ViewStates.Visible;
                    else
                        return;

                    View namebar = VideoDataWithEventsLoader.Root;
                    ViewGroup parent = (ViewGroup)namebar.Parent;
                    if (parent != null)
                    {
                        parent.RemoveView(namebar);
                        VideoSmallFrameLayout.AddView(namebar);
                    }

                    if (VideoDataWithEventsLoader.VideoData.VideoType == "VideoObject/youtube" || VideoDataWithEventsLoader.VideoData.VideoLocation.Contains("Youtube") || VideoDataWithEventsLoader.VideoData.VideoLocation.Contains("youtu"))
                    {
                        if (VideoDataWithEventsLoader?.YoutubePlayer != null)
                        {
                            if (!string.IsNullOrEmpty(VideoDataWithEventsLoader?.VideoIdYoutube) && VideoDataWithEventsLoader.YouTubePlayerEvents.IsPlaying)
                            {
                                //YoutubePlayer.SetPlayerStyle(YouTubePlayerPlayerStyle.Minimal);
                                VideoDataWithEventsLoader?.TubePlayerView.PlayerUiController.ShowUi(false);
                                if (Lifecycle.CurrentState == Lifecycle.State.Resumed)
                                    VideoDataWithEventsLoader?.YoutubePlayer.LoadVideo(VideoDataWithEventsLoader?.VideoIdYoutube, VideoDataWithEventsLoader.YouTubePlayerEvents.CurrentSecond);
                                else
                                    VideoDataWithEventsLoader?.YoutubePlayer.CueVideo(VideoDataWithEventsLoader?.VideoIdYoutube, VideoDataWithEventsLoader.YouTubePlayerEvents.CurrentSecond);
                            }
                        }
                    }
                    else if (VideoDataWithEventsLoader.VideoData.VideoType == "Vimeo" || VideoDataWithEventsLoader.VideoData.VideoType == "Twitch" || VideoDataWithEventsLoader.VideoData.VideoType == "Daily" || VideoDataWithEventsLoader.VideoData.VideoType == "Ok" || VideoDataWithEventsLoader.VideoData.VideoType == "Facebook")
                    {

                    }
                    else
                    {
                        VideoDataWithEventsLoader.PlayerView.HideController();

                        //small size 
                        if (VideoDataWithEventsLoader.RestrictedVideoPlayerFragment != null && VideoDataWithEventsLoader.VideoFrameLayoutFragments.Contains(VideoDataWithEventsLoader.RestrictedVideoPlayerFragment))
                        {
                            VideoDataWithEventsLoader.RestrictedVideoPlayerFragment.RestrictedTextView.SetTextSize(ComplexUnitType.Sp, 10F);
                            VideoDataWithEventsLoader.RestrictedVideoPlayerFragment.RestrictedIcon.SetPadding(10, 10, 10, 10);
                            if (!string.IsNullOrEmpty(VideoDataWithEventsLoader.RestrictedVideoPlayerFragment.PurchaseButton?.Tag?.ToString()))
                                VideoDataWithEventsLoader.RestrictedVideoPlayerFragment.PurchaseButton.Visibility = ViewStates.Gone;
                        }
                    }

                    SlidingUpPanel.Tag = "Shown";
                    IsBottomVideoViewShowing = true;
                    VideoButtomStyle?.Animate()?.TranslationY(0)?.TranslationX(0)?.Alpha(1)?.SetDuration(100);
                    VideoButtomStyle.Alpha = 1;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HideVideoButtomStyle()
        {
            try
            {
                if (SlidingUpPanel.Tag?.ToString() == "Shown")
                {
                    if (VideoButtomStyle.Visibility != ViewStates.Gone)
                        VideoButtomStyle.Visibility = ViewStates.Gone;
                    else
                        return;

                    if (VideoButtomStyle.TranslationY != 100)
                        VideoButtomStyle.Animate()?.TranslationY(100)?.SetDuration(50);

                    View namebar = VideoDataWithEventsLoader.Root;
                    ViewGroup parent = (ViewGroup)namebar.Parent;
                    if (parent != null)
                    {
                        parent.RemoveView(namebar);
                        MainVideoRoot.AddView(namebar);
                    }

                    if (VideoDataWithEventsLoader.VideoData.VideoType == "VideoObject/youtube" || VideoDataWithEventsLoader.VideoData.VideoLocation.Contains("Youtube") || VideoDataWithEventsLoader.VideoData.VideoLocation.Contains("youtu"))
                    {
                        if (VideoDataWithEventsLoader.YoutubePlayer != null)
                        {
                            if (!string.IsNullOrEmpty(VideoDataWithEventsLoader.VideoIdYoutube) && VideoDataWithEventsLoader.YouTubePlayerEvents.IsPlaying)
                            {
                                //YoutubePlayer.SetPlayerStyle(YouTubePlayerPlayerStyle.Default);
                                VideoDataWithEventsLoader.TubePlayerView.PlayerUiController.ShowUi(true);
                                if (Lifecycle.CurrentState == Lifecycle.State.Resumed)
                                    VideoDataWithEventsLoader.YoutubePlayer.LoadVideo(VideoDataWithEventsLoader.VideoIdYoutube, VideoDataWithEventsLoader.YouTubePlayerEvents.CurrentSecond);
                                else
                                    VideoDataWithEventsLoader.YoutubePlayer.CueVideo(VideoDataWithEventsLoader.VideoIdYoutube, VideoDataWithEventsLoader.YouTubePlayerEvents.CurrentSecond);
                            }
                            else
                            {
                                VideoDataWithEventsLoader.YoutubePlayer.Stop();
                            }
                        }
                        else
                        {
                            VideoDataWithEventsLoader.VideoIdYoutube = VideoDataWithEventsLoader.VideoData.VideoLocation.Split(new[] { "v=" }, StringSplitOptions.None).LastOrDefault();

                            VideoDataWithEventsLoader.TubePlayerView = new YouTubePlayerView(this);

                            // The player will automatically release itself when the activity is destroyed.
                            // The player will automatically pause when the activity is paused
                            // If you don't add YouTubePlayerView as a lifecycle observer, you will have to release it manually.
                            Lifecycle.AddObserver(VideoDataWithEventsLoader.TubePlayerView);

                            VideoDataWithEventsLoader.TubePlayerView.PlayerUiController.ShowMenuButton(false);
                            //TubePlayerView.PlayerUiController.Menu.AddItem(new MenuItem("example", Resource.Drawable.icon_settings_vector, (view)->Toast.makeText(this, "item clicked", Toast.LENGTH_SHORT).show()));
                            VideoDataWithEventsLoader.TubePlayerView.Initialize(VideoDataWithEventsLoader);

                            VideoDataWithEventsLoader.PlayerView.Visibility = ViewStates.Gone;
                            VideoDataWithEventsLoader.OnStop();
                        }
                    }
                    else if (VideoDataWithEventsLoader.VideoData.VideoType == "Vimeo" || VideoDataWithEventsLoader.VideoData.VideoType == "Twitch" || VideoDataWithEventsLoader.VideoData.VideoType == "Daily" || VideoDataWithEventsLoader.VideoData.VideoType == "Ok" || VideoDataWithEventsLoader.VideoData.VideoType == "Facebook")
                    {

                    }
                    else
                    {
                        //big size 
                        if (VideoDataWithEventsLoader.RestrictedVideoPlayerFragment != null && VideoDataWithEventsLoader.VideoFrameLayoutFragments.Contains(VideoDataWithEventsLoader.RestrictedVideoPlayerFragment))
                        {
                            VideoDataWithEventsLoader.RestrictedVideoPlayerFragment.RestrictedTextView.SetTextSize(ComplexUnitType.Sp, 14F);
                            VideoDataWithEventsLoader.RestrictedVideoPlayerFragment.RestrictedIcon.SetPadding(0, 0, 0, 0);

                            if (!string.IsNullOrEmpty(VideoDataWithEventsLoader.RestrictedVideoPlayerFragment.PurchaseButton?.Tag?.ToString()))
                                VideoDataWithEventsLoader.RestrictedVideoPlayerFragment.PurchaseButton.Visibility = ViewStates.Visible;
                        }
                    }

                    SlidingUpPanel.Tag = "Hidden";
                    IsBottomVideoViewShowing = false;
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
                            VideoDataWithEventsLoader.ExoController.RestartPlayAfterShrinkScreen();
                            break;
                        }
                    case 2100 when resultCode == Result.Ok:
                    {
                        VideoDataWithEventsLoader?.TubePlayerView?.ExitFullScreen();
                        break;
                    }
                    case 5000 when resultCode == Result.Ok:
                    {
                            var type = data.GetStringExtra("Open") ?? "";
                            switch (type)
                            {
                                case "UserProfile":
                                    {
                                        var userObject = JsonConvert.DeserializeObject<UserDataObject>(data.GetStringExtra("UserObject") ?? "");
                                        if (userObject != null)
                                            ShowUserChannelFragment(userObject, userObject.Id);
                                        break;
                                    }
                                case "VideosByCategory":
                                    {
                                        var categoryId = data.GetStringExtra("CatId") ?? "";
                                        var categoryName = data.GetStringExtra("CatName") ?? "";

                                        Bundle bundle = new Bundle();
                                        bundle.PutString("CatId", categoryId);
                                        bundle.PutString("CatName", categoryName);

                                        var videoViewerFragment = new VideosByCategoryFragment
                                        {
                                            Arguments = bundle
                                        };

                                        FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
                                        break;
                                    }
                                case "EditVideo":
                                    {
                                        var videoObject = JsonConvert.DeserializeObject<VideoDataObject>(data.GetStringExtra("ItemDataVideo") ?? "");
                                        if (videoObject != null)
                                        {
                                            Bundle bundle = new Bundle();
                                            bundle.PutString("ItemDataVideo", JsonConvert.SerializeObject(videoObject));

                                            var editVideoFragment = new EditVideoFragment
                                            {
                                                Arguments = bundle
                                            };

                                            FragmentBottomNavigator.DisplayFragment(editVideoFragment);
                                        }

                                        break;
                                    }
                            }

                            break;
                    }
                    case 3000 when resultCode == Result.Ok:
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Video_Uploaded), ToastLength.Long)?.Show();
                        break;
                    }
                    case 528 when resultCode == Result.Ok:
                    {
                        var item = JsonConvert.DeserializeObject<PlayListVideoObject>(data?.GetStringExtra("ItemPlaylist") ?? "");
                        if (item != null)
                        {
                            if (MyChannelFragment?.PlayListFragment?.MAdapter?.PlayListsList?.Count > 0)
                            {
                                var dataPlayList = MyChannelFragment.PlayListFragment.MAdapter.PlayListsList.FirstOrDefault(q => q.ListId == item.ListId);
                                if (dataPlayList != null)
                                {
                                    dataPlayList = item;
                                    int index = MyChannelFragment.PlayListFragment.MAdapter.PlayListsList.IndexOf(dataPlayList);
                                    MyChannelFragment.PlayListFragment.MAdapter.NotifyItemChanged(index);
                                }
                            }

                            if (LibraryFragment?.MAdapter?.PlayListsList?.Count > 0)
                            {
                                var dataPlayList = LibraryFragment.MAdapter.PlayListsList.FirstOrDefault(q => q.ListId == item.ListId);
                                if (dataPlayList != null)
                                {
                                    dataPlayList = item;
                                    int index = LibraryFragment.MAdapter.PlayListsList.IndexOf(dataPlayList);
                                    LibraryFragment.MAdapter.NotifyItemChanged(index);
                                }
                            }
                        }

                        break;
                    }
                    default:
                    {
                        switch (requestCode)
                        {
                            //If its from Camera or Gallery
                            case 252 when resultCode == Result.Ok:
                                //File fileCover = new File(UserDetails.Cover);
                                //var photoUriCover = FileProvider.GetUriForFile(this, this.PackageName + ".fileprovider", fileCover);
                                //Glide.With(this).Load(photoUriCover).Apply(new RequestOptions()).Into(MyChannelFragment.ImageCoverChannel);

                                //File fileAvatar = new File(UserDetails.Avatar);
                                //var photoUriAvatar = FileProvider.GetUriForFile(this, this.PackageName + ".fileprovider", fileAvatar);
                                //Glide.With(this).Load(photoUriAvatar).Apply(new RequestOptions().CircleCrop()).Into(MyChannelFragment.ImageChannel);

                                MyChannelFragment?.GetDataChannelApi();
                                break; 
                            case 8520 when AppTools.CheckPictureInPictureAllowed(this):
                                UserDetails.PipIsChecked = true;
                                MainSettings.SharedData?.Edit()?.PutBoolean("picture_in_picture_key", UserDetails.PipIsChecked)?.Commit();
                                break;
                        }

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
                    case 110 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                        break;
                    case 110:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        EditVideoFragment.GetInstance()?.GalleryController?.OpenDialogGallery();
                        break;
                    case 108:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 235 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        OpenDialogLive();
                        break;
                    case 235:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break; 
                    case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        VideoDataWithEventsLoader?.DownloadVideo();
                        break;
                    case 100:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 2100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        UserChannelFragment.Instance?.OpenChat();
                        break;
                    case 2100:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                }
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
                var isNotShowingVideo = !VideoDataWithEventsLoader.PlayerView.IsShown;
                if (isNotShowingVideo)
                {
                    return;
                }

                switch (VideoDataWithEventsLoader.VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        if (!IsBottomVideoViewShowing)
                            BackPressed();

                        if (UserDetails.PipIsChecked && Build.VERSION.SdkInt >= BuildVersionCodes.O && PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture))
                        {
                            Rational rational = new Rational(16, 9);
                            PictureInPictureParams.Builder builder = new PictureInPictureParams.Builder();
                            builder.SetAspectRatio(rational);
                            EnterPictureInPictureMode(builder.Build());
                            base.OnUserLeaveHint();
                        }
                        else
                        {
                            if (VideoDataWithEventsLoader?.PlayerView?.Player?.PlaybackState == IPlayer.StateReady)
                                VideoDataWithEventsLoader.PlayerView.Player.PlayWhenReady = false;
                        }
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        if (VideoDataWithEventsLoader.YouTubePlayerEvents != null && VideoDataWithEventsLoader.YoutubePlayer != null && VideoDataWithEventsLoader.YouTubePlayerEvents.IsPlaying)
                            VideoDataWithEventsLoader.YoutubePlayer?.Pause();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region WakeLock System

        private void AddFlagsWakeLock()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WakeLock) == Permission.Granted)
                    {
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        //request Code 110
                        new PermissionsController(this).RequestPermission(110);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ClearFlagsWakeLock()
        {
            try
            {
                Window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public void SetOnWakeLock()
        {
            try
            {
                if (Wl == null)
                {
                    PowerManager pm = (PowerManager)GetSystemService(PowerService);
                    Wl = pm.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                    Wl.Acquire();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Service Chat

        public void SetService(bool run = true)
        {
            try
            { 
                if (run)
                {  
                    ChatJobInfo.ScheduleJob(this); // reschedule the job
                }
                else
                {
                    // Cancel all jobs
                    ChatJobInfo.StopJob(this); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnReceiveResult(string resultData)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<GetChatsObject>(resultData);
                if (result != null)
                {
                    LastChatActivity.GetInstance()?.LoadDataJsonLastChat(result);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Payment & Dialog
         
        public void OpenDialog(UserDataObject userData)
        {
            try
            { 
                //This channel is paid, You must pay to subscribe
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
                dialog.SetMessage(GetText(Resource.String.Lbl_ChannelIsPaid));
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Purchase),async (materialDialog, action) =>
                {
                    try
                    {
                        if (AppTools.CheckWallet())
                        {
                            if (Methods.CheckConnectivity())
                            {
                                var (apiStatus, respond) = await RequestsAsync.Payment.PurchaseAsync("subscribe", userData.Id);
                                if (apiStatus == 200)
                                {
                                    if (respond is MessageObject result)
                                    {
                                        Console.WriteLine(result.Message);

                                        Toast.MakeText(this, GetText(Resource.String.Lbl_PurchasedSuccessfully), ToastLength.Long)?.Show();
                                        UserChannelFragment?.SetSubscribeChannelWithPaid();
                                    }
                                }
                                else Methods.DisplayReportResult(this, respond);
                            }
                            else
                                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                        else
                        {
                            var dialogBuilder = new MaterialAlertDialogBuilder(this);
                            dialogBuilder.SetTitle(GetText(Resource.String.Lbl_Wallet));
                            dialogBuilder.SetMessage(GetText(Resource.String.Lbl_Error_NoWallet));
                            dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_AddWallet),(materialDialog, action) =>
                            {
                                try
                                {
                                    StartActivity(new Intent(this, typeof(WalletActivity)));
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                            
                            dialogBuilder.Show();
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                
                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        #endregion

        #region Load General Data

        private void GetGeneralAppData()
        {
            try
            { 
                var sqlEntity = new SqLiteDatabase();

                var data = ListUtils.DataUserLoginList.FirstOrDefault();
                if (data != null && UserDetails.IsLogin && data.Status != "Active")
                {
                    data.Status = "Active";
                    UserDetails.Status = "Active";
                    sqlEntity.InsertOrUpdateLogin_Credentials(data);
                }
                
                if (UserDetails.IsLogin)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetNotInterestedVideos, ApiRequest.GetAllShortsVideo,  () => ApiRequest.GetChannelData(this, UserDetails.UserId), GetNotifications, () => ApiRequest.PlayListsVideosApi(this) });

                LoadConfigSettings();

                InAppUpdate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.Get_Settings();
                if (settingsData != null)
                    ListUtils.MySettingsList = settingsData;

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });

                if (UserDetails.IsLogin)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.AdsVideosApi(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void InAppUpdate()
        {
            try
            { 
                if (AppSettings.ShowSettingsUpdateManagerApp)
                    UpdateManagerApp.CheckUpdateApp(this, 4711, Intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static int CountRateApp;
         
        public void InAppReview()
        {
            try
            {
                bool inAppReview = MainSettings.InAppReview.GetBoolean(MainSettings.PrefKeyInAppReview, false);
                if (!inAppReview && AppSettings.ShowSettingsRateApp)
                {
                    if (CountRateApp == AppSettings.ShowRateAppCount)
                    {
                        var dialog = new MaterialAlertDialogBuilder(this);
                        dialog.SetTitle(GetText(Resource.String.Lbl_RateOurApp));
                        dialog.SetMessage(GetText(Resource.String.Lbl_RateOurAppContent));
                        dialog.SetPositiveButton(GetText(Resource.String.Lbl_Rate),(materialDialog, action) =>
                        {
                            try
                            {
                                StoreReviewApp store = new StoreReviewApp();
                                store.OpenStoreReviewPage(PackageName);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                        
                        dialog.Show();

                        MainSettings.InAppReview?.Edit()?.PutBoolean(MainSettings.PrefKeyInAppReview, true)?.Commit();
                    }
                    else
                    {
                        CountRateApp++;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region OneSignal
         
        public void GetDataOneSignal()
        {
            try
            { 
                string type = Intent?.GetStringExtra("TypeNotification") ?? "";
                if (!string.IsNullOrEmpty(type))
                {
                    if (type == "DeepLinks")
                    {
                        string videoId = Intent?.GetStringExtra("videoId") ?? "";

                        //Run fast data fetch from the server 
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetVideosInfoAsJson(videoId) }); 
                    }
                    else
                    {
                        string userId = Intent?.GetStringExtra("UserId") ?? "";

                        // var NotificationInfo = JsonConvert.DeserializeObject<OneSignalObject.NotificationInfoObject>(Intent?.GetStringExtra("NotificationInfo") ?? ""); 
                        var video = JsonConvert.DeserializeObject<VideoDataObject>(Intent?.GetStringExtra("VideoData") ?? "");

                        if (video != null && (type.Contains("added") || type.Contains("disliked") || type.Contains("liked") || type.Contains("commented")))
                        {
                            StartPlayVideo(video);
                        }
                        else switch (string.IsNullOrEmpty(userId))
                        {
                            case false when type.Contains("unsubscribed") || type.Contains("subscribed"):
                            case false:
                                ShowUserChannelFragment(new UserDataObject { Id = userId }, userId);
                                break;
                        }
                    } 
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task GetVideosInfoAsJson(string videoId)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Video.GetVideosDetailsAsync(videoId, UserDetails.AndroidId);
                if (apiStatus == 200)
                {
                    if (respond is GetVideosDetailsObject result)
                    {
                        await Task.Delay(1500);
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                if (result.DataResult.Type == "live")
                                {
                                    if (result.DataResult?.LiveTime != null && result.DataResult.LiveTime > 0)
                                    {
                                        Intent intent = new Intent(this, typeof(LiveStreamingActivity));
                                        //Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience
                                        intent.PutExtra(Constants.KeyClientRole, IO.Agora.Rtc2.Constants.ClientRoleAudience);
                                        intent.PutExtra("VideoId", result.DataResult.Id);
                                        intent.PutExtra("StreamName", result.DataResult.StreamName);
                                        intent.PutExtra("PostLiveStream", JsonConvert.SerializeObject(result.DataResult));
                                        StartActivity(intent);
                                    }
                                }
                                else
                                    StartPlayVideo(result.DataResult);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        }); 
                    }
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