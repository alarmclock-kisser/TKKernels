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

		public Type? CurrentInputType = null;




		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		private string Repopath => this.ContextH.Repopath;
		private ListBox LogBox => this.ContextH.LogBox;


		private CLContext? Ctx => this.ContextH.Ctx;
		private CLDevice? Dev => this.ContextH.Dev;

		private OpenClMemoryHandling? MemH => this.ContextH.MemH ?? null;
		private CLCommandQueue? Que => this.ContextH.MemH?.Que ?? null;



		public string[] KernelPaths => this.GetKernelPaths();
		public Type[] CurrentParameters => this.GetParameterTypes();



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
				this.LogBox.Items[^1] = msg;
			}
			else
			{
				this.LogBox.Items.Add(msg);
				this.LogBox.SelectedIndex = this.LogBox.Items.Count - 1;
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
			// If file does not exist: Try find path in Repopath\Kernels
			if (!File.Exists(filePath))
			{
				filePath = Path.Combine(this.Repopath, "Kernels", Path.GetFileName(filePath));
				if (!File.Exists(filePath))
				{
					this.Log("Error reading kernel file", "File not found: " + filePath);
					return null;
				}
			}

			// Try read file
			string? kernelString = null;
			try
			{
				kernelString = File.ReadAllText(filePath);
			}
			catch (Exception e)
			{
				this.Log("Error reading kernel file", e.Message);
			}

			// Return
			return kernelString;
		}

		public string? TryFindKernelPath(string kernelName)
		{
			// Get all .k files in Repopath
			string[] files = Directory.GetFiles(Path.Combine(this.Repopath, "Kernels"), "*.k");
			if (files.Length == 0)
			{
				this.Log("Error finding kernel path", "No .k files found in Repopath\\Kernels");
				return null;
			}
			else
			{
				this.Log("Found kernel files", files.Length + " files found");
			}

			// Find file with contains kernelName, compare by ToLower
			string? filePath = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).ToLower().Replace("kernel", "") == kernelName.ToLower().Replace("kernel", ""));
			if (filePath == null)
			{
				this.Log("Error finding kernel path", "No file found with kernelName: " + kernelName);
				return null;
			}

			// Return
			return filePath;
		}

		public string? PrecompileKernelString(string? kernelString)
		{
			// Abort if kernelString null
			if (kernelString == null)
			{
				this.Log("Error precompiling kernel string", "Kernel string is null");
				return null;
			}

			// Read file if path
			if (File.Exists(kernelString))
			{
				kernelString = this.ReadKernelFile(kernelString) ?? "";
			}

			string? kernelName = null;

			// Check if contains "__kernel " and " void "
			if (!kernelString.Contains("__kernel ") || !kernelString.Contains(" void "))
			{
				this.Log("Error precompiling kernel string", "Kernel string does not contain '__kernel ' and ' void '");
				return null;
			}

			// Check every bracked paired ()
			int openCount = kernelString.Count(c => c == '(');
			int closeCount = kernelString.Count(c => c == ')');
			foreach (char c in kernelString)
			if (closeCount != openCount || closeCount == 0)
			{
					this.Log("Error precompiling kernel string: ()-brackets not paired or 0", openCount + " open, " + closeCount + " close");
				return null;
			}

			// Check every bracked paired []
			openCount = kernelString.Count(c => c == '[');
			closeCount = kernelString.Count(c => c == ']');
			if (closeCount != openCount || closeCount == 0)
			{
				this.Log("Error precompiling kernel string: []-brackets not paired or 0", openCount + " open, " + closeCount + " close");
				return null;
			}


			// Check brackets paired {}
			openCount = kernelString.Count(c => c == '{');
			closeCount = kernelString.Count(c => c == '}');
			if (closeCount != openCount || closeCount == 0)
			{
				this.Log("Error precompiling kernel string: {}-brackets not paired or 0", openCount + " open, " + closeCount + " close");
				return null;
			}

			// Check if contains " int " (mandatory for input array length)
			if (!kernelString.Contains(" int "))
			{
				this.Log("Error precompiling kernel string", "Kernel string does not contain ' int '");
				return null;
			}

			// Get kernel name (start after " void ", end before "(")
			int start = kernelString.IndexOf(" void ") + 6;
			int end = kernelString.IndexOf("(", start);
			kernelName = kernelString.Substring(start, end - start).Trim();
			if (string.IsNullOrEmpty(kernelName))
			{
				this.Log("Error precompiling kernel string", "Kernel name not found");
				return null;
			}

			// Return
			return kernelName;
		}

		public string[] GetKernelPaths()
		{
			// Get all files in repopath
			string[] files = Directory.GetFiles(Path.Combine(this.Repopath, "Kernels"), "*.k");

			// Return
			return files;
		}

		public Type[] GetParameterTypes()
		{
			// Abort if no current kernel
			if (this.Current == null)
			{
				this.Log("Error getting parameter types", "No current kernel");
				return [];
			}

			// Get lines
			string[] lines = this.CurrentString?.Split('\n') ?? [];

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
						this.Log("Error getting parameter types", "Unknown type: " + typeString);
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

		public CLKernel? GetKernel(string kernelName)
		{
			// Get kernel
			CLKernel? kernel = this.Kernels.FirstOrDefault(k => this.GetKernelName(k) == kernelName);
			
			// Return
			return kernel;
		}

		public string? GetKernelName(CLKernel? kernel)
		{
			kernel ??= this.Current;
			if (kernel == null)
			{
				this.Log("Error getting kernel name", "Kernel is null");
				return null;
			}

			// Get kernel info
			CLResultCode err = CL.GetKernelInfo(kernel.Value, KernelInfo.FunctionName, out byte[]? name);
			if (err != CLResultCode.Success || name == null || name.Length == 0)
			{
				this.Log("Error getting kernel info KernelName", err.ToString());
			}

			// Return
			return Encoding.UTF8.GetString(name ?? []).Trim('\0');
		}

		public int GetKernelParamCount(CLKernel? kernel)
		{
			kernel ??= this.Current;
			if (kernel == null)
			{
				this.Log("Error getting kernel param count", "Kernel is null");
				return 0;
			}

			// Get kernel info
			CLResultCode err = CL.GetKernelInfo(kernel.Value, KernelInfo.NumberOfArguments, out byte[]? count);
			if (err != CLResultCode.Success || count == null || count.Length == 0)
			{
				this.Log("Error getting kernel info ParamCount", err.ToString());
				return 0;
			}

			// Return
			return BitConverter.ToInt32(count ?? [], 0);
		}

		public int GetKernelRefCount(CLKernel? kernel)
		{
			kernel ??= this.Current;
			if (kernel == null)
			{
				this.Log("Error getting kernel ref count", "Kernel is null");
				return 0;
			}

			// Get kernel info
			CLResultCode err = CL.GetKernelInfo(kernel.Value	, KernelInfo.ReferenceCount, out byte[]? count);
			if (err != CLResultCode.Success || count == null)
			{
				this.Log("Error getting kernel info RefCount", err.ToString());
				return 0;
			}

			// Return
			return BitConverter.ToInt32(count ?? [], 0);
		}

		public string[] LogEveryKernel()
		{
			// Log every kernel file (.k) found at Resources/Kernels
			List<string> entries = [];
			foreach (string file in this.KernelPaths)
			{
				this.Log("Found kernel file: '" + Path.GetFileName(file) + "'");
				entries.Add("Found kernel file: '" + Path.GetFileName(file) + "'");
			}

			// Log every compiled kernel (Kernels) info
			CLKernel[] kernels = this.Kernels.ToArray();
			foreach (CLKernel kernel in kernels)
			{
				string name = this.GetKernelName(kernel);
				int paramCount = this.GetKernelParamCount(kernel);
				int refCount = this.GetKernelRefCount(kernel);
				this.Log("Compiled kernel: '" + name + "'", "Parameters: " + paramCount + ", References: " + refCount);
				string entry = "Compiled kernel: '" + name + "', Parameters: " + paramCount + ", References: " + refCount;
				entries.Add(entry);
			}

			// Log total found / compiled
			this.Log("Total kernels compiled: " + this.Kernels.Count, this.KernelPaths.Length + " files found");


			// Return
			return entries.ToArray();
		}

		public string[] GetParamsString()
		{
			// Read kernel string
			string[] lines = this.CurrentString?.Split('\n') ?? [];

			// Search line with " void " and "(", read until ")"
			string parameters = lines.FirstOrDefault(x => x.Contains(" void ") && x.Contains('(') && x.Contains(')')) ?? "";
			if (string.IsNullOrEmpty(parameters))
			{
				// Abort if no parameters found
				return [];
			}

			// Get parameters between "(" and ")", separated by ","
			parameters = parameters.Substring(parameters.IndexOf("(") + 1);
			parameters = parameters.Substring(0, parameters.IndexOf(")"));

			// Get array paramStrings by splitting by "," and replacing " " with ":"
			string[] paramStrings = parameters.Split(',').Select(x => x.Trim().Replace(" ", ":")).ToArray();

			// Find array parameter -> Set input type
			string? arrayParamString = paramStrings.FirstOrDefault(x => x.Contains("*"));
			if (!string.IsNullOrEmpty(arrayParamString))
			{
				this.SetInputType(arrayParamString);
			}

			return paramStrings;
		}

		private void SetInputType(String arrayParamString)
		{
			// Get first word
			string? typeString = arrayParamString.Split(':').FirstOrDefault();
			if (string.IsNullOrEmpty(typeString))
			{
				this.Log("Error setting input type", "Type string is null or empty, or no separator ':' found");
				this.CurrentInputType = null;
				return;
			}

			// Set input type (switch)
			switch (typeString.ToLower())
			{
				case "int":
					this.CurrentInputType = typeof(int);
					break;
				case "float":
					this.CurrentInputType = typeof(float);
					break;
				case "double":
					this.CurrentInputType = typeof(double);
					break;
				case "char":
					this.CurrentInputType = typeof(char);
					break;
				case "byte":
					this.CurrentInputType = typeof(byte);
					break;
				case "short":
					this.CurrentInputType = typeof(short);
					break;
				case "long":
					this.CurrentInputType = typeof(long);
					break;
				case "bool":
					this.CurrentInputType = typeof(bool);
					break;
				default:
					this.Log("Error setting input type", "Unknown type: " + typeString);
					this.CurrentInputType = null;
					break;
			}

			// Log
			if (this.CurrentInputType != null)
			{
				this.Log("Set input type", this.CurrentInputType.Name);
			}
		}



		// Compile
		public CLKernel? CompileKernel(string? kernelString)
		{
			string? kernelName;

			// Read file if path
			if (File.Exists(kernelString) && Path.GetExtension(kernelString) == ".k")
			{
				kernelString = this.ReadKernelFile(kernelString);

				// Warn kernelString was file path
				this.Log("Kernel string was a file path", Path.GetFileName(kernelString ?? ""));
			}

			// Abort if kernelString null
			if (kernelString == null || kernelString == "" || kernelString.Length == 0)
			{
				this.Log("Error reading kernel file", "KernelString was null or empty");
				return null;
			}

			// Abort if no Ctx
			if (this.Ctx == null || this.Dev == null)
			{
				this.Log("No context to compile kernel");
				return null;
			}

			// Precompile -> Get kernel name
			kernelName = this.PrecompileKernelString(kernelString);
			if (kernelName == null)
			{
				this.Log("Error precompiling kernel string");
				return null;
			}

			// Try compile kernel
			CLKernel? kernel = null;
			
			// Get program
			CLProgram? program = CL.CreateProgramWithSource(this.Ctx.Value, kernelString, out CLResultCode err);
			if (err != CLResultCode.Success || program == null)
			{
				this.Log("Error creating program (1)", err.ToString());
				return null;
			}

			// Create callback
			CL.ClEventCallback callback = (ev, evStatus) =>
			{
				this.Log("Callback", "Event status: " + evStatus.ToString());
			};

			// Build program
			err = CL.BuildProgram(program.Value, [this.Dev.Value], "",callback);
			if (err != CLResultCode.Success)
			{
				this.Log("Error building program (2)", err.ToString());
				return null;
			}

			// Build info options (CLOptions)
			err = CL.GetProgramBuildInfo(program.Value, this.Dev.Value, ProgramBuildInfo.Options, out byte[]? buildOptions);
			if (err != CLResultCode.Success || buildOptions == null)
			{
				this.Log("Error getting program build info (3.1)", err.ToString());
				return null;
			}
			if (buildOptions.Length > 0)
			{
				this.Log("Program build options", Encoding.UTF8.GetString(buildOptions));
			}
			else
			{
				this.Log("No build options available");
			}

			// Build info status
			err = CL.GetProgramBuildInfo(program.Value, this.Dev.Value, ProgramBuildInfo.Status, out byte[]? buildStatus);
			if (err != CLResultCode.Success || buildStatus == null)
			{
				this.Log("Error getting program build info (3.2)", err.ToString());
				return null;
			}
			if (buildStatus.Length > 0)
			{
				this.Log("Program build status", BitConverter.ToInt32(buildStatus, 0).ToString());
			}
			else
			{
				this.Log("No build status available");
			}

			// Build info log
			err = CL.GetProgramBuildInfo(program.Value, this.Dev.Value, ProgramBuildInfo.Log, out byte[]? log);
			if (err != CLResultCode.Success || log == null)
			{
				this.Log("Error getting program build info (3.3)", err.ToString());
				return null;
			}
			if (log.Length > 0)
			{
				this.Log("Program build log", Encoding.UTF8.GetString(log));
			}
			else
			{
				this.Log("No build log available");
			}

			// Build kernel
			kernel = CL.CreateKernel(program.Value, kernelName, out err);
			if (err != CLResultCode.Success || kernel == null)
			{
				this.Log("Error creating kernel (4)", err.ToString());
				return null;
			}

			// Return
			return kernel;
		}

		public List<CLKernel> CompileAllPaths()
		{
			List<CLKernel> kernels = [];

			// For every kernel path: Compile
			foreach (string path in this.KernelPaths)
			{
				string? kernelString = this.ReadKernelFile(path);
				CLKernel? kernel = this.CompileKernel(kernelString);
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
			if (this.Ctx != null && this.Dev != null)
			{
				this.Kernels = this.CompileAllPaths();
				if (log)
				{
					this.LogEveryKernel();
				}
			}
		}



		// Load
		public void LoadKernel(string kernelName)
		{
			// Set current kernel
			this.Current = this.GetKernel(kernelName);
			if (this.Current == null)
			{
				this.Log("Error loading kernel", "Kernel not found: " + kernelName);
				return;
			}

			// Set current string
			this.CurrentString = this.TryFindKernelPath(kernelName);
			if (this.CurrentString == null)
			{
				this.Log("Error loading kernel", "Kernel string not found: " + kernelName);
				return;
			}

			// Log
			this.Log("Loaded kernel", kernelName);
		}



		// Execute kernel (no extra parameters)
		public void ExecuteKernelWithoutParams(CLKernel kernel, long ptr)
		{
			// Abort if no Que or CTx
			if (this.Que == null || this.Ctx == null || this.MemH == null)
			{
				this.Log("Error executing kernel", "No command queue or context");
				return;
			}

			// Get buffers & sizes
			CLBuffer[] buffers = this.MemH.FindBuffers(ptr);
			if (buffers.Length == 0)
			{
				this.Log("Error executing kernel", "No buffers found");
				return;
			}
			nuint[] sizes = this.MemH.FindLengths(ptr).Select(s => (nuint) s).ToArray();

			// Execute kernel with every buffer as argument with its length
			for (int i = 0; i < buffers.Length; i++)
			{
				// Set input buffer
				CLResultCode err = CL.SetKernelArg(kernel, 0, buffers[i]);
				if (err != CLResultCode.Success)
				{
					this.Log("Error setting kernel argument: pointer array", err.ToString());
					return;
				}

				// Set length
				err = CL.SetKernelArg(kernel, 1, sizes[i]);
				if (err != CLResultCode.Success)
				{
					this.Log("Error setting kernel argument: int length", err.ToString());
					return;
				}

				// Execute
				err = CL.EnqueueNDRangeKernel(this.Que.Value, kernel, 1, null, [sizes[i]], null, 0, null, out CLEvent evt);
				if (err != CLResultCode.Success)
				{
					this.Log("Error executing kernel", err.ToString());
					return;
				}

				// Wait for event
				err = CL.WaitForEvents(1, [evt]);
				if (err != CLResultCode.Success)
				{
					this.Log("Error waiting for event", err.ToString());
					return;
				}

				// Release event
				err = CL.ReleaseEvent(evt);
				if (err != CLResultCode.Success)
				{
					this.Log("Error releasing event", err.ToString());
					return;
				}
			}
		}

		public void FillKernelsList(ListBox listBox_kernels)
		{
			// Clear list
			listBox_kernels.Items.Clear();
			// Fill list
			foreach (CLKernel kernel in this.Kernels)
			{
				string name = this.GetKernelName(kernel) ?? "N/A";
				listBox_kernels.Items.Add(name);
			}
		}
	}
}