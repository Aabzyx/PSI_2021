using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjetFinal
{
    /// <summary>
    /// Logique d'interaction pour TypeFractale.xaml
    /// </summary>
    public partial class TypeFractale : Window
    {
        private int typeFractale;
        public TypeFractale()
        {
            InitializeComponent();
        }

        private void Mandelbrot(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            typeFractale = 1;
            Close();
        }

        private void Julia(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            typeFractale = 2;
            Close();
        }

        public int Answer
        {
            get { return typeFractale; }
        }
    }
}
