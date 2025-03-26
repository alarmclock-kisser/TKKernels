using OpenTK.Compute.OpenCL;
using System.Runtime.InteropServices;

namespace TKKernels
{
	public class OpenClMemoryHandling
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public OpenClContextHandling ContextH;


		public Dictionary<CLBuffer[], int[]> Buffers = [];

		public CLCommandQueue? Que = null;

		


		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		private string Repopath => this.ContextH.Repopath;
		private ListBox LogBox => this.ContextH.LogBox;


		private CLContext? Ctx => this.ContextH.Ctx;
		private CLDevice? Dev => this.ContextH.Dev;


		public long[] Pointers => this.Buffers.Select(b => (long) b.Key.FirstOrDefault().GetHashCode()).ToArray();

		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public OpenClMemoryHandling(OpenClContextHandling contextH)
		{
			// Set attributes
			this.ContextH = contextH;

			// Create CLCommandQueue
			if (this.Ctx != null && this.Dev != null)
			{
				this.Que = CL.CreateCommandQueueWithProperties(this.Ctx.Value, this.Dev.Value, 0, out CLResultCode err);
				if (err != CLResultCode.Success)
				{
					this.Log("Error creating command queue", err.ToString());
					this.Que = null;
				}
			}

		}





		// ----- ----- ----- METHODS ----- ----- ----- \\
		// Log
		public void Log(string message, string inner = "", int layer = 1, bool update = false)
		{
			string msg = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] ";
			msg += "<Memory>";

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


		// Dispose & Free
		public void Dispose()
		{
			// Free every buffer (group)
			long[] pointers = this.Pointers;
			for (int i = 0; i < pointers.Length; i++)
			{
				this.FreeBuffers(pointers[i]);
			}

			// Free command queue
			if (this.Que != null)
			{
				CLResultCode err = CL.ReleaseCommandQueue(this.Que.Value);
				if (err != CLResultCode.Success)
				{
					this.Log("Error releasing command queue", err.ToString());
				}
			}
			this.Que = null;

		}

		public void FreeBuffers(long ptr)
		{
			// Get buffers
			CLBuffer[] buffers = this.FindBuffers(ptr);
			if (buffers.Length == 0)
			{
				return;
			}

			try
			{
				// Free every buffer
				for (int i = 0; i < buffers.Length; i++)
				{
					CLResultCode err = CL.ReleaseMemoryObject(buffers[i]);
					if (err != CLResultCode.Success)
					{
						this.Log("Error freeing buffer", err.ToString());
					}
				}

				// Remove from Buffers & Types
				this.Buffers.Remove(buffers);
			}
			catch (Exception e)
			{
				this.Log("Error freeing buffers", e.Message);
			}
		}


		// Buffer (group) info
		public long GetMemoryTotal()
		{
			// Abort if no Dev
			if (this.Dev == null)
			{
				return 0;
			}

			// Get memory attribute for Dev or Ctx ...
			CLResultCode err = CL.GetDeviceInfo(this.Dev.Value, DeviceInfo.GlobalMemorySize, out byte[]? res);
			if (err != CLResultCode.Success || res == null)
			{
				this.Log("Error getting memory total", err.ToString());
				return -1;
			}
			return BitConverter.ToInt64(res, 0);
		}

		public long GetMemoryAllocated<T>() where T : unmanaged
		{
			// Get all pointers & type size
			long[] pointers = this.Pointers;
			int typeSize = Marshal.SizeOf<T>();

			// Get size of every buffer & add up
			long alloc = 0;
			for (int i = 0; i < pointers.Length; i++)
			{
				alloc += this.GetBuffersSize(pointers[i]) * typeSize;
			}

			// Return
			return alloc;
		}

		public string GetBufferPointerString(long ptr)
		{
			CLBuffer? buffer = this.FindBuffers(ptr).FirstOrDefault();
			if (buffer == null)
			{
				this.Log("Error getting buffer pointer", "No buffer found with ptr " + ptr);
				return "N/A";
			}

			CLResultCode err = CL.GetMemObjectInfo(buffer.Value, MemoryObjectInfo.HostPointer, out byte[]? res);
			if (err != CLResultCode.Success || res == null)
			{
				this.Log("Error getting buffer pointer", err.ToString());
				return "N/A" ;
			}

			// Return converted from byte[] to byteString
			return BitConverter.ToString(res);
		}

		public CLBuffer[] FindBuffers(long ptr)
		{
			// Find buffer group with first buffers hashCode == ptr
			return this.Buffers.FirstOrDefault(b => b.Key.FirstOrDefault().GetHashCode() == ptr).Key;
		}
		
		public int[] FindLengths(long ptr)
		{
			// Find buffer group with first buffers hashCode == ptr
			return this.Buffers.FirstOrDefault(b => b.Key.FirstOrDefault().GetHashCode() == ptr).Value;
		}

		public int GetBuffersCount(int ptr)
		{
			return this.FindBuffers(ptr).Length;
		}

		public long GetBuffersSize(long ptr)
		{
			// Get buffers
			CLBuffer[] buffers = this.FindBuffers(ptr);
			long total = 0;

			// Get size of every buffer & add up
			for (int i = 0;  i < buffers.Length; i++)
			{
				CLResultCode err = CL.GetMemObjectInfo(buffers[i], MemoryObjectInfo.Size, out byte[]? res);
				if (err != CLResultCode.Success || res == null)
				{
					this.Log("Error getting buffer size", err.ToString());
					continue;
				}
				long size = BitConverter.ToInt64(res, 0);
				total += size;
			}

			// Return
			return total;
		}


		// Push chunks
		public long PushChunks<T>(List<T[]> chunks) where T : unmanaged
		{
			long ptr = 0;

			// Abort if no chunks or no context
			if (chunks.Count == 0 || this.Ctx == null)
			{
				return ptr;
			}

			// Get sizes of chunks
			nuint[] lengths = chunks.Select(c => (nuint) c.Length).ToArray();
			if (lengths.Length == 0 || lengths.Any(s => s == 0))
			{
				return ptr;
			}

			// Create buffers
			CLBuffer[] buffers = new CLBuffer[lengths.Length];

			// For every chunk: Create buffer
			for (int i = 0; i < lengths.Length; i++)
			{
				// Create buffer
				buffers[i] = CL.CreateBuffer<T>(this.Ctx.Value, MemoryFlags.CopyHostPtr | MemoryFlags.ReadWrite, chunks[i], out CLResultCode err);
				if (err != CLResultCode.Success)
				{
					this.Log("Error creating buffer", err.ToString());
					return ptr;
				}
			}

			// Add to Buffers & Types
			this.Buffers.Add(buffers, lengths.Select(s => (int) s).ToArray());

			// Get hashCode of first buffer
			ptr = buffers.FirstOrDefault().GetHashCode();

			// Return
			return ptr;
		}


		// Pull chunks
		public List<T[]> PullChunks<T>(long ptr) where T : unmanaged
		{
			List<T[]> chunks = [];

			// Abort if no context
			if (this.Ctx == null || this.Que == null)
			{
				return chunks;
			}

			// Get buffers
			CLBuffer[] buffers = this.FindBuffers(ptr);
			int[] lengths = this.FindLengths(ptr);
			if (buffers.Length == 0 || lengths.Length == 0 || lengths.Any(s => s == 0))
			{
				return chunks;
			}

			// For every buffer: Pull data
			for (int i = 0; i < buffers.Length; i++)
			{
				// Create chunk
				T[] chunk = new T[lengths[i]];

				// Pull data
				CLResultCode err = CL.EnqueueReadBuffer<T>(this.Que.Value, buffers[i], true, 0,  chunk, null, out CLEvent evt);
				if (err != CLResultCode.Success)
				{
					this.Log("Error pulling buffer", err.ToString());
					return chunks;
				}
				
				// Add chunk to chunks
				chunks.Add(chunk);
			}

			// Free buffers
			this.FreeBuffers(ptr);

			// Return
			return chunks;
		}
	}
}