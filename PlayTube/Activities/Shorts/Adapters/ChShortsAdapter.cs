﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Java.Util;
using PlayTube.Activities.Models;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.RestCalls;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Shorts.Adapters
{
    public class ChShortsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, IVideoMenuListener
    {
        public event EventHandler<ChShortsAdapterClickEventArgs> ItemClick;
        public event EventHandler<ChShortsAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<VideoDataObject> VideoList = new ObservableCollection<VideoDataObject>();
        private readonly LibrarySynchronizer LibrarySynchronizer;

        private readonly TabbedMainActivity GlobalContext;
        private readonly RequestOptions Options;
        public ChShortsAdapter(Activity context)
        {
            HasStableIds = true;
            ActivityContext = context;
            LibrarySynchronizer = new LibrarySynchronizer(context);

            GlobalContext = TabbedMainActivity.GetInstance();
            Options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
                .Transform(new CenterCrop(), new RoundedCorners(7))
                .SetPriority(Priority.High)
                .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All).AutoClone()
                .Error(Resource.Drawable.ImagePlacholder)
                .Placeholder(Resource.Drawable.ImagePlacholder));
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Video_Big_View
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_ChShortsView, parent, false);
                var vh = new ChShortsAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is ChShortsAdapterViewHolder holder)
                {
                    var item = VideoList[position];
                    if (item != null)
                    { 
                        GlideImageLoader.LoadImage(ActivityContext, item.Thumbnail, holder.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable, false, Options);

                        holder.TxtDuration.Text = Methods.Time.SplitStringDuration(item.Duration);
                        holder.TxtTitle.Text = Methods.FunString.DecodeString(item.Title);

                        if (!string.IsNullOrEmpty(item.PausedTime) && !UserDetails.IsPauseWatchHistory)
                        {
                            int time = Convert.ToInt32(item.PausedTime);
                            if (time > 0)
                            {
                                holder.Progress.Visibility = ViewStates.Visible;
                                holder.Progress.SetProgress(time, false);
                            }
                            else
                            {
                                holder.Progress.Visibility = ViewStates.Gone;
                            }
                        }
                        else
                        {
                            holder.Progress.Visibility = ViewStates.Gone;
                        }

                        holder.TxtChannelName.Text = AppTools.GetNameFinal(item.Owner?.OwnerClass);

                        var view = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.Views)) + " " + ActivityContext.GetText(Resource.String.Lbl_Views);

                        holder.TxtViewsCount.Text = " | " + view + " | " + item.TimeAgo;

                        holder.TxtChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                        if (!holder.MenuView.HasOnClickListeners)
                        {
                            holder.MenuView.Click += (sender, args) =>
                            {
                                var data = GetItem(holder.BindingAdapterPosition);
                                VideoMenuBottomSheets videoMenuBottomSheets = new VideoMenuBottomSheets(data,this); 
                                videoMenuBottomSheets.Show(GlobalContext.SupportFragmentManager, videoMenuBottomSheets.Tag);
                            };

                            holder.InfoContainer.Click += (sender, args) =>
                            {
                                var data = GetItem(holder.BindingAdapterPosition);
                                GlobalContext?.ShowUserChannelFragment(data.Owner?.OwnerClass, data.Owner?.OwnerClass.Id);
                            };
                        }

                        //Set Badge on videos
                        //AppTools.ShowGlobalBadgeSystem(holder.VideoType, item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void RemoveVideo(VideoDataObject data)
        {
            try
            {
                var check = VideoList.FirstOrDefault(a => a.VideoId == data.VideoId);
                if (check != null)
                {
                    var index = VideoList.IndexOf(check);
                    if (index != -1)
                    {
                        VideoList.Remove(check);
                        NotifyItemRemoved(index);
                        NotifyItemRangeChanged(index, ItemCount);

                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_Removed), ToastLength.Short)?.Show();

                        var dataObject = ListUtils.GlobalNotInterestedList.FirstOrDefault(a => a.Id == data.Id);
                        if (dataObject == null)
                        {
                            ListUtils.GlobalNotInterestedList.Add(data);
                        }

                    }
                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddDeleteNotInterestedAsync(data.Id, true) });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => VideoList?.Count ?? 0;

        public VideoDataObject GetItem(int position)
        {
            return VideoList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        void OnClick(ChShortsAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(ChShortsAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                if (holder is ChShortsAdapterViewHolder viewHolder)
                {
                    Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.ChannelImage);
                    Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.VideoImage);
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = VideoList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Thumbnail != "")
                {
                    d.Add(item.Thumbnail);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString())
                .Apply(new RequestOptions().CenterCrop());
        }
    }

    public class ChShortsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView VideoImage { get; private set; }
        public TextView TxtDuration { get; private set; }
        public ImageView ChannelImage { get; private set; }
        public TextView TxtTitle { get; private set; }
        public ProgressBar Progress { get; private set; }
        public TextView TxtChannelName { get; private set; }
        public TextView TxtViewsCount { get; private set; }
        public ImageView MenuView { get; private set; }
        public TextView VideoType { get; private set; }
        public LinearLayout InfoContainer { get; private set; }

        #endregion

        public ChShortsAdapterViewHolder(View itemView, Action<ChShortsAdapterClickEventArgs> clickListener, Action<ChShortsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                VideoImage = MainView.FindViewById<ImageView>(Resource.Id.Imagevideo);
                VideoType = MainView.FindViewById<TextView>(Resource.Id.videoType);
                TxtDuration = MainView.FindViewById<TextView>(Resource.Id.duration);

                Progress = MainView.FindViewById<ProgressBar>(Resource.Id.Progress);

                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.Title);
                MenuView = MainView.FindViewById<ImageView>(Resource.Id.videoMenu);

                InfoContainer = MainView.FindViewById<LinearLayout>(Resource.Id.info_container);
                ChannelImage = MainView.FindViewById<ImageView>(Resource.Id.Image_Channel);
                TxtChannelName = MainView.FindViewById<TextView>(Resource.Id.ChannelName);
                TxtViewsCount = MainView.FindViewById<TextView>(Resource.Id.Views_Count);

                //FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, MenuView, FontAwesomeIcon.EllipsisV);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new ChShortsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, VideoStyle = ChShortsAdapterClickEventArgs.VideoType.BigVideo });
                itemView.LongClick += (sender, e) => longClickListener(new ChShortsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class ChShortsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public VideoType VideoStyle { get; set; }

        public enum VideoType
        {
            TopVideo, BigVideo, LatestVideo, FavVideo, LiveVideo, StockVideo
        }
    }
}