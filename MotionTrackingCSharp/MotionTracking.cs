using System;
using System.Collections.Generic;

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

namespace com.projecttango.motiontrackingcsharp
{


    //using OnTangoUpdateListener = Com.Google.Atap.Tangoservice.ITangoListener;
    using Android.App;
    using Com.Google.Atap.Tangoservice;
    using Android.Content;
    using Android.Views;
    using Android.OS;
    using Android.Widget;
    using Android.Content.PM;
    using NameNotFoundException = Android.Content.PM.PackageManager.NameNotFoundException;
    using Android.Opengl;
    using Android.Util;
    using GLSurfaceView = Android.Opengl.GLSurfaceView;





    /// <summary>
    /// Main Activity class for the Motion Tracking API Sample. Handles the
    /// connection to the Tango service and propagation of Tango pose data to OpenGL
    /// and Layout views. OpenGL rendering logic is delegated to the
    /// <seealso cref="MTGLRenderer"/> class.
    /// </summary>
    /// 

    [Activity(Label = "MotionTracking",
               Icon = "@drawable/icon")]
	public class MotionTracking : Activity, View.IOnClickListener
    {
 
		private static string TAG = typeof(MotionTracking).Name;
        static int SECS_TO_MILLISECS = 1000;
		private Tango mTango;
		private TangoConfig mConfig;
        private TextView mDeltaTextView;
        private TextView mPoseCountTextView;
        private TextView mPoseTextView;
        private TextView mQuatTextView;
        private TextView mPoseStatusTextView;
        private TextView mTangoServiceVersionTextView;
        private TextView mApplicationVersionTextView;
        private TextView mTangoEventTextView;
		private Button mMotionResetButton;
        private float mPreviousTimeStamp;
        private int count;
        private float mDeltaTime;
        private bool mIsAutoRecovery;
        private MTGLRenderer mRenderer;
        private GLSurfaceView mGLView;
        static string KEY_MOTIONTRACKING_AUTORECOVER = "com.projecttango.experiments.csharpmotiontracking.useautorecover";
        private  string _TAG;

        
        
        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
            var layOut = Resource.Layout.activity_motion_tracking;
            _TAG = this.GetType().Name;
            SetContentView(layOut);
			Intent intent = Intent;
            mIsAutoRecovery = intent.GetBooleanExtra(KEY_MOTIONTRACKING_AUTORECOVER, false);
			// Text views for displaying translation and rotation data
			mPoseTextView = (TextView) FindViewById(Resource.Id.pose);
			mQuatTextView = (TextView) FindViewById(Resource.Id.quat);
			mPoseCountTextView = (TextView) FindViewById(Resource.Id.posecount);
			mDeltaTextView = (TextView) FindViewById(Resource.Id.deltatime);
			mTangoEventTextView = (TextView) FindViewById(Resource.Id.tangoevent);
			// Buttons for selecting camera view and Set up button click listeners
			FindViewById(Resource.Id.first_person_button).SetOnClickListener(this);
			FindViewById(Resource.Id.third_person_button).SetOnClickListener(this);
			FindViewById(Resource.Id.top_down_button).SetOnClickListener(this);

			// Button to reset motion tracking
			mMotionResetButton = (Button) FindViewById(Resource.Id.resetmotion);

			// Text views for the status of the pose data and Tango library versions
			mPoseStatusTextView = (TextView) FindViewById(Resource.Id.status);
			mTangoServiceVersionTextView = (TextView) FindViewById(Resource.Id.version);
			mApplicationVersionTextView = (TextView) FindViewById(Resource.Id.appversion);

			// OpenGL view where all of the graphics are drawn
			mGLView = (GLSurfaceView) FindViewById(Resource.Id.gl_surface_view);

			// Set up button click listeners
            mMotionResetButton.Click += mMotionResetButton_Click;

			// Configure OpenGL renderer
			mRenderer = new MTGLRenderer();
			mGLView.SetEGLContextClientVersion( 2);
			mGLView.SetRenderer(mRenderer);
            mGLView.RenderMode = Rendermode.WhenDirty;

			// Instantiate the Tango service
			mTango = new Tango(this);
			// Create a new Tango Configuration and enable the MotionTracking API
			mConfig = new TangoConfig();
			mConfig = mTango.GetConfig(TangoConfig.ConfigTypeCurrent);
			mConfig.PutBoolean(TangoConfig.KeyBooleanMotiontracking, true);

			// The Auto-Recovery ToggleButton Sets a boolean variable to determine
			// if the
			// Tango service should automatically attempt to recover when
			// / MotionTracking enters an invalid state.
			if (mIsAutoRecovery)
			{
				mConfig.PutBoolean(TangoConfig.KeyBooleanAutorecovery, true);
				Log.Info(TAG, "Auto Recovery On");
			}
			else
			{
                mConfig.PutBoolean(TangoConfig.KeyBooleanAutorecovery, false);
				Log.Info(TAG, "Auto Recovery Off");
			}

			PackageInfo packageInfo;
			try
			{
			packageInfo = this.PackageManager.GetPackageInfo(this.PackageName, 0);
				mApplicationVersionTextView.Text = packageInfo.VersionName;
			}
			catch (NameNotFoundException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			// Display the library version for debug purposes
			mTangoServiceVersionTextView.Text = mConfig.GetString("tango_service_library_version");

		}

        void mMotionResetButton_Click(object sender, EventArgs e)
        {
            // proxy between event signatures
            OnClick(sender as View);
        }

		/// <summary>
		/// Set up the TangoConfig and the listeners for the Tango service, then
		/// begin using the Motion Tracking API. This is called in response to the
		/// user clicking the 'Start' Button.
		/// </summary>
        private void SetTangoListeners()
        {
            // Lock configuration and connect to Tango
            // Select coordinate frame pair

            // Original line in class:  final java.Util.ArrayList<com.google.atap.tangoservice.TangoCoordinateFramePair> framePairs = new java.Util.ArrayList<com.google.atap.tangoservice.TangoCoordinateFramePair>();
            List<TangoCoordinateFramePair> framePairs = new List<TangoCoordinateFramePair>();
            framePairs.Add(new TangoCoordinateFramePair(TangoPoseData.CoordinateFrameStartOfService, TangoPoseData.CoordinateFrameDevice));
            // Listen for new Tango data
            var listener = new  TangoProxy.TangoListener(this);
            listener.OnPoseAvailableCallback = OnPoseAvailable;
            listener.OnTangoEventCallBack = OnTangoEvent;

            mTango.ConnectListener(framePairs, listener);
        }

        public void OnTangoEvent(TangoEvent args)
        {
            RunOnUiThread(() =>
            {
                mTangoEventTextView.Text = args.EventKey + ": " + args.EventValue;
            });
        }


        public void OnPoseAvailable(TangoPoseData pose)
        {
            RunOnUiThread(() =>
            {

                // Log whenever Motion Tracking enters a n invalid state
                if (!mIsAutoRecovery && (pose.StatusCode == TangoPoseData.PoseInvalid))
                {
                    Log.Wtf(this.GetType().Name, "Invalid State");
                }
                mDeltaTime = (float)(pose.Timestamp - mPreviousTimeStamp) * SECS_TO_MILLISECS;
                mPreviousTimeStamp = (float)pose.Timestamp;
                Log.Info(_TAG, "Delta Time is: " + mDeltaTime);
                count++;
                // Update the OpenGL renderable objects with the new Tango Pose
                // data
                if (mRenderer.Trajectory != null)
                {
                    float[] translation = pose.GetTranslationAsFloats();
                    mRenderer.Trajectory.updateTrajectory(translation);
                    mRenderer.ModelMatCalculator.updateModelMatrix(translation, pose.GetRotationAsFloats());
                    mRenderer.updateViewMatrix();
                    mGLView.RequestRender();
                }

                string translationString = "[" + threeDec.format(pose.Translation[0]) + ", " + threeDec.format(pose.Translation[1]) + ", " + threeDec.format(pose.Translation[2]) + "] ";
                string quaternionString = "[" + threeDec.format(pose.Rotation[0]) + ", " + threeDec.format(pose.Rotation[1]) + ", " + threeDec.format(pose.Rotation[2]) + ", " + threeDec.format(pose.Rotation[3]) + "] ";

                // Display pose data On screen in TextViews
                mPoseTextView.Text = translationString;
                mQuatTextView.Text = quaternionString;
                mPoseCountTextView.Text = count.ToString();
                mDeltaTextView.Text = threeDec.format(mDeltaTime);
                if (pose.StatusCode == TangoPoseData.PoseValid)
                {
                    mPoseStatusTextView.Text = "Valid";
                }
                else if (pose.StatusCode == TangoPoseData.PoseInvalid)
                {
                    mPoseStatusTextView.Text = "Invalid";
                }
                else if (pose.StatusCode == TangoPoseData.PoseInitializing)
                {
                    mPoseStatusTextView.Text = "Initializing";
                }
                else if (pose.StatusCode == TangoPoseData.PoseUnknown)
                {
                    mPoseStatusTextView.Text = "Unknown";
                }
            });
           
        }

     

		private void motionReset()
		{
			mTango.ResetMotionTracking();
		}

		protected override void OnPause()
		{
			base.OnPause();
			try
			{
				mTango.Disconnect();
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			try
			{
				SetTangoListeners();
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
			}
			try
			{
				mTango.Connect(mConfig);
			}
			catch (TangoOutOfDateException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoOutOfDateException, Android.Widget.ToastLength.Short).Show();
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
			}
			SetUpExtrinsics();
		}

		protected override  void OnDestroy()
		{
			base.OnDestroy();
		}
		public  void OnClick(View v)
		{
			switch (v.Id)
			{
			case Resource.Id.first_person_button:
				mRenderer.setFirstPersonView();
				break;
			case Resource.Id.top_down_button:
				mRenderer.setTopDownView();
				break;
			case Resource.Id.third_person_button:
				mRenderer.setThirdPersonView();
				break;
			case Resource.Id.resetmotion:
				motionReset();
				break;
			default:
				Log.Wtf(TAG, "Unknown button click");
				return;
			}
		}

		public override bool OnTouchEvent(MotionEvent args)
		{
			return mRenderer.onTouchEvent(args);
		}

		private void SetUpExtrinsics()
		{
			// Get device to imu matrix.
			TangoPoseData device2IMUPose = new TangoPoseData();
			TangoCoordinateFramePair framePair = new TangoCoordinateFramePair();
			framePair.BaseFrame = TangoPoseData.CoordinateFrameImu;
			framePair.TargetFrame = TangoPoseData.CoordinateFrameDevice;
			try
			{
				device2IMUPose = mTango.GetPoseAtTime(0.0, framePair);
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
			}
			mRenderer.ModelMatCalculator.SetDevice2IMUMatrix(device2IMUPose.GetTranslationAsFloats(), device2IMUPose.GetRotationAsFloats());

			// Get color camera to imu matrix.
			TangoPoseData color2IMUPose = new TangoPoseData();

			framePair.BaseFrame = TangoPoseData.CoordinateFrameImu;
			framePair.TargetFrame = TangoPoseData.CoordinateFrameCameraColor;
			try
			{
				color2IMUPose = mTango.GetPoseAtTime(0.0, framePair);
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
			}
			mRenderer.ModelMatCalculator.SetColorCamera2IMUMatrix(color2IMUPose.GetTranslationAsFloats(), color2IMUPose.GetRotationAsFloats());
		}
	}
   
}