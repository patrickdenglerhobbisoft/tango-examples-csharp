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
    using Com.Google.Atap.Tangoservice;
    
    using Android.Content;
    using Android.Widget;
  
    using Android.Views;
    using Android.App;
    using Android.OS;
   
    [Activity(Label = "MotionTracking", 
        MainLauncher = true, 
        Icon = "@drawable/icon")]
	public class StartActivity : Activity, View.IOnClickListener
	{
		public const string KEY_MOTIONTRACKING_AUTORECOVER = "com.projecttango.motiontrackingcsharp.useautorecover";
		public const string EXTRA_KEY_PERMISSIONTYPE = "PERMISSIONTYPE";
		public const string EXTRA_VALUE_MOTION_TRACKING = "MOTION_TRACKING_PERMISSION";
		private ToggleButton mAutoResetButton;
		private Button mStartButton;
		private bool mUseAutoReset;

        
		protected  override void OnCreate(Bundle savedInstanceState)
		{
            base.OnCreate(savedInstanceState);  // TODO Is Based requured here?
           
            Intent permissionIntent = new Intent();
            permissionIntent.SetAction( "android.intent.action.REQUEST_TANGO_PERMISSION");
            permissionIntent.PutExtra(EXTRA_KEY_PERMISSIONTYPE, EXTRA_VALUE_MOTION_TRACKING);
            StartActivityForResult(permissionIntent, Tango.TangoIntentActivitycode);
            SetContentView(Resource.Layout.start);
          
            this.Title = GetString(Resource.String.app_name);
            mAutoResetButton = (ToggleButton)FindViewById(Resource.Id.autoresetbutton);
            mStartButton = (Button)FindViewById(Resource.Id.startbutton);
            mAutoResetButton.Click += mAutoResetButton_Click;
            mStartButton.Click += mAutoResetButton_Click;
            mUseAutoReset = mAutoResetButton.Checked;
		}

        void mAutoResetButton_Click(object sender, System.EventArgs e)
        {
            // proxy between onclick signatures
            OnClick(sender as View);
        }

		public  void OnClick(View v)
		{
			switch (v.Id)
			{
			case Resource.Id.startbutton:
				startMotionTracking();
				break;
			case Resource.Id.autoresetbutton:
				mUseAutoReset = mAutoResetButton.Checked;
				break;
			}
		}

		private void startMotionTracking()
		{
			Intent startmotiontracking = new Intent(this, typeof(MotionTracking));
			startmotiontracking.PutExtra(KEY_MOTIONTRACKING_AUTORECOVER, mUseAutoReset);
			StartActivity(startmotiontracking);
		}
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			// Check which request we're responding to
			if (requestCode == Tango.TangoIntentActivitycode)
			{
				// Make sure the request was successful
				if (resultCode == Result.Canceled)
				{
					Toast.MakeText(this, Resource.String.motiontrackingpermission, Android.Widget.ToastLength.Short).Show();
					Finish();
				}
			}
		}
	}

}