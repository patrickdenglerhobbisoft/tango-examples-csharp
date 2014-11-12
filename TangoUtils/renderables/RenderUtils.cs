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
    using Com.Google.Atap.Tangoservice;
	using GLES20 = Android.Opengl.GLES20;

	/// <summary>
	/// Static functions used by Renderer classes in Tango Java samples.
	/// </summary>
	public class RenderUtils
	{

		/// <summary>
		/// Creates a vertex or fragment shader.
		/// </summary>
		/// <param name="type">
		///            One of GLES20.GlVertexShader or GLES20.GlFragmentShader </param>
		/// <param name="shaderCode">
		///            GLSL code for the shader as a String </param>
		/// <returns> a compiled shader. </returns>
		public static int loadShader(int type, string shaderCode)
		{
			// Create a shader of the correct type
			int shader = GLES20.GlCreateShader(type);

			// Compile the shader from source code
			GLES20.GlShaderSource(shader, shaderCode);
			GLES20.GlCompileShader(shader);

			return shader;
		}

	}

}