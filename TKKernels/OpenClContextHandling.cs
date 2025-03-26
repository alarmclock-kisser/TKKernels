using OpenTK;
using OpenTK.Compute.OpenCL;
using System.Text;

namespace TKKernels
{
	public class OpenClContextHandling
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;

		public ListBox LogBox;
		public ComboBox DevicesCombo;


		public Dictionary<CLPlatform, CLDevice[]> Devices = [];

		public CLContext? Ctx = null;
		public CLDevice? Dev = null;

		public OpenClMemoryHandling? MemH;
		public OpenClKernelHandling? KernelH;



		// ----- ----- ----- LAMBDA ----- ----- ----- \\







		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public OpenClContextHandling(string repopath, ListBox? logBox = null, ComboBox? devicesCombo = null)
		{
			// Set attributes
			this.Repopath = repopath;
			this.LogBox = logBox ?? new ListBox();
			this.DevicesCombo = devicesCombo ?? new ComboBox();

			// Register events
			this.DevicesCombo.SelectedIndexChanged += (s, e) => this.InitializeContext(this.DevicesCombo.SelectedIndex);

			// Get devices
			this.Devices = this.GetDevices();
			this.Log("OpenCL Devices loaded", this.Devices.Count + " platforms", 1);

			// Fill combo and set device
			this.FillDevicesCombo(-1);
		}






		// ----- ----- ----- METHODS ----- ----- ----- \\
		// Log
		public void Log(string message, string inner = "", int layer = 1, bool update = false)
		{
			string msg = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] ";
			msg += "<Context>";

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


		// Get devices
		public Dictionary<CLPlatform, CLDevice[]> GetDevices(DeviceType? type = null)
		{
			// Get platforms
			var err = CL.GetPlatformIds(out CLPlatform[] platforms);
			if (err != CLResultCode.Success)
			{
				this.Log("Error getting platforms", err.ToString());
				return [];
			}

			// Get devices for each platform
			Dictionary<CLPlatform, CLDevice[]> devices = [];
			foreach (var platform in platforms)
			{
				err = CL.GetDeviceIds(platform, type ?? DeviceType.All, out CLDevice[] devs);
				if (err != CLResultCode.Success)
				{
					this.Log("Error getting devices", err.ToString());
					return [];
				}
				devices[platform] = devs;
			}

			return devices;
		}

		public List<CLDevice> GetDevicesList(DeviceType? type = null)
		{
			List<CLDevice> devices = [];
			foreach (var devs in this.Devices.Values)
			{
				devices.AddRange(devs);
			}

			return devices;
		}

		public void FillDevicesCombo(int set = -1)
		{
			// Clear
			this.DevicesCombo.Items.Clear();
			
			// Fill
			foreach (var platform in this.Devices.Keys)
			{
				foreach (var device in this.Devices[platform])
				{
					this.DevicesCombo.Items.Add(this.GetPlatformName(platform) + " - " + this.GetDeviceName(device));
				}
			}

			// Set
			if (set >= 0 && set < this.DevicesCombo.Items.Count)
			{
				this.DevicesCombo.SelectedIndex = set;
			}
		}


		// Info
		public string GetDeviceName(CLDevice device)
		{
			// Get name
			var err = CL.GetDeviceInfo(device, DeviceInfo.Name, out byte[] name);
			if (err != CLResultCode.Success)
			{
				this.Log("Error getting device name", err.ToString());
				return "";
			}

			// Convert to string
			return Encoding.ASCII.GetString(name).Trim();

		}

		public string GetDeviceType(CLDevice device)
		{
			// Get type
			var err = CL.GetDeviceInfo(device, DeviceInfo.Type, out byte[] type);
			if (err != CLResultCode.Success)
			{
				this.Log("Error getting device type", err.ToString());
				return "";
			}

			// Convert to string
			return Encoding.ASCII.GetString(type).Trim();
		}

		public string GetPlatformName(CLPlatform platform)
		{
			// Get name
			var err = CL.GetPlatformInfo(platform, PlatformInfo.Name, out byte[] name);
			if (err != CLResultCode.Success)
			{
				this.Log("Error getting platform name", err.ToString());
				return "";
			}

			// Convert to string
			return Encoding.ASCII.GetString(name).Trim();
		}

		public string GetPlatformVersion(CLPlatform platform)
		{
			// Get vendor
			var err = CL.GetPlatformInfo(platform, PlatformInfo.Version, out byte[] version);
			if (err != CLResultCode.Success)
			{
				this.Log("Error getting platform vendor", err.ToString());
				return "";
			}

			// Convert to string
			return Encoding.ASCII.GetString(version).Trim();
		}

		public List<string> GetDeviceNames()
		{
			// Get devices
			List<CLDevice> devices = this.GetDevicesList();

			// Get names
			List<string> names = [];
			foreach (var device in devices)
			{
				names.Add(this.GetDeviceName(device));
			}

			return names;
		}


		// Context
		public void InitializeContext(int deviceIndex)
		{
			// Void previous
			this.Dispose();

			// Get devices
			List<CLDevice> devices = this.GetDevicesList();

			// Check index
			if (deviceIndex < 0 || deviceIndex >= devices.Count)
			{
				this.Log("No OpenCL Context initialized", "Invalid device index", 1);
				return;
			}

			// Get device
			this.Dev = devices[deviceIndex];

			// Create context -> Ctx
			this.Ctx = CL.CreateContext(0, [this.Dev.Value], 0, 0, out var err);
			if (err != CLResultCode.Success)
			{
				this.Log("Error creating context", err.ToString());
				this.Ctx = null;
				return;
			}

			// Create objects
			this.MemH = new OpenClMemoryHandling(this);
			this.KernelH = new OpenClKernelHandling(this);

			// Log
			this.Log("OpenCL Context initialized", this.GetDeviceName(this.Dev.Value), 1);
		}

		public void Dispose()
		{
			// Void context
			if (this.Ctx != null)
			{
				CL.ReleaseContext(this.Ctx.Value);
				this.Ctx = null;
			}

			// Void device
			if (this.Dev != null)
			{
				CL.ReleaseDevice(this.Dev.Value);
				this.Dev = null;
			}

			// Dispose objects
			this.MemH?.Dispose();
			this.MemH = null;
			this.KernelH?.Dispose();
			this.KernelH = null;
		}

	}
}
