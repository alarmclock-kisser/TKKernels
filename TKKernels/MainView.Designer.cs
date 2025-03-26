namespace TKKernels
{
    partial class MainView
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			comboBox_devices = new ComboBox();
			listBox_log = new ListBox();
			pictureBox_view = new PictureBox();
			panel_view = new Panel();
			listBox_images = new ListBox();
			numericUpDown_zoom = new NumericUpDown();
			label_memory = new Label();
			label_size = new Label();
			((System.ComponentModel.ISupportInitialize) pictureBox_view).BeginInit();
			panel_view.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) numericUpDown_zoom).BeginInit();
			SuspendLayout();
			// 
			// comboBox_devices
			// 
			comboBox_devices.FormattingEnabled = true;
			comboBox_devices.Location = new Point(12, 12);
			comboBox_devices.Name = "comboBox_devices";
			comboBox_devices.Size = new Size(292, 23);
			comboBox_devices.TabIndex = 0;
			// 
			// listBox_log
			// 
			listBox_log.FormattingEnabled = true;
			listBox_log.ItemHeight = 15;
			listBox_log.Location = new Point(12, 795);
			listBox_log.Name = "listBox_log";
			listBox_log.Size = new Size(810, 154);
			listBox_log.TabIndex = 1;
			// 
			// pictureBox_view
			// 
			pictureBox_view.BackColor = Color.White;
			pictureBox_view.Location = new Point(0, 0);
			pictureBox_view.Name = "pictureBox_view";
			pictureBox_view.Size = new Size(512, 512);
			pictureBox_view.TabIndex = 2;
			pictureBox_view.TabStop = false;
			// 
			// panel_view
			// 
			panel_view.BackColor = Color.Black;
			panel_view.Controls.Add(pictureBox_view);
			panel_view.Location = new Point(310, 12);
			panel_view.Name = "panel_view";
			panel_view.Size = new Size(512, 512);
			panel_view.TabIndex = 3;
			// 
			// listBox_images
			// 
			listBox_images.FormattingEnabled = true;
			listBox_images.ItemHeight = 15;
			listBox_images.Location = new Point(12, 635);
			listBox_images.Name = "listBox_images";
			listBox_images.Size = new Size(194, 154);
			listBox_images.TabIndex = 4;
			// 
			// numericUpDown_zoom
			// 
			numericUpDown_zoom.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDown_zoom.Location = new Point(747, 530);
			numericUpDown_zoom.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			numericUpDown_zoom.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDown_zoom.Name = "numericUpDown_zoom";
			numericUpDown_zoom.Size = new Size(75, 23);
			numericUpDown_zoom.TabIndex = 5;
			numericUpDown_zoom.Value = new decimal(new int[] { 100, 0, 0, 0 });
			numericUpDown_zoom.ValueChanged += numericUpDown_zoom_ValueChanged;
			// 
			// label_memory
			// 
			label_memory.AutoSize = true;
			label_memory.Location = new Point(12, 38);
			label_memory.Name = "label_memory";
			label_memory.Size = new Size(58, 15);
			label_memory.TabIndex = 6;
			label_memory.Text = "Memory: ";
			// 
			// label_size
			// 
			label_size.AutoSize = true;
			label_size.Location = new Point(310, 527);
			label_size.Name = "label_size";
			label_size.Size = new Size(33, 15);
			label_size.TabIndex = 7;
			label_size.Text = "Size: ";
			// 
			// MainView
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(834, 961);
			Controls.Add(label_size);
			Controls.Add(label_memory);
			Controls.Add(numericUpDown_zoom);
			Controls.Add(listBox_images);
			Controls.Add(panel_view);
			Controls.Add(listBox_log);
			Controls.Add(comboBox_devices);
			Name = "MainView";
			Text = "Form1";
			((System.ComponentModel.ISupportInitialize) pictureBox_view).EndInit();
			panel_view.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize) numericUpDown_zoom).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ComboBox comboBox_devices;
		private ListBox listBox_log;
		private PictureBox pictureBox_view;
		private Panel panel_view;
		private ListBox listBox_images;
		private NumericUpDown numericUpDown_zoom;
		private Label label_memory;
		private Label label_size;
	}
}
