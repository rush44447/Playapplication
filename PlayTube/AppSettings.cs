//###############################################################
// Author >> Elin Doughouz
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System.Collections.Generic;
using PlayTube.Helpers.Models;

namespace PlayTube
{
    internal static class AppSettings
    {
        /// <summary>
        /// Deep Links To App Content
        /// you should add your website without http in the analytic.xml file >> ../values/analytic.xml .. line 5
        /// <string name="ApplicationUrlWeb">demo.playtubescript.com</string>
        /// </summary>
        public static readonly string TripleDesAppServiceProvider = "YE08GNSCCxCw2p+52wTT1X6m540SM++mA44K+TfRrM4+ypixOQg176vdf5C6HLOyE4kQP36cI6dA+Axy3U/u7vZQ2yzcGlInIqjEdXhgeclUyn3cN1GkJGUyc9wCWCYnDxfLojxnjvgiWYYydZLUI/4Dl+kWZf3v1pmc+bZFMFi+YyttcCIojbS2gfR2mcARcay6q6opT/3pkJ95xZjQEVYxdHKitzYlsqPX/7q9Zi2D2AaMfjYg81NHvqFgJHykI/f/XSmqwUvSZwJpbum739zqActQ79+/DYrMnHa971ZkmWSM/PkkMCO7Z5CdizBx1EdPddJQGdL21ksYhF1bTmuWCccet3cXN4vliXTbCkVYgd1pIrz8IDHHFXI9y9qkEBdwcMnCZ9utn5z8Zttvrb1ln6IimzViXySxiYo74tibifVtR52wvADlOAnrSSKjS6wuZoQ1SgZ5ydDRxjbrwmJUmY643IxLRPXbDjLjm1yUzpgYThWwrPAS8LYUQYczDchGCg0/hrqlYBz7bsahylsrYwOehiCtRk+Ltph4Y6xo3WMq/hT7LF+XpH044OL7TRvqv3MT/qC2tszIPk37Tx47Xzsl49mQeRpxwFlpNahcyXw3IGyo639vZkIhPYqpPzFYMdU95CygozQDFmG92CoGcHYDQxPx9Pa1tTO6iKona+iENHEILTthwsMMY3jI2cqEfavXU10UxNwCr9/mqBOyukHM3BuX70OK1rviNIbS43C4slSYu2HXakAzqQgnjnkFzDi9y51FsFF9L0n/D0YEWSQHXWoO8cXFyRf0tQgRPNQFpQX6B1PYdusrJ0+kDj2gvAnJPMd8+wpjG4YUFUDNeNOfwx2WX+KTC3FVD2X5DfCCePtPU/D7wEbqtATAALY7i7gc6wTYuHllcX0oH3HxLZUmoE3PobV29lTa077u7Cuxjz54oM38XvCnZA876qP85CN8yACWmK9tOy7zyduMLXxkC9Q4O6cDeoCokeChgdWwKJuA8zUbo/i9vWJPtWaofRlUw+Nrx0R7i+4IhekaLjrBrryc6Y9X4YFYRcfXxEYqQHxV6wpC4BhDx5tIU5z8vNL8De8Ud43RVqR4hkVOQ+iRS3YFgC3FSWPJQvLi9I98noQXHAIbLGj6GEAthL7dYxok2nvmgGBEJBJ3B6/C3X9nlj8dDC87TKf+zTPnS8JMer62rYBvlmC7hdHPp/AcCgV7OISIm2JXqqNvVnHv7Pbzfenl6BMDG8nmTt0D6sBJkkeM2byBppJDMD/01wLSGmaqyq9fbmQIjJ8Q/d4eC4wJr8RLejR9r5XD0bBJf+Gph2fypUyamXrkqp6QAJOy9gsNz1TZQbj2f30yGuv1Z8l/bNQkIqfFRT6YRLoEvHnJCrIJSnRALR6kFhnIP7TByrBQRT09KUODsySB6KVs5vWR/O8UnEknHe5o2apb9NYqwiy79McpL3a0Pqlau411ar8vXJGffhmWkyTJoZtxkrFjX9DPsPuSSS7IvuZjFkXvDZmG1NhIgNVE+UbGd6BThFkqRbHx1vo5uMZjHM8vHkc7KzTp5s3P3PStWNdQn+McCoDuRASGUSmELQcZCExCOefHONe/ounntivyPE3YgCh/lT2NOTHpKYQ/4qyYjuXh3LdIWC5m9RMnYUjgA1Q/k2XSGKRiS8zSQBfC2s3quDU45e6cYzCE3ISvdmLqCW9bmzdlfeCRq/jVxW/qdfoUhehdfLGQ26RNjl3tOSPKSGcITc8fopoN7+JHrH7/gSxjouGfE+jWE5Q+ehDI3RtaZZ9DbO/wv3ihRw9+542dQb2Zk+NYHofz++mt3O3FemOQbZ2f0qItffb+AB1IX2wb4gFDSPDIcQiHx18doB4I037IgDdArXs4IdzJ1FbD14/oCeNzpgiPIhrJ7oqiK/wEba6erH4M+OfwwluWmg==";
       
        //Main Settings >>>>> 
        //********************************************************* 
        public static readonly string ApplicationName = "DorcFlex";
        public static readonly string DatabaseName = "DorcFlex"; 
        public static string Version = "3";

        //Main Colors >>
        //*********************************************************
        public static readonly string MainColor = "#0F64F7";

        public static readonly bool LinkWithTv = true; 

        public static readonly PlayerTheme PlayerTheme = PlayerTheme.Theme2; 
         
        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar_AE

        //Set Language User on site from phone 
        public static readonly bool SetLangUser = false;
        
        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "8510e5d2-7e81-4345-9581-a9f5862453c9";

        //AdMob >> Please add the code ads in the Here and analytic.xml 
        //*********************************************************
        public static readonly ShowAds ShowAds = ShowAds.UnProfessional; 

        //Three times after entering the ad is displayed
        public static readonly int ShowAdInterstitialCount = 3;
        public static readonly int ShowAdRewardedVideoCount = 3;
        public static readonly int ShowAdNativeCount = 5;
        public static readonly int ShowAdAppOpenCount = 2;
        
        public static readonly bool ShowAdMobBanner = true;
        public static readonly bool ShowAdMobInterstitial = true;
        public static readonly bool ShowAdMobRewardVideo = true;
        public static readonly bool ShowAdMobNative = true;
        public static readonly bool ShowAdMobAppOpen = true;  
        public static readonly bool ShowAdMobRewardedInterstitial = true; 

        public static readonly string AdInterstitialKey = "ca-app-pub-5135691635931982/6168068662";
        public static readonly string AdRewardVideoKey = "ca-app-pub-5135691635931982/4663415300";
        public static readonly string AdAdMobNativeKey = "ca-app-pub-5135691635931982/2619721801";
        public static readonly string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/4967593321"; 
        public static readonly string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/1850136085";  

        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static readonly bool ShowFbBannerAds = false; 
        public static readonly bool ShowFbInterstitialAds = false; 
        public static readonly bool ShowFbRewardVideoAds = false; 
        public static readonly bool ShowFbNativeAds = false; 

        //YOUR_PLACEMENT_ID
        public static readonly string AdsFbBannerKey = "250485588986218_554026418632132"; 
        public static readonly string AdsFbInterstitialKey = "250485588986218_554026125298828"; 
        public static readonly string AdsFbRewardVideoKey = "250485588986218_554072818627492";  
        public static readonly string AdsFbNativeKey = "250485588986218_554706301897477";

        //Colony Ads >> Please add the code ad in the Here 
        //*********************************************************  
        public static readonly bool ShowColonyBannerAds = true;  
        public static readonly bool ShowColonyInterstitialAds = true; 
        public static readonly bool ShowColonyRewardAds = true;  

        public static readonly string AdsColonyAppId = "app6fa8d492324841b9b5";  
        public static readonly string AdsColonyBannerId = "vz8f1309aa856842248e";  
        public static readonly string AdsColonyInterstitialId = "vzd4f625080a1b4bc0be";  
        public static readonly string AdsColonyRewardedId = "vzb00816befb614810ac";

        //*********************************************************   

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file or AndroidManifest.xml
        //Facebook >> ../values/analytic.xml  
        //Google >> ../Properties/AndroidManifest.xml .. line 27
        //*********************************************************
        public static readonly bool EnableSmartLockForPasswords = false; 
         
        public static readonly bool ShowFacebookLogin = true;
        public static readonly bool ShowGoogleLogin = true; 
        public static readonly bool ShowWoWonderLogin = true; 

        public static readonly string AppNameWoWonder = "WoWonder"; 
        public static readonly string WoWonderDomainUri = "https://demo.wowonder.com"; 
        public static readonly string WoWonderAppKey = "131c471c8b4edf662dd0ebf7adf3c3d7365838b9"; 

        public static readonly string ClientId = "404363570731-j48d139m31tgaq2tj0gamg8ah430botj.apps.googleusercontent.com";

        //First Page
        //*********************************************************
        public static readonly bool ShowSkipButton = true; 
         
        public static readonly bool ShowRegisterButton = true; 
        public static readonly bool EnablePhoneNumber = false; 

        //Set Theme Full Screen App
        //*********************************************************
        public static readonly bool EnableFullScreenApp = false;
        public static bool EnablePictureToPictureMode = true; //>> Not Working >> Next update 

        //Data Channal Users >> About
        //*********************************************************
        public static readonly bool ShowEmailAccount = true;
        public static readonly bool ShowActivities = true; 

        //Tab >> 
        //*********************************************************
        public static readonly bool ShowArticle = true;
        public static readonly bool ShowMovies = true;
        public static readonly bool ShowShorts = true;  
        public static readonly bool ShowChannelPopular = true;  
         
        //how in search 
        public static readonly List<string> LastKeyWordList = new List<string>() { "Music", "Party", "Nature", "Snow", "Entertainment", "Holidays", "Covid19", "Comedy", "Politics", "Suspense" }; //#New 

        //Offline Watched Videos >>  
        //*********************************************************
        public static readonly bool AllowOfflineDownload = true;
        public static readonly bool AllowDownloadProUser = true;
        public static readonly bool AllowWatchLater = true; 
        public static readonly bool AllowRecentlyWatched = true; 
        public static readonly bool AllowPlayLists = true; 
        public static readonly bool AllowLiked = true; 
        public static readonly bool AllowShared = true; 
        public static readonly bool AllowPaid = true; 

        //Import && Upload Videos >>  
        //*********************************************************
        public static bool ShowButtonImport { get; set; } = true;
        public static bool ShowButtonUpload { get; set; } = true;

        //Last_Messages Page >>
        ///********************************************************* 
        public static readonly bool RunSoundControl = true;
        public static readonly int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
        public static readonly int MessageRequestSpeed = 3000; // 3 Seconds
         
        public static readonly int ShowButtonSkip = 10; // 6 Seconds 
         
        //Set Theme App >> Color - Tab
        //*********************************************************
        public static TabTheme SetTabDarkTheme = TabTheme.Light;

        public static readonly bool SetYoutubeTypeBadgeIcon = true;
        public static readonly bool SetVimeoTypeBadgeIcon = true;
        public static readonly bool SetDailyMotionTypeBadgeIcon = true;
        public static readonly bool SetTwichTypeBadgeIcon = true;
        public static readonly bool SetOkTypeBadgeIcon = true;
        public static readonly bool SetFacebookTypeBadgeIcon = true;

        //Bypass Web Errors 
        ///*********************************************************
        public static readonly bool TurnTrustFailureOnWebException = true;
        public static readonly bool TurnSecurityProtocolType3072On = true;

        //*********************************************************
        public static readonly bool RenderPriorityFastPostLoad = true;

        //Error Report Mode
        //*********************************************************
        public static readonly bool SetApisReportMode = false;
         
        public static readonly int AvatarSize = 60; 
        public static readonly int ImageSize = 400;

        //Home Page 
        //*********************************************************
        public static readonly bool ShowStockVideo = true;
        
        public static readonly int CountVideosTop = 10;  
        public static readonly int CountVideosLatest = 10;  
        public static readonly int CountVideosFav = 10;
        public static readonly int CountVideosLive = 13;
        public static readonly int CountVideosStock= 10;

        /// <summary>
        /// if Radius you can select how much Radius in the parameter #CardPlayerViewRadius
        /// </summary>
        public static readonly CardPlayerView CardPlayerView  = CardPlayerView.Square; 
        public static readonly float CardPlayerViewRadius = 10F;  

        public static readonly bool ShowGoLive = true;
        public static readonly string AppIdAgoraLive = "e1e7ecfd99264a74b71cb113d816ea86";

        //Settings 
        //*********************************************************
        public static readonly bool ShowEditPassword = true; 
        public static readonly bool ShowMonetization = true; //(Withdrawals)
        public static readonly bool ShowVerification = true; 
        public static readonly bool ShowBlockedUsers = true; 
        public static readonly bool ShowPoints = true; 
        public static readonly bool ShowSettingsTwoFactor = true;   
        public static readonly bool ShowSettingsManageSessions = true;   

        public static readonly bool ShowSettingsRateApp = true;  
        public static readonly int ShowRateAppCount = 5;  
         
        public static readonly bool ShowSettingsUpdateManagerApp = false; 

        public static readonly bool ShowGoPro = true; 

        public static readonly bool ShowClearHistory = true; 
        public static readonly bool ShowClearCache = true; 
         
        public static readonly bool ShowHelp = true; 
        public static readonly bool ShowTermsOfUse = true; 
        public static readonly bool ShowAbout = true; 
        public static readonly bool ShowDeleteAccount = true;

        //*********************************************************
        /// <summary>
        /// Currency
        /// CurrencyStatic = true : get currency from app not api 
        /// CurrencyStatic = false : get currency from api (default)
        /// </summary>
        public static readonly bool CurrencyStatic = false; 
        public static readonly string CurrencyIconStatic = "$"; 
        public static readonly string CurrencyCodeStatic = "USD"; 

        //********************************************************* 
        public static readonly bool RentVideosSystem = true; 
        
        //*********************************************************  
        public static readonly bool DonateVideosSystem = true;  
         
        //*********************************************************  
        /// <summary>
        /// Paypal and google pay using Braintree Gateway https://www.braintreepayments.com/
        /// 
        /// Add info keys in Payment Methods : https://prnt.sc/1z5bffc & https://prnt.sc/1z5b0yj
        /// To find your merchant ID :  https://prnt.sc/1z59dy8
        ///
        /// Tokenization Keys : https://prnt.sc/1z59smv
        /// </summary>
        public static readonly bool ShowPaypal = true;
        public static readonly string MerchantAccountId = "test"; 

        public static readonly string SandboxTokenizationKey = "sandbox_kt2f6mdh_hf4c******"; 
        public static readonly string ProductionTokenizationKey = "production_t2wns2y2_dfy45******";  

        public static readonly bool ShowCreditCard = true;
        public static readonly bool ShowBankTransfer = true;  

        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static readonly bool ShowInAppBilling = false;

        //*********************************************************   
        public static readonly bool ShowCashFree = true;   
         
        /// <summary>
        /// Currencies : http://prntscr.com/u600ok
        /// </summary>
        public static readonly string CashFreeCurrency = "INR";  

        /// <summary>
        /// Currencies : https://razorpay.com/accept-international-payments
        /// </summary>
        public static readonly string RazorPayCurrency = "INR"; 

        /// <summary>
        /// If you want RazorPay you should change id key in the analytic.xml file
        /// razorpay_api_Key >> .. line 18 
        /// </summary>
        public static readonly bool ShowRazorPay = true;  

        public static readonly bool ShowPayStack = true;  
        public static readonly bool ShowPaySera = true;   
        public static readonly bool ShowSecurionPay = true;  
        public static readonly bool ShowAuthorizeNet = true;  
        public static readonly bool ShowIyziPay = true;  
        public static readonly bool ShowAamarPay = true; //#New 
         
        //*********************************************************  

        public static readonly bool ShowVideoWithDynamicHeight = true;

        //********************************************************* 
        public static readonly bool ShowTextWithSpace = true; 

    }
}