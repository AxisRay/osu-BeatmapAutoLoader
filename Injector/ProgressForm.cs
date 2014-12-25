using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Injector
{
    public partial class ProgressForm : Form
    {
        public ProgressForm(string fileName)
        {
            lbl_fileName.Text = fileName;
            InitializeComponent();
        }

        public void SetProgress(int value)
        {
            BeginInvoke(new EventHandler((sender, args) =>
            {
                progressBar.Value = value;
                lbl_progress.Text = value.ToString() + "%";
            }));
        }
    }
}
