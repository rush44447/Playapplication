﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AamarPay;
using Android.App;
using Android.Widget;
using Newtonsoft.Json;
using Org.Json;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Utils;
using PlayTubeClient.Classes.Payment;
using PlayTubeClient.RestCalls;

namespace PlayTube.Payment
{
    public class InitAamarPayPayment : InitAamarPay.IOnInitListener, InitAamarPay.ITransactionInfoListener
    {
        private readonly Activity ActivityContext;
        private readonly TabbedMainActivity GlobalContext;
        private DialogBuilder DialogBuilder;
        private AlertDialog AlertDialog;
        private string TransactionId;
        private InitAamarPay AamarPay;
        private string Price;

        public InitAamarPayPayment(Activity activity)
        {
            try
            {
                ActivityContext = activity;
                GlobalContext = TabbedMainActivity.GetInstance();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //AamarPay
        public void BtnAamarPayOnClick(string price)
        {
            try
            {
                Price = price;
                var option = ListUtils.MyChannelList.FirstOrDefault(); 
                var currency = ListUtils.MySettingsList?.PaymentCurrency ?? "USD";
                 
                DialogBuilder = new DialogBuilder(ActivityContext, AlertDialog);

                // Initiate payment
                AamarPay = new InitAamarPay(ActivityContext, ListUtils.MySettingsList?.AamarpayStoreId, ListUtils.MySettingsList?.AamarpaySignatureKey);

                switch (ListUtils.MySettingsList?.AamarpayMode)
                {
                    case "live":
                        // Set Live Mode
                        AamarPay.TestMode(false);
                        break;
                    case "sandbox":
                        // Set Test Mode
                        AamarPay.TestMode(true);
                        break;
                    default:
                        // Set Test Mode
                        AamarPay.TestMode(true);
                        break;
                }

                // Auto generate Trx
                AamarPay.AutoGenerateTransactionId(true);

                // Generate unique transaction id
                TransactionId = AamarPay.generate_trx_id();

                DialogBuilder.ShowLoading();
                AamarPay.SetTransactionParameter(price, currency, "Pay the card");
                AamarPay.SetCustomerDetails(option.Name, option.Email, option.PhoneNumber, "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1", "Istanbul", "Turkey");
                AamarPay.InitPgw(this); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnInitFailure(bool? error, string message)
        {
            DialogBuilder.DismissDialog();
            DialogBuilder.ErrorPopUp(message);
        }

        public void OnPaymentSuccess(JSONObject jsonObject)
        { 
            try
            {
                var data = JsonConvert.DeserializeObject<SuccessAamarPayObject>(jsonObject.ToString());
                if (data != null)
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SuccessAamarPay(data.MerTxnid, data.PayStatus) });
                    DialogBuilder.DismissDialog();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            }
        }

        public void OnPaymentFailure(JSONObject jsonObject)
        {
            DialogBuilder.DismissDialog();
        }

        public void OnPaymentProcessingFailed(JSONObject jsonObject)
        {
            DialogBuilder.DismissDialog();
        }

        public void OnPaymentCancel(JSONObject jsonObject)
        {
            try
            {
                AamarPay.GetTransactionInfo(jsonObject.GetString("trx_id"), this);
            }
            catch (JSONException e)
            {
                Console.WriteLine(e);
            }
        }
         
        public void OnSuccess(JSONObject jsonObject)
        {
            DialogBuilder.DismissDialog();
        }

        public void OnFailure(bool? error, string message)
        {
            DialogBuilder.DismissDialog();
            DialogBuilder.ErrorPopUp(message);
        }
        
        private async Task SuccessAamarPay(string merTxnid, string payStatus)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (Methods.CheckConnectivity())
                    {
                        var (apiStatus, respond) = await RequestsAsync.Payment.TopWalletAamarAsync(Price, merTxnid, payStatus);
                        if (apiStatus == 200)
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                        }
                        else
                            Methods.DisplayReportResult(ActivityContext, respond);
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}