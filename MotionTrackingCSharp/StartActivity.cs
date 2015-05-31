/*
 * Copyright 2014 Google Inc. All Rights Reserved.
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

    using Tango = Com.Google.Atap.Tangoservice.Tango;

    using Activity = Android.App.Activity;
    using Intent = Android.Content.Intent;
    using Bundle = Android.OS.Bundle;
    using View = Android.Views.View;
    using Button = Android.Widget.Button;
    using Toast = Android.Widget.Toast;
    using ToggleButton = Android.Widget.ToggleButton;
    using Android.App;


    /// <summary>
    /// Application's entry point where the user gets to select a certain configuration and start the
    /// next activity.
    /// </summary>
    [Activity(Label = "MotionTracking",
     MainLauncher = true,
     Icon = "@drawable/icon")]
    public class StartActivity : Activity, View.IOnClickListener
	{
		public static string KEY_MOTIONTRACKING_AUTORECOVER = "com.projecttango.experiments.csharpmotiontracking.useautorecover";
		private ToggleButton mAutoResetButton;
		private Button mStartButton;
		private bool mUseAutoReset;

		protected  override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			StartActivityForResult(Tango.GetRequestPermissionIntent(Tango.PermissiontypeMotionTracking), Tango.TangoIntentActivitycode);
            SetContentView(motiontrackingcsharp.Resource.Layout.start);
           
			mAutoResetButton = (ToggleButton) FindViewById(motiontrackingcsharp.Resource.Id.autoresetbutton);
			mStartButton = (Button) FindViewById(motiontrackingcsharp.Resource.Id.startbutton);
            mAutoResetButton.Click += MAutoResetButton_Click;
            mStartButton.Click += MStartButton_Click;
			mStartButton.SetOnClickListener( this);
			mUseAutoReset = mAutoResetButton.Checked;
		}

        private void MStartButton_Click(object sender, System.EventArgs e)
        {
            OnClick(sender as View);
        }

        private void MAutoResetButton_Click(object sender, System.EventArgs e)
        {
            OnClick(sender as View);
        }

        public void OnClick(View v)
		{
			switch (v.Id)
			{
			case motiontrackingcsharp.Resource.Id.startbutton:
				startMotionTracking();
				break;
			case motiontrackingcsharp.Resource.Id.autoresetbutton:
				mUseAutoReset = mAutoResetButton.Checked;
				break;
			}
		}

		private void startMotionTracking()
		{
			Intent startmotiontracking = new Intent(this, typeof(com.projecttango.motiontrackingcsharp.MotionTracking));
			startmotiontracking.PutExtra(KEY_MOTIONTRACKING_AUTORECOVER, mUseAutoReset);
			StartActivity(startmotiontracking);
		}

		protected override  void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			// Check which request we're responding to
			if (requestCode == Tango.TangoIntentActivitycode)
			{
				// Make sure the request was successful
				if (resultCode == Result.Canceled)
				{
					Toast.MakeText(this, motiontrackingcsharp.Resource.String.motiontrackingpermission, Android.Widget.ToastLength.Short).Show();
					Finish();
				}
			}
		}
	}

}