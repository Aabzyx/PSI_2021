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
    /// Logique d'interaction pour Histogramme.xaml
    /// </summary>
    public partial class Histogramme : Window
    {
        private int typeHisto;
        public Histogramme()
        {
            InitializeComponent();
        }

        private void btnGris(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            typeHisto = 1;
            Close();
        }

        private void btnR(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            typeHisto = 2;
            Close();
        }

        private void btnV(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            typeHisto = 3;
            Close();
        }

        private void btnB(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            typeHisto = 4;
            Close();
        }

        private void btnRVB(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            typeHisto = 5;
            Close();
        }

        public int Answer
        {
            get { return typeHisto; }
        }
    }
}
