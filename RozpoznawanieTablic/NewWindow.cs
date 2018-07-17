using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RozpoznawanieTablic
{
    public partial class NewWindow : Form
    {
        public NewWindow(Emgu.CV.UI.ImageBox ObrazDoPokazania)
        {
            InitializeComponent();
            imageBox1.Image = ObrazDoPokazania.Image;
        }

        public NewWindow(Image<Gray, byte> _imgAlgorithm)
        {
            InitializeComponent();
            imageBox1.Image = _imgAlgorithm;
        }

        public NewWindow(Image<Bgr, byte> _imgAlgorithm)
        {
            InitializeComponent();
            imageBox1.Image = _imgAlgorithm;
        }

    }
}
