

/*
 * Copyright 2014 HobbiSoft. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed On an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace com.projecttango.quickstartcsharp
{
    using Com.Google.Atap.Tangoservice;
    using System.Collections.Generic;
    using SuppressLint = Android.Annotation.SuppressLint;
    using Android.Opengl;
    using Android.Util;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Views;
    using Android.OS;
    using Android.Widget;

    /// <summary>
    /// Main Activity for the Tango  Quickstart. Demonstrates establishing a
    /// connection to the <seealso cref="Tango"/> service and printing the <seealso cref="TangoPose"/>
    /// data to the LogCat. Also demonstrates Tango lifecycle management through
    /// <seealso cref="TangoConfig"/>.
    /// </summary>
    /// 
    [Activity(Label = "QuickStart",
       MainLauncher = true,
       Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public const string EXTRA_KEY_PERMISSIONTYPE = "PERMISSIONTYPE";
        public const string EXTRA_VALUE_MOTION_TRACKING = "MOTION_TRACKING_PERMISSION";
        private static readonly string TAG = typeof(MainActivity).Name;
        private const string sTranslationFormat = "Translation:  {0:f}, {1:f}, {2:f}";
        private const string sRotationFormat = "Rotation: {0:f}, {1:f}, {2:f}";

        private TextView mTranslationTextView;
        private TextView mRotationTextView;

        private Tango mTango;
        private TangoConfig mConfig;
        private bool mIsTangoServiceConnected;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            mTranslationTextView = (TextView)FindViewById(Resource.Id.translation_text_view);
            mRotationTextView = (TextView)FindViewById(Resource.Id.rotation_text_view);

            // Instantiate Tango client
            mTango = new Tango(this);

            // Set up Tango configuration for motion tracking
            // If you want to use other APIs, add more appropriate to the config
            // like: mConfig.PutBoolean(TangoConfig.KEY_BOOLEAN_DEPTH, true)
            mConfig = new TangoConfig();
            mConfig = mTango.GetConfig(TangoConfig.ConfigTypeCurrent);
            mConfig.PutBoolean(TangoConfig.KeyBooleanMotiontracking, true);

        }

        protected override void OnResume()
        {
            base.OnResume();
            // Lock the Tango configuration and reconnect to the service each time
            // the app
            // is brought to the foreground.
            base.OnResume();
            Intent permissionIntent = new Intent();
            permissionIntent.SetAction("android.intent.action.REQUEST_TANGO_PERMISSION");
            permissionIntent.PutExtra(EXTRA_KEY_PERMISSIONTYPE, EXTRA_VALUE_MOTION_TRACKING);
            if (!mIsTangoServiceConnected)
            {
                StartActivityForResult(permissionIntent, Tango.TangoIntentActivitycode);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // Check which request we're responding to
            if (requestCode == Tango.TangoIntentActivitycode)
            {
                // Make sure the request was successful
                if (resultCode == Result.Canceled)
                {
                    Toast.MakeText(this, "This app requires Motion Tracking permission!", ToastLength.Long).Show();
                    Finish();
                    return;
                }
                try
                {
                    SetTangoListeners();
                }
                catch (TangoErrorException)
                {
                    Toast.MakeText(this, "Tango Error! Restart the app!", Android.Widget.ToastLength.Short).Show();
                }
                try
                {
                    mTango.Connect(mConfig);
                    mIsTangoServiceConnected = true;
                }
                catch (TangoOutOfDateException)
                {
                    Toast.MakeText(ApplicationContext, "Tango Service out of date!", Android.Widget.ToastLength.Short).Show();
                }
                catch (TangoErrorException)
                {
                    Toast.MakeText(ApplicationContext, "Tango Error! Restart the app!", Android.Widget.ToastLength.Short).Show();
                }

            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            // When the app is pushed to the background, unlock the Tango
            // configuration and disconnect
            // from the service so that other apps will behave properly.
            try
            {
                mTango.Disconnect();
                mIsTangoServiceConnected = false;
            }
            catch (TangoErrorException)
            {
                Toast.MakeText(ApplicationContext, "Tango Error!", Android.Widget.ToastLength.Short).Show();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void SetTangoListeners()
        {
            // Select coordinate frame pairs
            List<TangoCoordinateFramePair> framePairs = new List<TangoCoordinateFramePair>();
            framePairs.Add(new TangoCoordinateFramePair(TangoPoseData.CoordinateFrameStartOfService, TangoPoseData.CoordinateFrameDevice));

            // Add a listener for Tango pose data
            var listener = new TangoProxy.TangoListener(this);
            listener.OnPoseAvailableCallback = OnPoseAvailable;
            listener.OnTangoEventCallBack = OnTangoEvent;
            mTango.ConnectListener(framePairs, listener);
        }


        public void OnPoseAvailable(TangoPoseData pose)
        {
            // Format Translation and Rotation data

            string translationMsg = string.Format(sTranslationFormat, pose.Translation[0], pose.Translation[1], pose.Translation[2]);

            string rotationMsg = string.Format(sRotationFormat, pose.Rotation[0], pose.Rotation[1], pose.Rotation[2], pose.Rotation[3]);

            // Output to LogCat
            string logMsg = translationMsg + " | " + rotationMsg;
            Log.Info(TAG, logMsg);

            // Display data in TextViews. This must be done inside a
            // System.Threading.Thread.Start call because
            // it affects the UI, which will cause an error if performed
            // from the Tango
            // service thread
            RunOnUiThread(() =>
            {



                mTranslationTextView.Text = translationMsg;
                mRotationTextView.Text = rotationMsg;
            });
        }


        public void OnXyzIjAvailable(TangoXyzIjData arg0)
        {
            // Ignoring XyzIj data
        }

        public void OnTangoEvent(TangoEvent arg0)
        {
            // Ignoring TangoEvents
        }


    }

}

