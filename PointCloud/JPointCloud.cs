
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

namespace com.projecttango.pointcloudcsharp
{
    using System;
    using System.Collections.Generic;
  
    using Android.Opengl;
    using Android.Util;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Views;
    using Android.OS;
    using Android.Widget;
    using Java.IO;
	
    using IOnClickListener = Android.Views.View.IOnClickListener;
    using Com.Google.Atap.Tangoservice;

	/// <summary>
	/// Main Activity class for the Point Cloud Sample. Handles the connection to the
	/// <seealso cref="Tango"/> service and propagation of Tango XyzIj data to OpenGL and
	/// Layout views. OpenGL rendering logic is delegated to the <seealso cref="PCrenderer"/>
	/// class.
	/// </summary>
     [Activity(Label = "JPointCloud"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
	public class JPointCloud : Activity, IOnClickListener
	{

		public const string EXTRA_KEY_PERMISSIONTYPE = "PERMISSIONTYPE";
		public const string EXTRA_VALUE_MOTION_TRACKING = "MOTION_TRACKING_PERMISSION";
		private static readonly string TAG = typeof(JPointCloud).Name;
		private static int SECS_TO_MILLI = 1000;
		private Tango mTango;
		private TangoConfig mConfig;

		private PCRenderer mRenderer;
		private GLSurfaceView mGLView;

		private TextView mDeltaTextView;
		private TextView mPoseCountTextView;
		private TextView mPoseTextView;
		private TextView mQuatTextView;
		private TextView mPoseStatusTextView;
		private TextView mTangoEventTextView;
		private TextView mPointCountTextView;
		private TextView mTangoServiceVersionTextView;
		private TextView mApplicationVersionTextView;
		private TextView mAverageZTextView;
		private TextView mFrequencyTextView;

		private Button mFirstPersonButton;
		private Button mThirdPersonButton;
		private Button mTopDownButton;
        private List<TangoCoordinateFramePair> framePairs = new List<TangoCoordinateFramePair>();
		private int count;
		private float mDeltaTime;
		private float mPosePreviousTimeStamp;
		private float mXyIjPreviousTimeStamp;
		private float mCurrentTimeStamp;
		private string mServiceVersion;
		private bool mIsTangoServiceConnected;

		protected  override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView( Resource.Layout.activity_jpoint_cloud);  
			Title = GetString(Resource.String.app_name);

			mPoseTextView = (TextView) FindViewById(Resource.Id.pose);
			mQuatTextView = (TextView) FindViewById(Resource.Id.quat);
			mPoseCountTextView = (TextView) FindViewById(Resource.Id.posecount);
			mDeltaTextView = (TextView) FindViewById(Resource.Id.deltatime);
			mTangoEventTextView = (TextView) FindViewById(Resource.Id.tangoevent);
			mPoseStatusTextView = (TextView) FindViewById(Resource.Id.status);
			mPointCountTextView = (TextView) FindViewById(Resource.Id.pointCount);
			mTangoServiceVersionTextView = (TextView) FindViewById(Resource.Id.version);
			mApplicationVersionTextView = (TextView) FindViewById(Resource.Id.appversion);
			mAverageZTextView = (TextView) FindViewById(Resource.Id.averageZ);
			mFrequencyTextView = (TextView) FindViewById(Resource.Id.frameDelta);

			mFirstPersonButton = (Button) FindViewById(Resource.Id.first_person_button);
			mFirstPersonButton.SetOnClickListener(this);
			mThirdPersonButton = (Button) FindViewById(Resource.Id.third_person_button);
			mThirdPersonButton.SetOnClickListener(this);
			mTopDownButton = (Button) FindViewById(Resource.Id.top_down_button);
			mTopDownButton.SetOnClickListener(this);

			mTango = new Tango(this);
			mConfig = new TangoConfig();
			mConfig = mTango.GetConfig(TangoConfig.ConfigTypeCurrent);
			mConfig.PutBoolean(TangoConfig.KeyBooleanDepth, true);

			int maxDepthPoints = mConfig.GetInt("max_point_cloud_elements");
			mRenderer = new PCRenderer(maxDepthPoints);
			mGLView = (GLSurfaceView) FindViewById(Resource.Id.gl_surface_view);
            mGLView.SetEGLContextClientVersion(2);
            mGLView.SetRenderer(mRenderer);
			mGLView.RenderMode =  Rendermode.WhenDirty;

			PackageInfo packageInfo;
			try
			{
			packageInfo = this.PackageManager.GetPackageInfo(this.PackageName, 0);
				mApplicationVersionTextView.Text = packageInfo.VersionName;
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString());
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			// Display the version of Tango Service
			mServiceVersion = mConfig.GetString("tango_service_library_version");
			mTangoServiceVersionTextView.Text = mServiceVersion;
			mIsTangoServiceConnected = false;
		}

		protected  void OnPause()
		{
			base.OnPause();
			try
			{
				mTango.Disconnect();
				mIsTangoServiceConnected = false;
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(ApplicationContext, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
			}
		}

		protected  override void OnResume()
		{
			base.OnResume();
			Intent permissionIntent = new Intent();
			permissionIntent.SetAction("android.intent.action.REQUEST_TANGO_PERMISSION");
			permissionIntent.PutExtra(EXTRA_KEY_PERMISSIONTYPE, EXTRA_VALUE_MOTION_TRACKING);
			if (!mIsTangoServiceConnected)
			{
				StartActivityForResult(permissionIntent, Tango.TangoIntentActivitycode);
			}
			Log.Info(TAG, "onResumed");
		}

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // Check which request we're responding to
            if (requestCode == Tango.TangoIntentActivitycode)
            {
                Log.Info(TAG, "Triggered");
                // Make sure the request was successful
                if (resultCode == Result.Canceled)
                {
                    Toast.MakeText(this, Resource.String.motiontrackingpermission, ToastLength.Long).Show();
                    Finish();
                    return;
                }
                try
                {
                    SetTangoListeners();
                }
                catch (TangoErrorException)
                {
                    Toast.MakeText(this, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
                }
                try
                {
                    mTango.Connect(mConfig);
                    mIsTangoServiceConnected = true;
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
        }

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		public void OnClick(View v)
		{
			switch (v.Id)
			{
			case Resource.Id.first_person_button:
				mRenderer.setFirstPersonView();
				break;
			case Resource.Id.third_person_button:
				mRenderer.setThirdPersonView();
				break;
			case Resource.Id.top_down_button:
				mRenderer.setTopDownView();
				break;
			default:
				Log.Wtf(TAG, "Unrecognized button click.");
				return;
			}
		}

		public override bool OnTouchEvent(MotionEvent args)
		{
			return mRenderer.onTouchEvent(args);
		}

		private void SetUpExtrinsics()
		{
			// Set device to imu matrix in Model Matrix Calculator.
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

			// Set color camera to imu matrix in Model Matrix Calculator.
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

		private void SetTangoListeners()
		{
			// Configure the Tango coordinate frame pair

			framePairs.Add(new TangoCoordinateFramePair(TangoPoseData.CoordinateFrameStartOfService, TangoPoseData.CoordinateFrameDevice));
			// Listen for new Tango data

            var listener = new TangoProxy.TangoListener(this);
            listener.OnPoseAvailableCallback = OnPoseAvailable;
            listener.OnXyzIjAvailableCallBack = OnXyzIjAvailable;
            listener.OnTangoEventCallBack = OnTangoEvent;
           
            mTango.ConnectListener(framePairs, listener);
          
		}
			public void OnPoseAvailable(TangoPoseData pose)
			{
				mDeltaTime = (float)(pose.Timestamp - mPosePreviousTimeStamp) * SECS_TO_MILLI;
				mPosePreviousTimeStamp = (float) pose.Timestamp;
				count++;
				mRenderer.ModelMatCalculator.updateModelMatrix(pose.GetTranslationAsFloats(), pose.GetRotationAsFloats());
				mRenderer.updateViewMatrix();
				mGLView.RequestRender();
				// Update the UI with TangoPose information
			    RunOnUiThread(() =>
                {
                    string translationString = "[" + threeDec.format(pose.Translation[0]) + ", " + threeDec.format(pose.Translation[1]) + ", " + threeDec.format(pose.Translation[2]) + "] ";
					string quaternionString = "[" + threeDec.format(pose.Rotation[0]) + ", " + threeDec.format(pose.Rotation[1]) + ", " + threeDec.format(pose.Rotation[2]) + ", " + threeDec.format(pose.Rotation[3]) + "] ";

					// Display pose data On screen in TextViews
					mPoseTextView.Text = translationString;
					mQuatTextView.Text = quaternionString;
					mPoseCountTextView.Text = Convert.ToString(count);
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
			
			public void  OnXyzIjAvailable(TangoXyzIjData xyzIj)
			{
				mCurrentTimeStamp = (float) xyzIj.Timestamp;

				float frameDelta = (mCurrentTimeStamp - mXyIjPreviousTimeStamp) * SECS_TO_MILLI;
				mXyIjPreviousTimeStamp = mCurrentTimeStamp;
                byte[] buffer = new byte[xyzIj.XyzCount * 3 * 4];
				FileInputStream fileStream = new FileInputStream(xyzIj.XyzParcelFileDescriptor.FileDescriptor);
				try
				{
					fileStream.Read(buffer, xyzIj.XyzParcelFileDescriptorOffset, buffer.Length);
					fileStream.Close();
				}
				catch (IOException e)
				{
					System.Diagnostics.Debug.WriteLine(e.ToString());
					System.Diagnostics.Debug.WriteLine(e.StackTrace);
				}
				try
				{
					TangoPoseData pointCloudPose = mTango.GetPoseAtTime(mCurrentTimeStamp, framePairs[0]);

                //	mRenderer.PointCloud.UpdatePoints(buffer, xyzIj.XyzCount);
                    mRenderer.PointCloud.UpdatePoints((Java.Nio.FloatBuffer)buffer);
					mRenderer.ModelMatCalculator.updatePointCloudModelMatrix(pointCloudPose.GetTranslationAsFloats(), pointCloudPose.GetRotationAsFloats());
					mRenderer.PointCloud.ModelMatrix = mRenderer.ModelMatCalculator.PointCloudModelMatrixCopy;
				}
				catch (TangoErrorException)
				{
					Toast.MakeText(Android.App.Application.Context, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
				}
				catch (TangoInvalidException)
				{
                    Toast.MakeText(Android.App.Application.Context, Resource.String.TangoError, Android.Widget.ToastLength.Short).Show();
				}

				// Must run UI changes On the UI thread. Running in the Tango
				// service thread
				// will result in an error.
				RunOnUiThread(() => 
                {
                    mPointCountTextView.Text = Convert.ToString(xyzIj.XyzCount);
				    mFrequencyTextView.Text = "" + threeDec.format(frameDelta);
				    mAverageZTextView.Text = "" + threeDec.format(mRenderer.PointCloud.AverageZ);
                });
			}

		

            public void OnTangoEvent(TangoEvent args)
            {
                RunOnUiThread(() => 
                {
                    mTangoEventTextView.Text = args.EventKey + ": " + args.EventValue;
                });
            }

		
       
	}

}