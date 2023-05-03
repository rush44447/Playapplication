using Android.App;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Java.Net;
using System;
using System.Collections.Generic;
using PlayTube.Helpers.Utils;
using Uri = Android.Net.Uri;
using Android.OS;
using PlayTube.Activities.Tabbes;
using System.Linq;
using System.Timers;
using Android.Graphics;
using Android.Text;
using AndroidX.AppCompat.App;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.Classes.Video;
using PlayTube.Helpers.Controller;
using PlayTube.Activities.Models;

namespace PlayTube.MediaPlayers.Exo
{
    public class ExoController
    {
        private readonly Activity ActivityContext;

        private IExoPlayer VideoPlayer;
        private readonly StyledPlayerView PlayerView;
        private StyledPlayerControlView ControlView;

        private PreCachingExoPlayerVideo PreCachingExoPlayerVideo;

        private IDataSource.IFactory DataSourceFactory;
        private IDataSource.IFactory HttpDataSourceFactory;
        private PlayerEvents PlayerListener;
         
        public ImageView ExoBackButton, DownloadIcon, ShareIcon, BtnPrev, MenuButton;
        public ImageButton MFullScreenIcon, BtnNext;
        public LinearLayout ExoTopLayout, MFullScreenButton, ExoTopAds, ExoEventButton;
        public FrameLayout BtnBackward, BtnForward;
        public TextView BtnSkipIntro;

        private VideoAdDataObject DataAdsVideo;
        private Timer TimerAds;

        public ExoController(Activity context, StyledPlayerView playerView , string typePage = "normal")
        {
            try
            {
                ActivityContext = context;
                PlayerView = playerView; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void SetPlayer(bool useController = true)
        {
            try
            {
                PreCachingExoPlayerVideo = new PreCachingExoPlayerVideo(ActivityContext);
                DefaultTrackSelector trackSelector = new DefaultTrackSelector(ActivityContext);
                ControlView = PlayerView.FindViewById<StyledPlayerControlView>(Resource.Id.exo_controller);

                VideoPlayer = new IExoPlayer.Builder(ActivityContext)?.SetTrackSelector(trackSelector)?.Build();
                PlayerListener = new PlayerEvents(ActivityContext, ControlView);
                VideoPlayer?.AddListener(PlayerListener);

                PlayerView.UseController = useController;
                PlayerView.Player = VideoPlayer; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetPlayerControl(bool showFullScreen = true, bool isFullScreen = false)
        {
            try
            {
                if (ControlView != null)
                {
                    //Check All Views  
                    ExoTopLayout = ControlView.FindViewById<LinearLayout>(Resource.Id.topLayout);
                    ExoBackButton = ControlView.FindViewById<ImageView>(Resource.Id.BackIcon); 
                    DownloadIcon = ControlView.FindViewById<ImageView>(Resource.Id.Download_icon); 
                    ShareIcon = ControlView.FindViewById<ImageView>(Resource.Id.share_icon);
                    MenuButton = ControlView.FindViewById<ImageView>(Resource.Id.exo_more_icon);

                    ExoTopAds = ControlView.FindViewById<LinearLayout>(Resource.Id.exo_top_ads);
                    ExoEventButton = ControlView.FindViewById<LinearLayout>(Resource.Id.exo_event_buttons);
                    BtnSkipIntro = ControlView.FindViewById<TextView>(Resource.Id.exo_skipIntro);

                    BtnBackward = ControlView.FindViewById<FrameLayout>(Resource.Id.backward);
                    BtnForward = ControlView.FindViewById<FrameLayout>(Resource.Id.forward);

                    BtnPrev = ControlView.FindViewById<ImageView>(Resource.Id.image_prev);
                    BtnNext = ControlView.FindViewById<ImageButton>(Resource.Id.image_next);

                    BtnSkipIntro.Visibility = ViewStates.Gone;
                    ExoTopAds.Visibility = ViewStates.Gone;

                    MFullScreenIcon = ControlView.FindViewById<ImageButton>(Resource.Id.exo_minimal_fullscreen);
                    MFullScreenButton = ControlView.FindViewById<LinearLayout>(Resource.Id.exo_minimal_controls);

                    if (!showFullScreen)
                    { 
                        MFullScreenIcon.Visibility = ViewStates.Gone;
                        MFullScreenButton.Visibility = ViewStates.Gone;
                    }
                    else
                    { 
                        MenuButton.Click +=MenuButtonOnClick;
                        BtnSkipIntro.Click += BtnSkipIntroOnClick;
                        ExoTopAds.Click += ExoTopAdsOnClick;

                        BtnBackward.Click += BtnBackwardOnClick;
                        BtnForward.Click += BtnForwardOnClick;

                        BtnPrev.Click += BtnPrevOnClick;
                        BtnNext.Click += BtnNextOnClick;

                        MFullScreenButton.Click += MFullScreenButtonOnClick;

                        if (isFullScreen)
                        {
                            MFullScreenButton.Tag = "FullScreenOpen";
                            MFullScreenIcon.SetImageResource(Resource.Drawable.ic_action_ic_fullscreen_skrink);
                        }
                        else
                        {
                            MFullScreenButton.Tag = "FullScreenClose";
                            MFullScreenIcon.SetImageResource(Resource.Drawable.ic_action_ic_fullscreen_expand);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
        {
            try
            {
                if (DataSourceFactory == null)
                {
                    DefaultDataSource.Factory upstreamFactory = new DefaultDataSource.Factory(ActivityContext, GetHttpDataSourceFactory());
                    DataSourceFactory = BuildReadOnlyCacheDataSource(upstreamFactory, PreCachingExoPlayerVideo.GetCache());
                }

                IMediaSource src = new ProgressiveMediaSource.Factory(DataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
                if (src?.MediaItem != null)
                    src.MediaItem.MediaId = tag; 

                return src;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        private IDataSource.IFactory GetHttpDataSourceFactory()
        {
            if (HttpDataSourceFactory == null)
            {
                CookieManager cookieManager = new CookieManager();
                cookieManager.SetCookiePolicy(ICookiePolicy.AcceptOriginalServer);
                CookieHandler.Default = cookieManager;
                HttpDataSourceFactory = new DefaultHttpDataSource.Factory();
            }

            return HttpDataSourceFactory;
        }

        private CacheDataSource.Factory BuildReadOnlyCacheDataSource(IDataSource.IFactory upstreamFactory, ICache cache)
        {
            return new CacheDataSource.Factory()?.SetCache(cache)?.SetUpstreamDataSourceFactory(upstreamFactory)?.SetCacheWriteDataSinkFactory(null)?.SetFlags(CacheDataSource.FlagIgnoreCacheOnError);
        }

        public void FirstPlayVideo(Uri uri, VideoDataObject videoData, bool showAds = true)
        {
            try
            {
                bool canPrev = ListUtils.LessonList.Count > 0;
                BtnPrev.Enabled = canPrev;
                BtnPrev.Alpha = canPrev ? 1f : 0.3f;

                bool canNext = ListUtils.ArrayListPlay.Count > 0;
                BtnNext.Enabled = canNext;
                BtnNext.Alpha = canNext ? 1f : 0.3f;

                bool vidMonit = /*ListUtils.MySettingsList?.UsrVMon == "on" &&*/ videoData.Monetization == "1" && videoData.Owner?.OwnerClass?.VideoMon == "1";

                var isPro = ListUtils.MyChannelList?.FirstOrDefault()?.IsPro ?? "0";
                if (!AppSettings.AllowOfflineDownload || AppSettings.AllowDownloadProUser && isPro == "0")
                    DownloadIcon.Visibility = ViewStates.Gone;
                 
                var videoSource = GetMediaSourceFromUrl(uri, "normal");

                if (PlayerSettings.EnableOfflineMode && uri.ToString()!.Contains("http"))
                {
                    PreCachingExoPlayerVideo.CacheVideosFiles(uri);
                    videoSource = new ProgressiveMediaSource.Factory(PreCachingExoPlayerVideo.CacheDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
                    if (videoSource?.MediaItem != null) 
                        videoSource.MediaItem.MediaId = "normal";
                }

                if (showAds)
                    RunVideoWithAds(videoSource, vidMonit);
                else
                {
                    VideoPlayer.SetMediaSource(videoSource);
                    VideoPlayer.Prepare();
                    VideoPlayer.PlayWhenReady = true;
                    VideoPlayer.SeekTo(0);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void FirstPlayVideo(Uri uri, long videoDuration)
        {
            try
            {
                var videoSource = GetMediaSourceFromUrl(uri, "normal");

                if (PlayerSettings.EnableOfflineMode && uri.ToString()!.Contains("http"))
                {
                    PreCachingExoPlayerVideo.CacheVideosFiles(uri);
                    videoSource = new ProgressiveMediaSource.Factory(PreCachingExoPlayerVideo.CacheDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
                }

                VideoPlayer.SetMediaSource(videoSource);
                VideoPlayer.Prepare();
                VideoPlayer.PlayWhenReady = true;
                VideoPlayer.SeekTo(videoDuration);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void RunVideoWithAds(IMediaSource videoSource, bool showAds)
        {
            try
            {
                var isPro = ListUtils.MyChannelList?.FirstOrDefault()?.IsPro ?? "0";
                if (isPro == "0" && ListUtils.AdsVideoList.Count > 0 && Methods.CheckConnectivity() && showAds)
                {
                    Random rand = new Random();

                    var playPos = rand.Next(ListUtils.AdsVideoList.Count - 1 + 1);
                    DataAdsVideo = ListUtils.AdsVideoList[playPos];
                  
                    var urlAds = "";
                    if (!string.IsNullOrEmpty(DataAdsVideo?.Media))
                    {
                        urlAds = DataAdsVideo.Media;
                    }
                    else if (!string.IsNullOrEmpty(DataAdsVideo?.AdMedia))
                    {
                        urlAds = DataAdsVideo.AdMedia;
                    }

                    var type = Methods.AttachmentFiles.Check_FileExtension(urlAds);
                    if (type == "Video" && DataAdsVideo != null)
                    {
                        //AppSettings.ShowButtonSkip = DataAdsVideo
                        var adVideoSource = GetMediaSourceFromUrl(Uri.Parse(urlAds), "Ads");
                        if (adVideoSource != null)
                        {
                            ListUtils.AdsVideoList.Remove(DataAdsVideo);

                            // Plays the first video, then the second video.
                            //var concatenatedSource = new ConcatenatingMediaSource(adVideoSource, videoSource);

                            VideoPlayer.SetMediaSources(new List<IMediaSource>() { adVideoSource, videoSource });

                            ExoTopLayout.Visibility = ViewStates.Gone;
                            ExoEventButton.Visibility = ViewStates.Invisible;
                            BtnSkipIntro.Visibility = ViewStates.Visible;
                            ExoTopAds.Visibility = ViewStates.Visible;

                            if (DataAdsVideo.SkipSeconds?.ToString() != "0")
                            {
                                BtnSkipIntro.Text = DataAdsVideo.SkipSeconds?.ToString();
                                // CountShow = DataAdsVideo.SkipSeconds.Value;
                                CountShow = AppSettings.ShowButtonSkip;
                            }
                            else
                            {
                                BtnSkipIntro.Text = AppSettings.ShowButtonSkip.ToString();
                                CountShow = AppSettings.ShowButtonSkip;
                            }
                            BtnSkipIntro.Enabled = false;

                            RunTimer();
                        }
                        else
                        {
                            VideoPlayer.SetMediaSource(videoSource);

                            ExoTopLayout.Visibility = ViewStates.Visible;
                            ExoEventButton.Visibility = ViewStates.Visible;
                            BtnSkipIntro.Visibility = ViewStates.Gone;
                            ExoTopAds.Visibility = ViewStates.Gone;
                        }
                    }
                    else
                    {
                        VideoPlayer.SetMediaSource(videoSource);
                         
                        ExoTopLayout.Visibility = ViewStates.Visible;
                        ExoEventButton.Visibility = ViewStates.Visible;
                        BtnSkipIntro.Visibility = ViewStates.Gone;
                        ExoTopAds.Visibility = ViewStates.Gone;
                    }
                }
                else
                {
                    VideoPlayer.SetMediaSource(videoSource);
                     
                    ExoTopLayout.Visibility = ViewStates.Visible;
                    ExoEventButton.Visibility = ViewStates.Visible;
                    BtnSkipIntro.Visibility = ViewStates.Gone;
                    ExoTopAds.Visibility = ViewStates.Gone;
                }
                 
                VideoPlayer.Prepare();
                VideoPlayer.PlayWhenReady = true;
                VideoPlayer.SeekTo(0); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RunTimer()
        {
            try
            {
                TimerAds = new Timer { Interval = 1000 };
                TimerAds.Elapsed += TimerAdsOnElapsed;
                TimerAds.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private long CountShow = AppSettings.ShowButtonSkip;
        private void TimerAdsOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                ActivityContext?.RunOnUiThread(() =>
                {
                    try
                    {
                        switch (CountShow)
                        {
                            case 0:
                                SetTextSkipIntro();

                                BtnSkipIntro.Enabled = true;

                                if (TimerAds != null)
                                {
                                    TimerAds.Enabled = false;
                                    TimerAds.Stop();
                                }

                                TimerAds = null!;
                                break;
                            case > 0:
                                CountShow--;
                                BtnSkipIntro.Text = CountShow.ToString();
                                break;
                            default:
                                SetTextSkipIntro();
                                BtnSkipIntro.Enabled = true;

                                if (TimerAds != null)
                                {
                                    TimerAds.Enabled = false;
                                    TimerAds.Stop();
                                }

                                TimerAds = null!;
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void SetTextSkipIntro()
        {
            try
            {
                Typeface font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");

                BtnSkipIntro.Gravity = GravityFlags.CenterHorizontal;
                BtnSkipIntro.SetTypeface(font, TypefaceStyle.Normal);
                var woTextDecorator = new TextDecorator
                {
                    Content = ActivityContext.GetText(Resource.String.Lbl_SkipAds) + " " + IonIconsFonts.IosArrowForward,
                    DecoratedContent = new SpannableString(ActivityContext.GetText(Resource.String.Lbl_SkipAds) + " " + IonIconsFonts.IosArrowForward)
                };
                woTextDecorator.SetTextColor(IonIconsFonts.ArrowForward, "#ffffff");
                woTextDecorator.Build(BtnSkipIntro, woTextDecorator.DecoratedContent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void PlayVideo()
        {
            try
            {
                if (PlayerView?.Player != null && PlayerView.Player.PlaybackState == IPlayer.StateReady && !PlayerView.Player.PlayWhenReady)
                    PlayerView.Player.PlayWhenReady = true;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopVideo()
        {
            try
            {
                if (PlayerView?.Player != null && PlayerView.Player.PlayWhenReady)
                    PlayerView.Player.PlayWhenReady = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleaseVideo()
        {
            try
            {
                StopVideo();
                PlayerView?.Player?.Stop();

                if (VideoPlayer != null)
                {
                    VideoPlayer.Release();
                    VideoPlayer = null;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public StyledPlayerControlView GetControlView()
        {
            return ControlView;
        }

        public StyledPlayerView GetPlayerView()
        {
            return PlayerView;
        }

        public IExoPlayer GetExoPlayer()
        {
            return VideoPlayer;
        }

        private void MFullScreenButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                //if (MFullScreenButton?.Tag?.ToString() == "FullScreenClose")
                //{
                //    Intent intent = new Intent(ActivityContext, typeof(VideoFullScreenActivity));
                //    intent.PutExtra("videoDuration", VideoPlayer.Duration);
                //    ActivityContext.StartActivityForResult(intent, 2000);
                //}
                //else if (MFullScreenButton?.Tag?.ToString() == "FullScreenOpen")
                //{
                //    Intent intent = new Intent();
                //    VideoFullScreenActivity.Instance?.SetResult(Result.Ok, intent);
                //    VideoFullScreenActivity.Instance?.Finish();
                //}
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void RestartPlayAfterShrinkScreen(IExoPlayer player)
        {
            try
            {
                PlayerView.Player = null;
                PlayerView.Player = player;
                PlayerView.Player.PlayWhenReady = true;
                MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_expand));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ChangePlaybackSpeed(PlaybackParameters playbackParameters)
        {
            try
            {
                if (PlayerView.Player != null)
                {
                    PlayerView.Player.PlaybackParameters = playbackParameters;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Event Control View


        private void BtnNextOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ListUtils.ArrayListPlay.Count > 0)
                {
                    var data = ListUtils.ArrayListPlay.FirstOrDefault();
                    if (data != null)
                    {
                        ListUtils.LessonList.Add(data);
                        TabbedMainActivity.GetInstance()?.StartPlayVideo(data);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnPrevOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ListUtils.LessonList.Count > 0)
                {
                    var data = ListUtils.LessonList.LastOrDefault();
                    if (data != null)
                    {
                        TabbedMainActivity.GetInstance()?.StartPlayVideo(data);
                        ListUtils.LessonList.Remove(data);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnForwardOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ForwardPressed)
                {
                    PressedHandler.RemoveCallbacks(() => { ForwardPressed = false; });
                    ForwardPressed = false;

                    //Add event
                    var fTime = 10000; // 10 Sec
                    if (PlayerView?.Player != null)
                    {
                        var eTime = PlayerView.Player.Duration;
                        var sTime = PlayerView.Player.CurrentPosition;
                        if ((sTime + fTime) <= eTime)
                        {
                            sTime += fTime;
                            PlayerView.Player.SeekTo(sTime);

                            if (!PlayerView.Player.PlayWhenReady)
                                PlayerView.Player.PlayWhenReady = true;
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_ErrorForward), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    ForwardPressed = true;
                    PressedHandler.PostDelayed(() => { ForwardPressed = false; }, 2000L);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        private bool BackwardPressed, ForwardPressed;
        private readonly Handler PressedHandler = new Handler(Looper.MainLooper);
        private void BtnBackwardOnClick(object sender, EventArgs e)
        {
            try
            {
                if (BackwardPressed)
                {
                    PressedHandler.RemoveCallbacks(() => { BackwardPressed = false; });
                    BackwardPressed = false;

                    //Add event
                    var bTime = 10000; // 10 Sec
                    if (PlayerView.Player != null)
                    {
                        var sTime = PlayerView.Player.CurrentPosition;

                        if ((sTime - bTime) > 0)
                        {
                            sTime -= bTime;
                            PlayerView.Player.SeekTo(sTime);

                            if (!PlayerView.Player.PlayWhenReady)
                                PlayerView.Player.PlayWhenReady = true;
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_ErrorBackward), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    BackwardPressed = true;
                    PressedHandler.PostDelayed(() => { BackwardPressed = false; }, 2000L);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ExoTopAdsOnClick(object sender, EventArgs e)
        {
            try
            {
                if (DataAdsVideo != null)
                {
                    string url = DataAdsVideo.Url;
                    new IntentController(ActivityContext).OpenBrowserFromApp(url);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MenuButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var activity = (AppCompatActivity)ActivityContext;
                var dialogFragment = new MoreMenuVideoDialogFragment();
                dialogFragment.Show(activity.SupportFragmentManager, dialogFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnSkipIntroOnClick(object sender, EventArgs e)
        {
            try
            {
                if (PlayerView?.Player != null)
                {
                    PlayerView.Player.Next();

                    ExoTopLayout.Visibility = ViewStates.Visible;
                    ExoEventButton.Visibility = ViewStates.Visible;
                    BtnSkipIntro.Visibility = ViewStates.Gone;
                    ExoTopAds.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         

        #endregion
         
        public void HideControls(bool isInPictureInPictureMode)
        {
            try
            {
                ExoTopLayout.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
                ExoBackButton.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
                DownloadIcon.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
                MFullScreenIcon.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
                MFullScreenButton.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
                ShareIcon.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
                MenuButton.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void ToggleExoPlayerKeepScreenOnFeature(bool keepScreenOn)
        {
            try
            {
                if (PlayerView != null)
                {
                    PlayerView.KeepScreenOn = keepScreenOn;
                }

                //if (FullScreenPlayerView != null)
                //{
                //    FullScreenPlayerView.KeepScreenOn = keepScreenOn;
                //}
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //wael
        public void RestartPlayAfterShrinkScreen()
        {
            try
            {
                PlayerView.Player = null;
                if (PlayerView != null)
                {
                    //Player?.AddListener(PlayerListener);
                    //Player?.AddVideoListener(this);
                    //PlayerView.Player = FullScreenPlayerView.Player;
                    //PlayerView.Player.PlayWhenReady = true;
                    //PlayerView.RequestFocus();
                    //PlayerView.Visibility = ViewStates.Visible;
                    //MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.pif_video_player_menu_full_screen));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}