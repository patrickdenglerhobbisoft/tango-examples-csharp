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

using Com.Google.Atap.Tangoservice;
namespace com.projecttango.areadescriptioncsharp
{

    
    using Android.Opengl;
    using Android.Util;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Views;
    using Android.OS;
    using Android.Widget;
    using OnTangoUpdateListener = Tango.IOnTangoUpdateListener;

    using IOnClickListener = Android.Views.View.IOnClickListener;

    using R = Resource;
    using id = Resource.Id;
    using Layout = Resource.Layout;
    using _string = Resource.String;
	/// <summary>
	/// This Class shows a dialog to Set the name of an ADF. When you press okay
	/// SetNameLocation Call back is called where Setting the name should be handled.
	/// </summary>
	public class SetADFNameDialog : DialogFragment, View.IOnClickListener
	{
		private EditText mNameEditText;
		private TextView mUUIDTextView;
		internal SetNameCommunicator mCommunicator;
  
		public override void OnAttach(Activity activity)
		{
			base.OnAttach(activity);
			mCommunicator = (SetNameCommunicator) activity;
		}

		public override View OnCreateView(LayoutInflater inflator, ViewGroup container, Bundle savedInstanceState)
		{
         

			View dialogView = inflator.Inflate(Resource.Layout.set_name_dialog, null);
			Dialog.SetTitle(Resource.String.set_name_dialogTitle);
            mNameEditText = (EditText)dialogView.FindViewById(Resource.Id.name);
            mUUIDTextView = (TextView)dialogView.FindViewById(Resource.Id.uuidDisplay);
            dialogView.FindViewById(Resource.Id.Ok).Click += OnClick;
            dialogView.FindViewById(Resource.Id.cancel).Click += OnClick;
			Cancelable = false;
			string name = this.Arguments.GetString("name");
			string id = this.Arguments.GetString("id");
			if (name != null)
			{
				mNameEditText.Text = name;
			}
			if (id != null)
			{
				mUUIDTextView.Text = id;
			}
			return dialogView;
		}
       
        void OnClick(object sender, System.EventArgs e)
        {
            // manage signature mismatch of interface (fix interface later)
            OnClick(sender as View);
        }
		public void OnClick(View v)
		{
			switch (v.Id)
			{
			case id.Ok:
				mCommunicator.SetName(mNameEditText.Text.ToString(), mUUIDTextView.Text.ToString());
				Dismiss();
				break;
			case id.cancel:
				Dismiss();
				break;
			}
		}

		internal interface SetNameCommunicator
		{
			void SetName(string name, string uuid);
		}
	}

}