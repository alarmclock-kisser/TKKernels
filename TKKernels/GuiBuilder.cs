using System;

namespace TKKernels
{
	public class GuiBuilder
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		private MainView Win;
		private ListBox LogBox;
		private string Repopath => this.Win.Repopath;

		private Panel ParamsPanel;

		private NumericUpDown[] ParamsNumerics = [];
		private Label[] ParamsLabels = [];
		private ToolTip[] ParamsTips = [];

		public object[] ParamValues => this.GetParamValues(out this.ParamTypes);
		public Type[] ParamTypes = [];

		// ----- ----- ----- CONSTRUCTORS ----- ----- ----- \\
		public GuiBuilder(MainView win, ListBox? listBox_log = null)
		{
			// Set attributes
			this.Win = win;
			this.LogBox = listBox_log ?? new ListBox();


			// Get panel
			this.ParamsPanel = this.GetControl("panel_params", typeof(Panel)) as Panel ?? new Panel();


		}






		// ----- ----- ----- METHODS ----- ----- ----- \\
		// Log
		public void Log(string message, string inner = "", int layer = 1, bool update = false)
		{
			string msg = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] ";
			msg += "<GUI>";

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



		// Get control
		public Control? GetControl(string searchName = "listBox_log", Type? targetType = null)
		{
			Control? control = null;

			// Try to find control in main view controls with contains searchName
			foreach (Control c in this.Win.Controls)
			{
				bool nameMatch = c.Name.Contains(searchName);
				bool typeMatch = true;

				if (targetType != null)
				{
					typeMatch = targetType.IsAssignableFrom(c.GetType());
				}

				if (nameMatch && typeMatch)
				{
					control = c;
					this.Log("Found control: " + control?.Name ?? "<ERROR>", "Search string: '" + searchName ?? "<ERROR>'", 2);
					break;
				}
			}
			if (control == null)
			{
				this.Log("Control not found", "Search string: '" + searchName + "'" + "'" ?? "Returning NULL-Control'", 2);
			}

			return control;
		}

		public Control[] GetControls(string searchName = "listBox_log", Type? targetType = null)
		{
			List<Control> controls = [];
			
			// Try to find control in main view controls with contains searchName
			foreach (Control c in this.Win.Controls)
			{
				bool nameMatch = c.Name.Contains(searchName);
				bool typeMatch = true;
				if (targetType != null)
				{
					typeMatch = targetType.IsAssignableFrom(c.GetType());
				}
				if (nameMatch && typeMatch)
				{
					controls.Add(c);
					this.Log("Found control: " + c.Name, "Search string: '" + searchName + "'", 2);
				}
			}
			if (controls.Count == 0)
			{
				this.Log("Control(s) not found", "Search string: '" + searchName + "'", 2);
			}

			return controls.ToArray();
		}



		// Fill param elements
		public void FillParamElements(string[] paramStrings)
		{
			// Check ParamsPanel
			if (this.ParamsPanel == null)
			{
				this.Log("ParamsPanel is NULL", "Cannot fill param elements", 2);
				return;
			}

			// Clear ParamsPanel & attributes
			this.ParamsPanel.Controls.Clear();
			this.ParamsNumerics = [];
			List<NumericUpDown> numerics = [];
			this.ParamsLabels = [];
			List<Label> labels = [];
			this.ParamsTips = [];
			List<ToolTip> tips = [];

			// Create param elements for each param string (type:name)
			for (int i = 0; i < paramStrings.Length; i++)
			{
				String p = paramStrings[i];

				// Get type & name
				string[] parts = p.Split(':');
				string type = parts[0];
				string name = parts[1];

				// Verify type (default: float (compatibility)) & numerics attributes
				Type t = typeof(float);
				int decimals = 6;
				int min = -10;
				int max = 10;
				float step = 0.001f;
				float value = 1.0f;

				switch (type.ToLower().Trim())
				{
					case "int":
						t = typeof(int);
						decimals = 0;
						min = 0;
						max = 65535;
						step = 1;
						value = 32.0f;
						break;
					case "double":
						t = typeof(double);
						decimals = 15;
						min = -65535;
						max = 65535;
						step = 0.000001f;
						value = 0.0f;
						break;
					case "decimal":
						t = typeof(decimal);
						decimals = 28;
						min = -255;
						max = 255;
						step = 0.0000000000000001f;
						value = 0.0f;
						break;
					case "float":
						break;
					default:
						this.Log("Unknown param type: " + type, "Using default: float (6 decimals)", 2);
						break;
				}

				// Start in ParamsPanel top left corner (margin: 5)
				int x = 5;
				int y = 5 + i * 30;

				// Create label with type (upper case)
				Label label = new()
				{
					Name = "label_param" + i,
					Text = type.ToUpper() + " : ",
					AutoSize = true,
					Location = new Point(x, y),
					Font = new Font("Arial", 10, FontStyle.Bold),
				};

				// Create numeric control with attributes and size till end of ParamsPanel (right margin: 5)
				NumericUpDown numeric = new()
				{
					Name = "numericUpDown_param" + i,
					DecimalPlaces = decimals,
					Minimum = min,
					Maximum = max,
					Increment = (decimal) step,
					Value = (decimal) value,
					Location = new Point(x + 100, y),
					Size = new Size(this.ParamsPanel.Width - 110, 23)
				};

				// Create tooltip with name on label
				ToolTip tip = new()
				{
					ToolTipTitle = name,
					ToolTipIcon = ToolTipIcon.Info,
					IsBalloon = true,
					ShowAlways = true,
					InitialDelay = 500,
					AutoPopDelay = 5000,
					ReshowDelay = 1000
				};
				tip.SetToolTip(label, name);

				// Add to ParamsPanel
				this.ParamsPanel.Controls.Add(label);
				this.ParamsPanel.Controls.Add(numeric);

				// Add to Lists
				labels.Add(label);
				numerics.Add(numeric);
				tips.Add(tip);
			}

			// Set attributes
			this.ParamsLabels = labels.ToArray();
			this.ParamsNumerics = numerics.ToArray();
			this.ParamsTips = tips.ToArray();

			// Log
			this.Log("Filled param elements", "Count: " + this.ParamsNumerics.Length, 2);
		}



		// Get param values
		public object[] GetParamValues(out Type[] types)
		{
			// Get values in types from numeric controls in ParamsPanel
			object[] values = new object[this.ParamsNumerics.Length];
			types = new Type[this.ParamsNumerics.Length];

			for (int i = 0; i < this.ParamsNumerics.Length; i++)
			{
				// Get numeric decimals count
				int decimals = this.ParamsNumerics[i].DecimalPlaces;

				// Convert to correct type -> values
				switch (decimals)
				{
					case 0:
						// int (0 decimals)
						values[i] = (int) this.ParamsNumerics[i].Value;
						types[i] = typeof(int);
						break;
					case 6:
						// float (6 decimals)
						values[i] = (float) this.ParamsNumerics[i].Value;
						types[i] = typeof(float);
						break;
					case 15:
						// double (15 decimals)
						values[i] = (double) this.ParamsNumerics[i].Value;
						types[i] = typeof(double);
						break;
					case 28:
						// decimal (28 decimals)
						values[i] = (decimal) this.ParamsNumerics[i].Value;
						types[i] = typeof(decimal);
						break;
					default:
						// Adjust decimals for float (high compatibility)
						this.ParamsNumerics[i].DecimalPlaces = 6;
						values[i] = (float) this.ParamsNumerics[i].Value;
						types[i] = typeof(float);
						break;
				}
			}

			// Return
			return values;
		}





	}
}
