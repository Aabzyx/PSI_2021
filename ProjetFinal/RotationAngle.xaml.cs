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
    /// Logique d'interaction pour RotationAngle.xaml
    /// </summary>
    public partial class RotationAngle : Window
    {
        public RotationAngle(string question)
        {
            InitializeComponent();
            txtQuestion.Content = question;
            txtAnswer.Text = "";
        }

        private void btnDialog_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtAnswer.SelectAll();
            txtAnswer.Focus();
        }

        public string Answer
        {
            get { return txtAnswer.Text; }
        }
    }
}
