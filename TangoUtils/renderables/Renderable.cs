/*
 * Copyright 2014 HobbiSoft. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.Apache.Org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace com.projecttango.tangoutils.renderables
{

	using Matrix = Android.Opengl.Matrix;

	/// <summary>
	/// Base class for all self-drawing OpenGL objects used in Tango Java examples.
	/// Contains common logic for handling the MVP matrices.
	/// </summary>
	public abstract class Renderable
	{

		private float[] mModelMatrix = new float[16];
		private float[] mMvMatrix = new float[16];
		private float[] mMvpMatrix = new float[16];

		/// <summary>
		/// Applies the view and projection matrices and draws the Renderable.
		/// </summary>
		/// <param name="viewMatrix">
		///            the view matrix to map from world space to camera space. </param>
		/// <param name="projectionMatrix">
		///            the projection matrix to map from camera space to screen
		///            space. </param>
		public abstract void draw(float[] viewMatrix, float[] projectionMatrix);

		public virtual void updateMvpMatrix(float[] viewMatrix, float[] projectionMatrix)
		{
			lock (this)
			{
				// Compose the model, view, and projection matrices into a single mvp
				// matrix
				Matrix.SetIdentityM(mMvMatrix, 0);
				Matrix.SetIdentityM(mMvpMatrix, 0);
				Matrix.MultiplyMM(mMvMatrix, 0, viewMatrix, 0, mModelMatrix, 0);
				Matrix.MultiplyMM(mMvpMatrix, 0, projectionMatrix, 0, mMvMatrix, 0);
			}
		}

		public virtual float[] ModelMatrix
		{
			get
			{
				return mModelMatrix;
			}
			set
			{
				mModelMatrix = value;
			}
		}


		public virtual float[] MvMatrix
		{
			get
			{
				return mMvMatrix;
			}
		}

		public virtual float[] MvpMatrix
		{
			get
			{
				return mMvpMatrix;
			}
		}
	}

}