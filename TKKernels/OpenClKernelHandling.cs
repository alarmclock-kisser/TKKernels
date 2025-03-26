using OpenTK.Compute.OpenCL;
using System.Text;

namespace TKKernels
{
	public class OpenClKernelHandling
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		private OpenClContextHandling ContextH;


		public CLKernel? Current = null;
		public List<CLKernel> Kernels = [];
		public string? CurrentString = null;

		public Type? CurrentInputType => GetInputType();




		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		private string Repopath => ContextH.Repopath;
		private ListBox LogBox => ContextH.LogBox;


		private CLContext? Ctx => ContextH.Ctx;
		private CLDevice? Dev => ContextH.Dev;

		private OpenClMemoryHandling? MemH => ContextH.MemH ?? null;
		private CLCommandQueue? Que => ContextH.MemH?.Que ?? null;



		public string[] KernelPaths => GetKernelPaths();
		public Type[] CurrentParameters => GetParameterTypes();



		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public OpenClKernelHandling(OpenClContextHandling openClContextHandling)
		{
			// Set attributes
			this.ContextH = openClContextHandling;

			
		}






		// ----- ----- ----- METHODS ----- ----- ----- \\
		// Log
		public void Log(string message, string inner = "", int layer = 1, bool update = false)
		{
			string msg = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] ";
			msg += "<Kernel>";
			for (int i = 0; i <= layer; i++)
			{
				msg += " - ";
			}
			msg += message;
			if (inner != "")
			{
				msg += "  (" + inner + ")";
			}
			if (update)
			{
				LogBox.Items[^1] = msg;
			}
			else
			{
				LogBox.Items.Add(msg);
				LogBox.SelectedIndex = LogBox.Items.Count - 1;
			}
		}


		// Dispose
		public void Dispose()
		{
			// Dispose kernels
		}



		// Load & Info
		public string? ReadKernelFile(string filePath)
		{
			// Try read file
			string? kernelString = null;
			try
			{
				kernelString = File.ReadAllText(filePath);
			}
			catch (Exception e)
			{
				Log("Error reading kernel file", e.Message);
			}

			// Return
			return kernelString;
		}

		public string? PrecompileKernelString(string? kernelString)
		{
			// Abort if kernelString null
			if (kernelString == null)
			{
				Log("Error precompiling kernel string", "Kernel string is null");
				return null;
			}

			// Read file if path
			if (File.Exists(kernelString))
			{
				kernelString = ReadKernelFile(kernelString) ?? "";
			}

			string? kernelName = null;

			// Check if contains "__kernel " and " void "
			if (!kernelString.Contains("__kernel ") || !kernelString.Contains(" void "))
			{
				Log("Error precompiling kernel string", "Kernel string does not contain '__kernel ' and ' void '");
				return null;
			}

			// Check every bracked paired ()
			int openCount = kernelString.Count(c => c == '(');
			int closeCount = kernelString.Count(c => c == ')');
			foreach (char c in kernelString)
			if (closeCount != openCount || closeCount == 0)
			{
				Log("Error precompiling kernel string: ()-brackets not paired or 0", openCount + " open, " + closeCount + " close");
				return null;
			}

			// Check every bracked paired []
			openCount = kernelString.Count(c => c == '[');
			closeCount = kernelString.Count(c => c == ']');
			if (closeCount != openCount || closeCount == 0)
			{
				Log("Error precompiling kernel string: []-brackets not paired or 0", openCount + " open, " + closeCount + " close");
				return null;
			}


			// Check brackets paired {}
			openCount = kernelString.Count(c => c == '{');
			closeCount = kernelString.Count(c => c == '}');
			if (closeCount != openCount || closeCount == 0)
			{
				Log("Error precompiling kernel string: {}-brackets not paired or 0", openCount + " open, " + closeCount + " close");
				return null;
			}

			// Check if contains " int " (mandatory for input array length)
			if (!kernelString.Contains(" int "))
			{
				Log("Error precompiling kernel string", "Kernel string does not contain ' int '");
				return null;
			}

			// Get kernel name (start after " void ", end before "(")
			int start = kernelString.IndexOf(" void ") + 6;
			int end = kernelString.IndexOf("(", start);
			kernelName = kernelString.Substring(start, end - start).Trim();
			if (string.IsNullOrEmpty(kernelName))
			{
				Log("Error precompiling kernel string", "Kernel name not found");
				return null;
			}

			// Return
			return kernelName;
		}

		public string[] GetKernelPaths()
		{
			// Get all files in repopath
			string[] files = Directory.GetFiles(Path.Combine(Repopath, "Kernels"), "*.k");

			// Return
			return files;
		}

		public Type[] GetParameterTypes()
		{
			// Abort if no current kernel
			if (Current == null)
			{
				Log("Error getting parameter types", "No current kernel");
				return [];
			}

			// Get lines
			string[] lines = CurrentString?.Split('\n') ?? [];

			// Get parameters between "(" and ")", separated by ","
			string parameters = lines.FirstOrDefault(x => x.Contains("(")) ?? "";
			parameters = parameters.Substring(parameters.IndexOf("(") + 1);
			parameters = parameters.Substring(0, parameters.IndexOf(")"));
			string[] paramArray = parameters.Split(',').Select(x => x.Split("=").First().Trim()).Where(x => !x.Contains("*")).ToArray();

			// Get types
			List<Type> types = [];
			foreach (string param in paramArray)
			{
				// Get type
				string typeString = param.Split(' ').First().Trim();
				Type type;
				switch (typeString)
				{
					case "int":
						type = typeof(int);
						break;
					case "float":
						type = typeof(float);
						break;
					case "double":
						type = typeof(double);
						break;
					case "char":
						type = typeof(char);
						break;
					case "byte":
						type = typeof(byte);
						break;
					case "short":
						type = typeof(short);
						break;
					case "long":
						type = typeof(long);
						break;
					case "bool":
						type = typeof(bool);
						break;
					default:
						Log("Error getting parameter types", "Unknown type: " + typeString);
						type = typeof(object);
						break;
				}

				// Add type
				if (type != null)
				{
					types.Add(type);
				}
			}

			// Return
			return types.ToArray();
		}

		public Type GetInputType()
		{
			// STUB
			return typeof(int);
		}

		public CLKernel? GetKernel(string kernelName)
		{
			// Get kernel
			CLKernel? kernel = Kernels.FirstOrDefault(k => GetKernelName(k) == kernelName);
			
			// Return
			return kernel;
		}

		public string GetKernelName(CLKernel kernel)
		{
			// Get kernel info
			CLResultCode err = CL.GetKernelInfo(kernel, KernelInfo.FunctionName, out byte[]? name);
			if (err != CLResultCode.Success || name == null || name.Length == 0)
			{
				Log("Error getting kernel info KernelName", err.ToString());
			}

			// Return
			return Encoding.UTF8.GetString(name ?? []);
		}

		public int GetKernelParamCount(CLKernel kernel)
		{
			// Get kernel info
			CLResultCode err = CL.GetKernelInfo(kernel, KernelInfo.NumberOfArguments, out byte[]? count);
			if (err != CLResultCode.Success || count == null || count.Length == 0)
			{
				Log("Error getting kernel info ParamCount", err.ToString());
				return 0;
			}

			// Return
			return BitConverter.ToInt32(count ?? [], 0);
		}

		public int GetKernelRefCount(CLKernel kernel)
		{
			// Get kernel info
			CLResultCode err = CL.GetKernelInfo(kernel, KernelInfo.ReferenceCount, out byte[]? count);
			if (err != CLResultCode.Success || count == null)
			{
				Log("Error getting kernel info RefCount", err.ToString());
				return 0;
			}

			// Return
			return BitConverter.ToInt32(count ?? [], 0);
		}

		public string[] LogEveryKernel()
		{
			// Log every kernel file (.k) found at Resources/Kernels
			List<string> entries = [];
			foreach (string file in KernelPaths)
			{
				Log("Found kernel file: '" + Path.GetFileName(file) + "'");
				entries.Add("Found kernel file: '" + Path.GetFileName(file) + "'");
			}

			// Log every compiled kernel (Kernels) info
			CLKernel[] kernels = Kernels.ToArray();
			foreach (CLKernel kernel in kernels)
			{
				string name = GetKernelName(kernel);
				int paramCount = GetKernelParamCount(kernel);
				int refCount = GetKernelRefCount(kernel);
				Log("Compiled kernel: '" + name + "'", "Parameters: " + paramCount + ", References: " + refCount);
				string entry = "Compiled kernel: '" + name + "', Parameters: " + paramCount + ", References: " + refCount;
				entries.Add(entry);
			}

			// Log total found / compiled
			Log("Total kernels compiled: " + Kernels.Count, KernelPaths.Length + " files found");


			// Return
			return entries.ToArray();
		}



		// Compile
		public CLKernel? CompileKernel(string? kernelString)
		{
			string? kernelName;

			// Read file if path
			if (File.Exists(kernelString) && Path.GetExtension(kernelString) == ".k")
			{
				kernelString = ReadKernelFile(kernelString);

				// Warn kernelString was file path
				Log("Kernel string was a file path", Path.GetFileName(kernelString ?? ""));
			}

			// Abort if kernelString null
			if (kernelString == null || kernelString == "" || kernelString.Length == 0)
			{
				Log("Error reading kernel file", "KernelString was null or empty");
				return null;
			}

			// Abort if no Ctx
			if (Ctx == null || Dev == null)
			{
				Log("No context to compile kernel");
				return null;
			}

			// Precompile -> Get kernel name
			kernelName = PrecompileKernelString(kernelString);
			if (kernelName == null)
			{
				Log("Error precompiling kernel string");
				return null;
			}

			// Try compile kernel
			CLKernel? kernel = null;
			
			// Get program
			CLProgram? program = CL.CreateProgramWithSource(Ctx.Value, kernelString, out CLResultCode err);
			if (err != CLResultCode.Success || program == null)
			{
				Log("Error creating program (1)", err.ToString());
				return null;
			}

			// Create callback
			CL.ClEventCallback callback = (ev, evStatus) =>
			{
				Log("Callback", "Event status: " + evStatus.ToString());
			};

			// Build program
			err = CL.BuildProgram(program.Value, [Dev.Value], "",callback);
			if (err != CLResultCode.Success)
			{
				Log("Error building program (2)", err.ToString());
				return null;
			}

			// Build info options (CLOptions)
			err = CL.GetProgramBuildInfo(program.Value, Dev.Value, ProgramBuildInfo.Options, out byte[]? buildOptions);
			if (err != CLResultCode.Success || buildOptions == null)
			{
				Log("Error getting program build info (3.1)", err.ToString());
				return null;
			}
			if (buildOptions.Length > 0)
			{
				Log("Program build options", Encoding.UTF8.GetString(buildOptions));
			}
			else
			{
				Log("No build options available");
			}

			// Build info status
			err = CL.GetProgramBuildInfo(program.Value, Dev.Value, ProgramBuildInfo.Status, out byte[]? buildStatus);
			if (err != CLResultCode.Success || buildStatus == null)
			{
				Log("Error getting program build info (3.2)", err.ToString());
				return null;
			}
			if (buildStatus.Length > 0)
			{
				Log("Program build status", BitConverter.ToInt32(buildStatus, 0).ToString());
			}
			else
			{
				Log("No build status available");
			}

			// Build info log
			err = CL.GetProgramBuildInfo(program.Value, Dev.Value, ProgramBuildInfo.Log, out byte[]? log);
			if (err != CLResultCode.Success || log == null)
			{
				Log("Error getting program build info (3.3)", err.ToString());
				return null;
			}
			if (log.Length > 0)
			{
				Log("Program build log", Encoding.UTF8.GetString(log));
			}
			else
			{
				Log("No build log available");
			}

			// Build kernel
			kernel = CL.CreateKernel(program.Value, kernelName, out err);
			if (err != CLResultCode.Success || kernel == null)
			{
				Log("Error creating kernel (4)", err.ToString());
				return null;
			}

			// Return
			return kernel;
		}

		public List<CLKernel> CompileAllPaths()
		{
			List<CLKernel> kernels = [];

			// For every kernel path: Compile
			foreach (string path in KernelPaths)
			{
				string? kernelString = ReadKernelFile(path);
				CLKernel? kernel = CompileKernel(kernelString);
				if (kernel != null)
				{
					kernels.Add(kernel.Value);
				}
			}

			// Return
			return kernels;
		}

		public void ReCompileAll(bool log = false)
		{
			// Set Kernels & Log
			if (Ctx != null && Dev != null)
			{
				Kernels = CompileAllPaths();
				if (log)
				{
					LogEveryKernel();
				}
			}
		}




		// Execute kernel (no extra parameters)
		public void ExecuteKernelWithoutParams(CLKernel kernel, long ptr)
		{
			// Abort if no Que or CTx
			if (Que == null || Ctx == null || MemH == null)
			{
				Log("Error executing kernel", "No command queue or context");
				return;
			}

			// Get buffers & sizes
			CLBuffer[] buffers = MemH.FindBuffers(ptr);
			if (buffers.Length == 0)
			{
				Log("Error executing kernel", "No buffers found");
				return;
			}
			nuint[] sizes = MemH.FindLengths(ptr).Select(s => (nuint) s).ToArray();

			// Execute kernel with every buffer as argument with its length
			for (int i = 0; i < buffers.Length; i++)
			{
				// Set input buffer
				CLResultCode err = CL.SetKernelArg(kernel, 0, buffers[i]);
				if (err != CLResultCode.Success)
				{
					Log("Error setting kernel argument: pointer array", err.ToString());
					return;
				}

				// Set length
				err = CL.SetKernelArg(kernel, 1, sizes[i]);
				if (err != CLResultCode.Success)
				{
					Log("Error setting kernel argument: int length", err.ToString());
					return;
				}

				// Execute
				err = CL.EnqueueNDRangeKernel(Que.Value, kernel, 1, null, [sizes[i]], null, 0, null, out CLEvent evt);
				if (err != CLResultCode.Success)
				{
					Log("Error executing kernel", err.ToString());
					return;
				}

				// Wait for event
				err = CL.WaitForEvents(1, [evt]);
				if (err != CLResultCode.Success)
				{
					Log("Error waiting for event", err.ToString());
					return;
				}

				// Release event
				err = CL.ReleaseEvent(evt);
				if (err != CLResultCode.Success)
				{
					Log("Error releasing event", err.ToString());
					return;
				}
			}
		}


	}
}