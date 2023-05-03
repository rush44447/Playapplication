using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.TextField;
using PlayTube.Activities.Base;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTubeClient.RestCalls;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using PlayTube.Helpers.CacheLoaders;
using Google.Android.Material.Dialog;
using AndroidHUD;
using Com.Canhub.Cropper;
using Java.Util;
using PlayTube.SQLite;
using PlayTubeClient.Classes.Global;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using AndroidX.Activity.Result;

namespace PlayTube.Activities.Channel
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditMyChannelActivity : BaseActivity, IDialogListCallBack, IActivityResultCallback
    {
        #region Variables Basic

        private ImageView ImageCover, ImageAvatar;
        private LinearLayout ChangeCoverLayout;
        private RelativeLayout ChangeAvatarLayout;

        private TextInputEditText TxtUsername, TxtFullName, TxtEmail, TxtAbout, TxtFavCategory, TxtGender, TxtAge, TxtCountry, TxtFacebook, TxtTwitter;
        private AppCompatButton SaveButton;

        private PublisherAdView PublisherAdView;
        private string ImageType, GenderStatus, Age, CountryId;
        private string CategoryId, CategoryName, DialogType;
        private List<string> CategorySelect = new List<string>();
        private DialogGalleryController GalleryController;

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
                SetContentView(Resource.Layout.EditMyChannelLayout);
                 
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                Get_Data_User();
                GalleryController = new DialogGalleryController(this, this);
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
                AddOrRemoveEvent(true);
                PublisherAdView?.Resume();
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
                AddOrRemoveEvent(false);
                PublisherAdView?.Pause();
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

        protected override void OnDestroy()
        {
            try
            {
                PublisherAdView?.Destroy();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ImageCover = FindViewById<ImageView>(Resource.Id.imageCover);
                ChangeCoverLayout = FindViewById<LinearLayout>(Resource.Id.ChangeCoverLayout);
                 
                ImageAvatar = FindViewById<ImageView>(Resource.Id.imageAvatar);
                ChangeAvatarLayout = FindViewById<RelativeLayout>(Resource.Id.ChangeAvatarLayout);

                TxtUsername = FindViewById<TextInputEditText>(Resource.Id.usernameEdit);
                TxtFullName = FindViewById<TextInputEditText>(Resource.Id.FullNameEdit);
                TxtEmail = FindViewById<TextInputEditText>(Resource.Id.emailEdit);
                TxtAbout = FindViewById<TextInputEditText>(Resource.Id.aboutEdit);
                TxtFavCategory = FindViewById<TextInputEditText>(Resource.Id.favCategoryEdit);
                TxtGender = FindViewById<TextInputEditText>(Resource.Id.genderEdit);
                TxtAge = FindViewById<TextInputEditText>(Resource.Id.ageEdit);
                TxtCountry = FindViewById<TextInputEditText>(Resource.Id.countryEdit);
                TxtFacebook = FindViewById<TextInputEditText>(Resource.Id.facebookEdit);
                TxtTwitter = FindViewById<TextInputEditText>(Resource.Id.twitterEdit);
           
                SaveButton = FindViewById<AppCompatButton>(Resource.Id.SaveButton);

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);

                Methods.SetFocusable(TxtFavCategory);
                Methods.SetFocusable(TxtGender);
                Methods.SetFocusable(TxtAge);
                Methods.SetFocusable(TxtCountry);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = " ";
                    toolbar.SetTitleTextColor(AppTools.IsTabDark() ? Color.White : Color.Black);

                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(AppTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    ChangeCoverLayout.Click += ChangeCoverLayoutOnClick;
                    ChangeAvatarLayout.Click += ChangeAvatarLayoutOnClick;
                    TxtFavCategory.Touch += TxtFavCategoryOnTouch;
                    TxtGender.Touch += TxtGenderOnTouch;
                    TxtAge.Touch += TxtAgeOnTouch;
                    TxtCountry.Touch += TxtCountryOnTouch;
                    SaveButton.Click += SaveButtonOnClick;
                }
                else
                {
                    ChangeCoverLayout.Click -= ChangeCoverLayoutOnClick;
                    ChangeAvatarLayout.Click -= ChangeAvatarLayoutOnClick;
                    TxtFavCategory.Touch -= TxtFavCategoryOnTouch;
                    TxtGender.Touch -= TxtGenderOnTouch;
                    TxtAge.Touch -= TxtAgeOnTouch;
                    TxtCountry.Touch -= TxtCountryOnTouch;
                    SaveButton.Click -= SaveButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
         
        private void ChangeAvatarLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                ImageType = "Avatar";
                GalleryController?.OpenDialogGallery("Avatar");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ChangeCoverLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                ImageType = "Cover";
                GalleryController?.OpenDialogGallery("Cover");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtFavCategoryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Up) return;

                DialogType = "FavCategory";

                //var dialogList = new MaterialAlertDialogBuilder(this);

                //var arrayAdapter = CategoriesController.ListCategories.Select(item => Methods.FunString.DecodeString(item.Name)).ToList();
                //var arrayIndexAdapter = new List<int>();
                //if (CategorySelect?.Count > 0)
                //{
                //    arrayIndexAdapter.AddRange(CategorySelect.Select(t => CategoriesController.ListCategories.IndexOf(CategoriesController.ListCategories.FirstOrDefault(c => c.Id == t))));
                //}
                //else
                //{
                //    var local = ListUtils.MyChannelList?.FirstOrDefault();
                //    if (local?.FavCategory?.Count > 0)
                //    {
                //        arrayIndexAdapter.AddRange(local?.FavCategory.Select(t => CategoriesController.ListCategories.IndexOf(CategoriesController.ListCategories.FirstOrDefault(c => c.Id == t))));
                //    }
                //}

                //dialogList.SetTitle(GetText(Resource.String.Lbl_ChooseFavCategory))
                //    .SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this))
                //    .ItemsCallbackMultiChoice(arrayIndexAdapter.ToArray(), this)
                //    .AlwaysCallMultiChoiceCallback()
                //    .SetPositiveButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils())
                //    .Show();

                //wael test
                var countriesArray = CategoriesController.ListCategories.Select(item => Methods.FunString.DecodeString(item.Name)).ToList();
                
                var checkedItems = new bool[countriesArray.Count];
                var selectedItems = new List<string>(countriesArray);

                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(Resource.String.Lbl_ChooseFavCategory);
                dialogList.SetCancelable(false);
                dialogList.SetMultiChoiceItems(countriesArray.ToArray(), checkedItems, (o, args) =>
                {
                    try
                    {
                        checkedItems[args.Which] = args.IsChecked;

                        var text = selectedItems[args.Which] ?? "";
                        Console.WriteLine(text);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Close), (o, args) =>
                {
                    try
                    {
                        CategoryId = "";
                        CategoryName = "";
                        CategorySelect = new List<string>();

                        for (int i = 0; i < checkedItems.Length; i++)
                        {
                            if (checkedItems[i])
                            {
                                var text = selectedItems[i];

                                CategoryId += CategoriesController.ListCategories[i].Id + ",";
                                CategoryName += CategoriesController.ListCategories[i].Name + ",";

                                CategorySelect.Add(CategoryId); 
                            }
                        }

                        TxtFavCategory.Text = CategoryName.Remove(CategoryName.Length - 1, 1);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetNeutralButton(Resource.String.Lbl_SelectAll, (o, args) =>
                {
                    try
                    {
                        Arrays.Fill(checkedItems, true);

                        CategoryId = "";
                        CategoryName = "";
                        CategorySelect = new List<string>();

                        foreach (var item in CategoriesController.ListCategories)
                        {
                            CategoryId += item.Id + ",";
                            CategoryName += item.Name + ",";

                            CategorySelect.Add(CategoryId);
                        }

                        TxtFavCategory.Text = CategoryName.Remove(CategoryName.Length - 1, 1);
                    }
                    catch (Exception ex)
                    {
                        Methods.DisplayReportResultTrack(ex);
                    }
                });

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void TxtCountryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Up) return;

                DialogType = "Country";
                 
                var dialogList = new MaterialAlertDialogBuilder(this);

                var countriesArray = AppTools.GetCountryList(this);
                var arrayAdapter = countriesArray.Select(item => item.Value).ToList();

                dialogList.SetTitle(GetText(Resource.String.Lbl_Country));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtAgeOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Up) return;

                DialogType = "Age";
                 
                var arrayAdapter = Enumerable.Range(1, 99).Select(i => i.ToString()).ToList();
                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(GetText(Resource.String.Lbl_Age));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtGenderOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Up) return;

                DialogType = "Gender";
                List<string> arrayAdapter = new List<string>();
                MaterialAlertDialogBuilder dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Radio_Male));
                arrayAdapter.Add(GetText(Resource.String.Radio_Female));

                dialogList.SetTitle(GetText(Resource.String.Lbl_Gender));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private async void SaveButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                  
                    string first = "", last = ""; 
                    var name = TxtFullName.Text?.Split(' ');
                    if (name?.Length > 0)
                    {
                        first = name.FirstOrDefault();
                        last = name.LastOrDefault();
                    }

                    var dictionary = new Dictionary<string, string>
                    {
                        {"settings_type", "general"},
                        {"username", TxtUsername.Text},
                        {"email", TxtEmail.Text},
                        {"first_name", first},
                        {"last_name", last},
                        {"about", TxtAbout.Text},
                        {"facebook", TxtFacebook.Text},
                        {"twitter", TxtTwitter.Text},
                        //{"google", TxtGoogle.Text},
                        {"gender", GenderStatus},
                        {"age", Age},
                        {"fav_category", CategoryId},
                        {"country", CountryId}
                    };

                    var (apiResult, respond) = await RequestsAsync.Global.UpdateUserDataGeneralAsync(dictionary);
                    if (apiResult == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            var local = ListUtils.MyChannelList?.FirstOrDefault();
                            if (local != null)
                            {
                                local.Username = UserDetails.Username = TxtUsername.Text;
                                local.Email = UserDetails.Email = TxtEmail.Text;
                                local.FirstName = first;
                                local.LastName = last;
                                local.About = TxtAbout.Text;
                                local.Gender = GenderStatus;
                                local.Facebook = TxtFacebook.Text;
                                local.Twitter = TxtTwitter.Text;
                                //local.Google = TxtGoogle.Text;
                                local.FavCategory = CategorySelect;
                                local.Age = Age;
                                local.CountryId = CountryId;
                                local.CountryName = TxtCountry.Text;

                                var database = new SqLiteDatabase();
                                database.InsertOrUpdate_DataMyChannel(local);
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss(this);

                            Intent intent = new Intent();
                            SetResult(Result.Ok, intent);
                            Finish();
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Permissions && Result
         
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108) //Image Picker
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //Open Image 
                        GalleryController?.OpenDialogGallery(ImageType);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        
        #endregion
         
        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (DialogType)
                {
                    case "Gender":
                        TxtGender.Text = itemString;
                        GenderStatus = position == 0 ? "male" : "female";
                        break;
                    case "Age":
                        TxtAge.Text = itemString;
                        Age = itemString;
                        break;
                    case "Country":
                        var countriesArray = AppTools.GetCountryList(this);
                        var check = countriesArray.FirstOrDefault(a => a.Value == itemString).Key;
                        if (check != null)
                        {
                            CountryId = check;
                        }

                        TxtCountry.Text = itemString;
                        break;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Result Gallery

        public void OnActivityResult(Java.Lang.Object p0)
        {
            try
            {
                if (p0 is CropImageView.CropResult result)
                {
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.UriContent;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, resultUri);
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri
                            if (ImageType == "Avatar")
                            {
                                Glide.With(this).Load(filepath).Apply(GlideImageLoader.GetOptions(ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser)).Into(ImageAvatar);
                            }
                            else if (ImageType == "Cover")
                            {
                                Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(ImageCover);
                            }

                            //Send image function
                            if (Methods.CheckConnectivity())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataImageAsync(resultUri.Path, ImageType.ToLower()) });
                            else
                                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        private async void Get_Data_User()
        {
            try
            {
                if (ListUtils.MyChannelList?.Count == 0)
                    await ApiRequest.GetChannelData(this, UserDetails.UserId);

                var local = ListUtils.MyChannelList?.FirstOrDefault();
                if (local != null)
                {
                    GlideImageLoader.LoadImage(this, local.Avatar, ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                    GlideImageLoader.LoadImage(this, local.Cover, ImageCover, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                     
                    if (local.Gender == "male" || local.Gender == "Male")
                    {
                        GenderStatus = "male";
                        TxtGender.Text = GetText(Resource.String.Radio_Male);
                    }
                    else
                    {

                        GenderStatus = "female";
                        TxtGender.Text = GetText(Resource.String.Radio_Female);
                    }

                    TxtUsername.Text = local.Username;
                    TxtEmail.Text = local.Email;
                    TxtFullName.Text = local.FirstName + " " + local.LastName;
                    TxtAbout.Text = local.About;
                    TxtFacebook.Text = local.Facebook;
                    TxtTwitter.Text = local.Twitter;

                    TxtAge.Text = local.Age == "0" ? GetText(Resource.String.Lbl_Age) : local.Age;
                    Age = local.Age;

                    TxtCountry.Text = local.CountryName;
                    CountryId = local.CountryId;

                    if (local?.FavCategory?.Count > 0)
                    {
                        CategorySelect = local.FavCategory;
                        foreach (var t in local.FavCategory)
                        {
                            CategoryId += t + ",";
                            CategoryName += CategoriesController.ListCategories.FirstOrDefault(q => q.Id == t)?.Name + ",";
                        }

                        TxtFavCategory.Text = CategoryName.Remove(CategoryName.Length - 1, 1);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}