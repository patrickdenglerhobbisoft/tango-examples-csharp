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

    using Java.Nio;
    using Java.Util.Concurrent.Atomic;
    using GLES20 = Android.Opengl.GLES20;
    using Matrix = Android.Opengl.Matrix;

    /// <summary>
    /// <seealso cref="Renderable"/> OpenGL showing a PointCloud obtained from Tango XyzIj
    /// data. The point count can vary over as the information is updated.
    /// </summary>
    public class PointCloud : Renderable
	{

		private const int COORDS_PER_VERTEX = 3;

		private const string sVertexShaderCode = "uniform mat4 uMVPMatrix;" + "attribute vec4 vPosition;" + "varying vec4 vColor;" + "void main() {" + "gl_PointSize = 5.0;" + "  gl_Position = uMVPMatrix * vPosition;" + "  vColor = vPosition;" + "}";
		private const string sFragmentShaderCode = "precision mediump float;" + "varying vec4 vColor;" + "void main() {" + "  gl_FragColor = vec4(vColor);" + "}";

		private const int BYTES_PER_FLOAT = 4;
		private const int POINT_TO_XYZ = 3;

		internal int mVertexVBO; // VertexBufferObject.
		private AtomicBoolean mUpdateVBO = new AtomicBoolean();
		private volatile  FloatBuffer mPointCloudBuffer;
        //byte[] bytes = new byte[12];
        //float[] floats = { 1.5f, 2.5f, 0.000001f };

        //Buffer.BlockCopy(floats, 0, bytes, 0, 12);

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

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int buffers[] = new int[1];
			int[] buffers = new int[1];
			GLES20.GlGenBuffers(1, buffers, 0);
			mVertexVBO = buffers[0];
		}

		public virtual void UpdatePoints(FloatBuffer pointCloudFloatBuffer)
		{
			lock (this)
			{
				//save the reference in order to update this in the proper thread.
				mPointCloudBuffer = pointCloudFloatBuffer;
        
				//signal the update
				mUpdateVBO.Set(true);
			}
		}

		public override void draw(float[] viewMatrix, float[] projectionMatrix)
		{
			lock (this)
			{
				GLES20.GlBindBuffer(GLES20.GlArrayBuffer, mVertexVBO);
        
				if (mUpdateVBO.GetAndSet(false))
				{
					if (mPointCloudBuffer != null)
					{
						mPointCloudBuffer.Position(0);
						// Pass the info to the VBO
						GLES20.GlBufferData(GLES20.GlArrayBuffer, mPointCloudBuffer.Capacity() * BYTES_PER_FLOAT, mPointCloudBuffer, GLES20.GlStaticDraw);
						mPointCount = mPointCloudBuffer.Capacity() / 3;
						float totalZ = 0;
						for (int i = 0; i < mPointCloudBuffer.Capacity() - 3; i = i + 3)
						{
							totalZ = totalZ + mPointCloudBuffer.Get(i + 2);
						}
						if (mPointCount != 0)
						{
							mAverageZ = totalZ / mPointCount;
						}
						// signal the update
						mUpdateVBO.Set(true);
					}
					mPointCloudBuffer = null;
				}
        
				if (mPointCount > 0)
				{
        
					GLES20.GlUseProgram(mProgram);
					updateMvpMatrix(viewMatrix, projectionMatrix);
					GLES20.GlVertexAttribPointer(mPosHandle, COORDS_PER_VERTEX, GLES20.GlFloat, false, 0, 0);
					GLES20.GlEnableVertexAttribArray(mPosHandle);
					GLES20.GlUniformMatrix4fv(mMVPMatrixHandle, 1, false, MvpMatrix, 0);
					GLES20.GlDrawArrays(GLES20.GlPoints, 0, mPointCount);
				}
				GLES20.GlBindBuffer(GLES20.GlArrayBuffer, 0);
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