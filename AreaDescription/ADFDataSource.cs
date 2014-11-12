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

namespace com.projecttango.areadescriptionjava
{

    using Com.Google.Atap.Tangoservice;
    using Tango = Com.Google.Atap.Tangoservice.Tango;
    using TangoAreaDescriptionMetaData = Com.Google.Atap.Tangoservice.TangoAreaDescriptionMetaData;
    using TangoErrorException = Com.Google.Atap.Tangoservice.TangoErrorException;

	using Context = Android.Content.Context;
	using Toast = Android.Widget.Toast;

	/// <summary>
	/// This class interfaces a Tango Object and maintains a 
	/// full list of ADF UUIds. Whenever an adf is deleted or added,
	/// getFullUUIDList needs to be called to update the UUIDList within this class.
	/// 
	/// </summary>
	public class ADFDataSource
	{
		private Tango mTango;
		private IList<string> mFullUUIDList;
		private Context mContext;
		public ADFDataSource(Context context)
		{
			mContext = context;
			mTango = new Tango(context);
			try
			{
				mFullUUIDList = mTango.ListAreaDescriptions();
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(mContext,Resource.String.tango_error,Android.Widget.ToastLength.Short).Show();
			}
			if (mFullUUIDList.Count == 0)
			{
				Toast.MakeText(context,Resource.String.no_adfs_tango_error, Android.Widget.ToastLength.Short).Show();
			}
		}

		public virtual string[] FullUUIDList
		{
			get
			{
				try
				{
					mFullUUIDList = mTango.ListAreaDescriptions();
				}
				catch (TangoErrorException)
				{
					Toast.MakeText(mContext,Resource.String.tango_error, Android.Widget.ToastLength.Short).Show();
				}
                string[] result = new string[mFullUUIDList.Count];
                mFullUUIDList.CopyTo(result,0);
                return result;
			}
		}

		public virtual string[] UUIDNames
		{
			get
			{
				TangoAreaDescriptionMetaData metadata = new TangoAreaDescriptionMetaData();
				string[] list = new string[mFullUUIDList.Count];
				for (int i = 0 ; i < list.Length;i++)
				{
					try
					{
						metadata = mTango.LoadAreaDescriptionMetaData(mFullUUIDList[i]);
					}
					catch (TangoErrorException)
					{
						Toast.MakeText(mContext,Resource.String.tango_error, Android.Widget.ToastLength.Short).Show();
					}
                    var byteResult=metadata.Get("name");
                    list[i] = StringHelperClass.NewString(byteResult);
                     
				}
				return list;
			}
		}

		public virtual void deleteADFandUpdateList(string uuid)
		{
			try
			{
				mTango.DeleteAreaDescription(uuid);
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(mContext,Resource.String.no_uuid_tango_error, Android.Widget.ToastLength.Short).Show();
			}
			mFullUUIDList.Clear();
			try
			{
				mFullUUIDList = mTango.ListAreaDescriptions();
			}
			catch (TangoErrorException)
			{
				Toast.MakeText(mContext,Resource.String.tango_error, Android.Widget.ToastLength.Short).Show();
			}
		}

		public virtual Tango Tango
		{
			get
			{
				return mTango;
			}
		}
	}

}