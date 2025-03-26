using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKKernels
{
	public class ImageHandling
	{
		// ----- ATTRIBUTES -----
		private string Repopath;
		private ListBox ImagesList;

		private BindingList<ImageObject> bindingImages;



		// ----- LAMBDA -----
		public BindingList<ImageObject> Images => bindingImages;

		public int ImageCount => this.Images.Count;
		public long[] ImagePointers => this.Images.Select(x => x.Ptr).ToArray();

		public ImageObject? CurrentImage =>
			ImagesList.SelectedIndex != -1 && ImagesList.SelectedIndex < ImageCount ?
			this.Images[ImagesList.SelectedIndex] : null;



		// ----- CONSTRUCTOR -----
		public ImageHandling(string repopath, ListBox? imagesList = null)
		{
			this.Repopath = repopath;
			this.ImagesList = imagesList ?? new ListBox();

			// BindingList initialisieren (keine extra List<ImageObject> mehr nötig)
			this.bindingImages = new BindingList<ImageObject>();

			// Data binding für ListBox
			this.ImagesList.DataSource = this.bindingImages;
			this.ImagesList.DisplayMember = "Name";
			this.ImagesList.SelectedIndex = -1;

			// Register events
			this.ImagesList.MouseDown += (s, e) => RemoveImageOnRightClick(this.ImagesList.SelectedIndex);

		}



		// ----- METHODS -----
		// Add
		public void AddImage(string filePath)
		{
			Image img = Image.FromFile(filePath);
			var newImage = new ImageObject(Path.GetFileName(filePath), img);
			this.bindingImages.Add(newImage);
		}

		public void AddImage(Image img, string name = "")
		{
			var newImage = new ImageObject(name, img);
			this.bindingImages.Add(newImage);
		}

		public void AddImage(int ptr, string name = "")
		{
			var newImage = new ImageObject(name, ptr);
			this.bindingImages.Add(newImage);
		}

		// Remove
		public void RemoveImage(string name)
		{
			var item = this.bindingImages.FirstOrDefault(x => x.Name == name);
			if (item != null)
			{
				item.Dispose();
				this.bindingImages.Remove(item);
			}
		}

		public void RemoveImage(int index)
		{
			if (index >= 0 && index < this.bindingImages.Count)
			{
				this.bindingImages[index].Dispose();
				this.bindingImages.RemoveAt(index);
			}
		}

		// Clear
		public void ClearImages()
		{
			foreach (var img in this.bindingImages)
			{
				img.Dispose();
			}
			this.bindingImages.Clear();
		}

		// ----- EVENTS -----
		private void RemoveImageOnRightClick(int index)
		{
			if (Control.MouseButtons != MouseButtons.Right || Control.MouseButtons == MouseButtons.Left)
			{
				return;
			}

			this.RemoveImage(index);
		}
	}




	public class ImageObject
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Name { get; set; } = "";
		public long Ptr = 0;


		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		public bool OnHost => this.Img != null && this.Ptr == 0;
		public bool OnDevice => this.Ptr != 0 && this.Img == null;


		public Image? Img = null;

		public int Width => this.Img?.Width ?? 0;
		public int Height => this.Img?.Height ?? 0;

		public long Size => this.Width * this.Height * 4;

		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public ImageObject(string name, Image? img = null)
		{
			this.Name = name;
			this.Img = img;
		}

		public ImageObject(string name, int ptr)
		{
			this.Name = name;
			this.Ptr = ptr;
		}




		// ----- ----- ----- METHODS ----- ----- ----- \\
		public void Dispose()
		{
			this.Img?.Dispose();
			this.Img = null;
		}

		public void ClearImage()
		{
			this.Img?.Dispose();
			this.Img = null;
		}

		public List<byte[]> GetPixelRowsAsBytes()
		{
			// New byte[]-list for each row & new bitmap from image
			List<byte[]> rows = [];

			// Check Img
			if (this.Img == null)
			{
				return rows;
			}
			Bitmap bmp = new(this.Img);

			// For every row: Lock, copy, unlock
			for (int y = 0; y < bmp.Height; y++)
			{
				// Get row
				Rectangle rect = new Rectangle(0, y, bmp.Width, 1);
				BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

				// Copy row
				byte[] row = new byte[bmpData.Stride];
				System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, row, 0, row.Length);
				rows.Add(row);

				// Unlock
				bmp.UnlockBits(bmpData);
			}

			// Return list of byte[]
			return rows;
		}

		public Image? SetImageFromChunks(List<byte[]> rows)
		{
			// Get dimensions from rows
			int width = (rows.FirstOrDefault()?.Length ?? 0) / 4;
			int height = rows.Count;

			// Check dimensions
			if (width < 1 || height < 1)
			{
				Ptr = 0;
				Img = null;
				return null;
			}

			// New bitmap
			Bitmap bmp = new(width, height, PixelFormat.Format32bppArgb);

			// For every row: Lock, copy, unlock
			for (int y = 0; y < bmp.Height; y++)
			{
				// Get row
				Rectangle rect = new(0, y, bmp.Width, 1);
				BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

				// Copy row
				byte[] row = rows[y];
				System.Runtime.InteropServices.Marshal.Copy(row, 0, bmpData.Scan0, row.Length);

				// Unlock
				bmp.UnlockBits(bmpData);
			}

			// Update image
			this.Img = bmp;
			Ptr = 0;

			// Return image
			return bmp;
		}

	}

}
