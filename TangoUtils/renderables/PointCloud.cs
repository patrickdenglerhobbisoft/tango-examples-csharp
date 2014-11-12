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

namespace com.projecttango.tangoutils.renderables
{
    using Java.Nio;
    using Com.Google.Atap.Tangoservice;
	using GLES20 = Android.Opengl.GLES20;
    using Matrix = Android.Opengl.Matrix;
    using System.Collections;
	/// <summary>
	/// <seealso cref="Renderable"/> openGl showing a PointCloud obtained from Tango XyzIj
	/// data. The point count can vary over as the information is updated.
	/// </summary>
	public class PointCloud : Renderable
	{

		private const int COORDS_PER_VERTEX = 3;

		private const string sVertexShaderCode = "uniform mat4 uMVPMatrix;" + "attribute vec4 vPosition;" + "varying vec4 vColor;" + "void main() {" + "gl_PointSize = 5.0;" + "  gl_Position = uMVPMatrix * vPosition;" + "  vColor = vPosition;" + "}";
		private const string sFragmentShaderCode = "precision mediump float;" + "varying vec4 vColor;" + "void main() {" + "  gl_FragColor = vec4(vColor);" + "}";

		private const int BYTES_PER_FLOAT = 4;
		private const int POINT_TO_XYZ = 3;
		private FloatBuffer mVertexBuffer;
		private readonly int mProgram;
		private int mPosHandle;
		private int mMVPMatrixHandle;
		private int mPointCount;
		private float mAverageZ;

		public PointCloud(int maxDepthPoints)
		{
			mAverageZ = 0;
			int vertexShader = RenderUtils.loadShader(GLES20.GlVertexShader, sVertexShaderCode);
			int fragShader = RenderUtils.loadShader(GLES20.GlFragmentShader, sFragmentShaderCode);
			mProgram = GLES20.GlCreateProgram();
			GLES20.GlAttachShader(mProgram, vertexShader);
			GLES20.GlAttachShader(mProgram, fragShader);
			GLES20.GlLinkProgram(mProgram);
			Matrix.SetIdentityM(ModelMatrix, 0);
			mVertexBuffer = ByteBuffer.AllocateDirect(maxDepthPoints * BYTES_PER_FLOAT * POINT_TO_XYZ).Order(ByteOrder.NativeOrder()).AsFloatBuffer();
		}

		public virtual void UpdatePoints(byte[] byteArray, int pointCount)
		{
			lock (this)
			{
				FloatBuffer mPointCloudFloatBuffer;
				mPointCloudFloatBuffer = ByteBuffer.Wrap( byteArray).Order(ByteOrder.NativeOrder()).AsFloatBuffer();
				mPointCount = pointCount;
				mVertexBuffer.Clear();
				mVertexBuffer.Position(0);
				mVertexBuffer.Put(mPointCloudFloatBuffer);
				float totalZ = 0;
				for (int i = 0; i < mPointCloudFloatBuffer.Capacity() - 3; i = i + 3)
				{
					totalZ = totalZ + mPointCloudFloatBuffer.Get(i + 2);
				}
				mAverageZ = totalZ / mPointCount;
			}
		}

		public override void draw(float[] viewMatrix, float[] projectionMatrix)
		{
			lock (this)
			{
				if (mPointCount > 0)
				{
					mVertexBuffer.Position(0);
					GLES20.GlUseProgram(mProgram);
					updateMvpMatrix(viewMatrix, projectionMatrix);
					mPosHandle = GLES20.GlGetAttribLocation(mProgram, "vPosition");
					GLES20.GlVertexAttribPointer(mPosHandle, COORDS_PER_VERTEX, GLES20.GlFloat, false, 0, mVertexBuffer);
					GLES20.GlEnableVertexAttribArray(mPosHandle);
					mMVPMatrixHandle = GLES20.GlGetUniformLocation(mProgram, "uMVPMatrix");
					GLES20.GlUniformMatrix4fv(mMVPMatrixHandle, 1, false, MvpMatrix, 0);
					GLES20.GlDrawArrays(GLES20.GlPoints, 0, mPointCount);
				}
			}
		}

		public virtual float AverageZ
		{
			get
			{
				return mAverageZ;
			}
		}

		public virtual int PointCount
		{
			get
			{
				return mPointCount;
			}
		}
	}

}