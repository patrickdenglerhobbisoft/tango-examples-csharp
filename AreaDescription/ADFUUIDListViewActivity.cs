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

namespace com.projecttango.areadescriptioncsharp
{
    
    using Com.Google.Atap.Tangoservice;
    using Tango = Com.Google.Atap.Tangoservice.Tango;
    using TangoAreaDescriptionMetaData = Com.Google.Atap.Tangoservice.TangoAreaDescriptionMetaData;
    using TangoErrorException = Com.Google.Atap.Tangoservice.TangoErrorException;
    using SetNameCommunicator = SetADFNameDialog.SetNameCommunicator;


	using Android.App;

	using Android.Content;
	using  Android.OS;
    using Android.Views;


    using Java.IO;
	using Android.Widget;


	/// <summary>
	/// This class lets you manage ADFs between this class's Application Package
	/// folder and API private space. This show cases mainly three things: Import,
	/// Export, Delete an ADF file from API private space to any known and accessible
	/// file path.
	/// 
	/// </summary>
	public class ADFUUIDListViewActivity : Activity, SetNameCommunicator
	{
		private ADFDataSource mADFDataSource;
		private ListView mUUIDListView, mAppSpaceUUIDListView;
		internal ADFUUIDArrayAdapter mADFAdapter, mAppSpaceADFAdapter;
		internal string[] mUUIDList, mUUIDNames, mAppSpaceUUIDList, mAppSpaceUUIDNames;
		internal string[] mAPISpaceMenuStrings, mAppSpaceMenuStrings;
		internal string mAppSpaceADFFolder;

        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		    SetContentView(Resource.Layout.uuid_listview); 
			mAPISpaceMenuStrings = Resources.GetStringArray(Resource.Array.SetDialogMenuItemsAPISpace);
			mAppSpaceMenuStrings = Resources.GetStringArray(Resource.Array.SetDialogMenuItemsAppSpace);

			// Get API ADF ListView Ready
			mUUIDListView = (ListView) FindViewById(Resource.Id.uuidlistviewAPI);
			mADFDataSource = new ADFDataSource(this);
			mUUIDList = mADFDataSource.FullUUIDList;
			mUUIDNames = mADFDataSource.UUIDNames;
			mADFAdapter = new ADFUUIDArrayAdapter(this, mUUIDList, mUUIDNames);
			mUUIDListView.Adapter = mADFAdapter;
			RegisterForContextMenu(mUUIDListView);

			// Get Apps Space ADF List View Ready
			mAppSpaceUUIDListView = (ListView) FindViewById(Resource.Id.uuidlistviewApplicationSpace);
			mAppSpaceADFFolder = AppSpaceADFFolder;
			mAppSpaceUUIDList = AppSpaceADFList;
			mAppSpaceADFAdapter = new ADFUUIDArrayAdapter(this, mAppSpaceUUIDList, null);
			mAppSpaceUUIDListView.Adapter = mAppSpaceADFAdapter;
			RegisterForContextMenu(mAppSpaceUUIDListView);
		}

        public override void OnCreateContextMenu(IContextMenu menu, View v,IContextMenuContextMenuInfo menuInfo)
        {
            //base.OnCreateContextMenu(menu, v, menuInfo);   TODO: Is this required??
            AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo)menuInfo;
            if (v.Id == Resource.Id.uuidlistviewAPI)
            {
                menu.SetHeaderTitle (mUUIDList[info.Position]);
                menu.Add(mAPISpaceMenuStrings[0]);
                menu.Add(mAPISpaceMenuStrings[1]);
                menu.Add(mAPISpaceMenuStrings[2]);
            }

            if (v.Id == Resource.Id.uuidlistviewApplicationSpace)
            {
                menu.SetHeaderTitle( mAppSpaceUUIDList[info.Position]);
                menu.Add(mAppSpaceMenuStrings[0]);
                menu.Add(mAppSpaceMenuStrings[1]);
            }
        }
        public override bool OnContextItemSelected(IMenuItem item)
		{
			AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo) item.MenuInfo;
			string itemName = (string) item.TitleFormatted.ToString();
			// Rename the ADF from API storage
			if (itemName.Equals(mAPISpaceMenuStrings[0]))
			{
				showSetNameDialog(mUUIDList[info.Position]);
			}
			// Delete the ADF from API storage and update the API ADF Listview
			else if (itemName.Equals(mAPISpaceMenuStrings[1]))
			{
				mADFDataSource.deleteADFandUpdateList(mUUIDList[info.Position]);
				// Update the API ADF Listview
				mUUIDList = mADFDataSource.FullUUIDList;
				mUUIDNames = mADFDataSource.UUIDNames;
				mADFAdapter = new ADFUUIDArrayAdapter(this, mUUIDList, mUUIDNames);
				mUUIDListView.Adapter = mADFAdapter;
			}
			// Export the ADF into application package folder and update the
			// Listview
			else if (itemName.Equals(mAPISpaceMenuStrings[2]))
			{
				try
				{
					mADFDataSource.Tango.ExportAreaDescriptionFile(mUUIDList[info.Position], mAppSpaceADFFolder);
				}
				catch (TangoErrorException)
				{
					Toast.MakeText(this, Resource.String.adf_exists_app_space, Android.Widget.ToastLength.Short).Show();
				}
			}

			// Delete an ADF from App space and update the App space ADF Listview
			else if (itemName.Equals(mAppSpaceMenuStrings[0]))
			{
				File file = new File(mAppSpaceADFFolder + File.Separator + mAppSpaceUUIDList[info.Position]);
				file.Delete();
				// Update App space ADF ListView
				mAppSpaceUUIDList = AppSpaceADFList;
				mAppSpaceADFAdapter = new ADFUUIDArrayAdapter(this, mAppSpaceUUIDList, null);
				mAppSpaceUUIDListView.Adapter = mAppSpaceADFAdapter;
			}

			// Import an ADF into API private Storage and update the API ADF
			// Listview
			else if (itemName.Equals(mAppSpaceMenuStrings[1]))
			{
				try
				{
					mADFDataSource.Tango.ImportAreaDescriptionFile(mAppSpaceADFFolder + File.Separator + mAppSpaceUUIDList[info.Position]);
				}
				catch (TangoErrorException)
				{
					Toast.MakeText(this, Resource.String.adf_exists_api_space, Android.Widget.ToastLength.Short).Show();
				}
			}
			return true;
		}
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        
        {
            // Check which request we're responding to
            if (requestCode == Tango.TangoIntentActivitycode)
            {
                // Make sure the request was successful
                if (resultCode == Result.Canceled)
                {
                    Toast.MakeText(this, Resource.String.no_permissions,  ToastLength.Long).Show();
                }
            }

            // Update App space ADF Listview
            mAppSpaceUUIDList = AppSpaceADFList;
            mAppSpaceADFAdapter = new ADFUUIDArrayAdapter(this, mAppSpaceUUIDList, null);
            mAppSpaceUUIDListView.Adapter = mAppSpaceADFAdapter;

            // Update API ADF Listview
            mUUIDList = mADFDataSource.FullUUIDList;
            mUUIDNames = mADFDataSource.UUIDNames;
            mADFAdapter = new ADFUUIDArrayAdapter(this, mUUIDList, mUUIDNames);
            mUUIDListView.Adapter = mADFAdapter;
        }

		/*
		 * Returns maps storage location in the App package folder. Creates a folder
		 * called Maps, if it doesnt exist.
		 */
		private string AppSpaceADFFolder
		{
			get
			{
				string mapsFolder = FilesDir.AbsolutePath + File.Separator + "Maps";
				File file = new File(mapsFolder);
				if (!file.Exists())
				{
					file.Mkdirs();
				}
				return mapsFolder;
			}
		}

		/*
		 * Returns the names of all ADFs in String array in the files/maps folder.
		 */
		private string[] AppSpaceADFList
		{
			get
			{
				File file = new File(mAppSpaceADFFolder);
				File[] ADFFileList = file.ListFiles();
				string[] appSpaceADFList = new string[ADFFileList.Length];
				for (int i = 0; i < appSpaceADFList.Length; i++)
				{
					appSpaceADFList[i] = ADFFileList[i].Name;
				}
				
				return appSpaceADFList;
			}
		}

		private void showSetNameDialog(string mCurrentUUID)
		{
			Bundle bundle = new Bundle();
			TangoAreaDescriptionMetaData metaData = mADFDataSource.Tango.LoadAreaDescriptionMetaData(mCurrentUUID);
		    byte[] adfNameBytes = metaData.Get("name");
			if (adfNameBytes != null)
			{
				string fillDialogName = StringHelper.NewString(adfNameBytes);
				bundle.PutString("name", fillDialogName);
			}
			bundle.PutString("id", mCurrentUUID);
			FragmentManager manager = FragmentManager;
			SetADFNameDialog SetADFNameDialog = new SetADFNameDialog();
			SetADFNameDialog.Arguments = (bundle);
			SetADFNameDialog.Show(manager, "ADFNameDialog");
		}

		public virtual void SetName(string name, string uuid)
		{
			TangoAreaDescriptionMetaData metadata = new TangoAreaDescriptionMetaData();
			metadata = mADFDataSource.Tango.LoadAreaDescriptionMetaData(uuid);
			var adfNameBytes = metadata.Get("name");
            string comparer = StringHelper.NewString(adfNameBytes);
            if (comparer != name)
			{
				adfNameBytes = (byte[])(System.Array) name.GetBytes();
                metadata.Set("name", (byte[])(System.Array)name.GetBytes());
			}
			mADFDataSource.Tango.SaveAreaDescriptionMetadata(uuid, metadata);
			mUUIDList = mADFDataSource.FullUUIDList;
			mUUIDNames = mADFDataSource.UUIDNames;
			mADFAdapter = new ADFUUIDArrayAdapter(this, mUUIDList, mUUIDNames);
			mUUIDListView.Adapter = mADFAdapter;
		}
	}

	/// <summary>
	/// This is an adapter class which maps the ListView with a Data Source(Array of
	/// strings)
	/// 
	/// </summary>
	internal class ADFUUIDArrayAdapter : Android.Widget.ArrayAdapter<string>
	{
		internal Context mContext;
		private string[] mUUIDStringArray, mUUIDNamesStringArray;

		public ADFUUIDArrayAdapter(Context context, string[] uuids, string[] uuidNames) : base(context, Resource.Layout.uuid_view, Resource.Id.uuid, uuids)
		{
			mContext = context;
			mUUIDStringArray = uuids;
			if (uuidNames != null)
			{
				mUUIDNamesStringArray = uuidNames;
			}
		}
        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {

            LayoutInflater inflator = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
            View row = inflator.Inflate(Resource.Layout.uuid_view, parent, false);
            TextView uuid = (TextView)row.FindViewById(Resource.Id.uuid);
            TextView uuidName = (TextView)row.FindViewById(Resource.Id.adfName);
            uuid.Text = mUUIDStringArray[position];

            if (mUUIDNamesStringArray != null)
            {
                uuidName.Text = mUUIDNamesStringArray[position];
            }
            else
            {
                uuidName.Text = "Meta Data cannot be read";// Resource.String.metadata_not_read;  /// TODO: Fix look up
            }
            return row;
        }
	}
}