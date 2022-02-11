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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Path = System.IO.Path;
using Directory = System.IO.Directory;

namespace ProjetFinal
{
    public partial class MainWindow : Window
    {
        private string imageDeBase;
        private List<string> historique = new List<string>();
        /// <summary>
        /// Cette fonction sera appelée pour que l'utilisateur choisisse l'image à traiter.
        /// On va donc ouvrir une fenêtre de dialogue et n'afficher que les fichier bmp
        /// Si l'utilisateur clique sur "ouvrir", on va activer les boutons de modification (rotation, changement de taille, etc.)
        /// qui étaient désactivés sans image sélectionnée. On active également le bouton "Reset", qui permettra de retourner à l'image de base à tout moment.
        /// Enfin, on va afficher le chemin vers l'image sélectionnée dans l'encadré à gauche du bouton, et enregistrer ledit chemin dans la liste servant d'historique
        /// Enfin, on affiche l'image dans l'application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChoixFichier(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files|*.bmp|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ModifImage.IsEnabled = true;
                Defaut.IsEnabled = true;

                imageDeBase = dlg.FileName;
                string selectedFileName = dlg.FileName;
                FileNameLabel.Content = selectedFileName;

                historique.Add(selectedFileName);

                ImageSource imgSource = new BitmapImage(new Uri(selectedFileName));

                ImageAAfficher.Source = imgSource;
            }
        }

        /// <summary>
        /// Ici, on met en place la fonction qui sert à actualiser l'image affichée dans l'application
        /// les deux lignes suivant BeginInit serviront à pouvoir réutiliser une image qui vient d'être exportée et qui est toujours en cache
        /// (par exemple, pour l'utilisation répétée d'une même fonction), puis on actualise le chemin vers la nouvelle image dans le cadre en haut de l'application
        /// et on sauvegarde ledit chemin dans l'historique, avant d'afficher l'image (on utilise le chemin relatif, pour que le programme fonctionne chez tous les utilisateurs)
        /// </summary>
        /// <param name="path">Chemin de l'image à afficher</param>
        private void ActualisationImage(string path)
        {
            BitmapImage affichage = new BitmapImage();
            affichage.BeginInit();
            affichage.CacheOption = BitmapCacheOption.OnLoad;
            affichage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            affichage.UriSource = new Uri(Path.Combine(Directory.GetCurrentDirectory(), @path));
            affichage.EndInit();
            ImageSource imgSource = affichage;
            FileNameLabel.Content = Path.Combine(Directory.GetCurrentDirectory(), @path);
            historique.Add(Path.Combine(Directory.GetCurrentDirectory(), @path));
            Past.IsEnabled = true;
            ImageAAfficher.Source = imgSource;
        }

        /// <summary>
        /// Pour l'intégralité de la section TD3, les fonctions marchent exactement de la même manière :
        /// on créée une nouvelle variable de classe MyImage, on applique la fonction demandée, on exporte l'image et on l'affiche
        /// (on aura pris soin pour les fonctions Rotation et ChangementTaille de mettre en place une sécurité évitant le crash du programme
        /// si l'utilisateur ne rentre pas une réponse valide).
        /// </summary>
        #region TD3
        private void NuancesDeGris(object sender, RoutedEventArgs e)
        {
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            image.NuancesDeGris();
            image.From_Image_To_File("./gris.bmp");
            ActualisationImage("gris.bmp");
        }

        private void NoirEtBlanc(object sender, RoutedEventArgs e)
        {
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            image.NoirEtBlanc();
            image.From_Image_To_File("./noiretblanc.bmp");
            ActualisationImage("noiretblanc.bmp");
        }

        private void Rotate(object sender, RoutedEventArgs e)
        {
            RotationAngle rotationAngle = new RotationAngle("De quel angle souhaitez-vous faire pivoter l'image ?");
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            double angle;
            if (rotationAngle.ShowDialog() == true)
            {
                try
                {
                    angle = Convert.ToDouble(rotationAngle.Answer);
                }
                catch
                {
                    angle = 0;
                }
                angle = Math.PI * angle / 180;
                image.Rotate(angle);
                image.From_Image_To_File("./pivot.bmp");
                ActualisationImage("pivot.bmp");
            }
        }

        private void ChangementTaille(object sender, RoutedEventArgs e)
        {
            RotationAngle rotationAngle = new RotationAngle("Coefficient ? (AVEC UNE VIRGULE, PAS DE POINT)");
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            double coeff;
            if (rotationAngle.ShowDialog() == true)
            {
                if (rotationAngle.Answer != "" && rotationAngle.Answer != "0")
                {
                    try{
                        coeff = Convert.ToDouble(rotationAngle.Answer);
                        if(coeff <= 0)
                        {
                            coeff = 1;
                        }
                    }
                    catch{
                        coeff = 1;
                    }
                    image.ChgmtTaille(coeff);
                    image.From_Image_To_File("./changementTaille.bmp");
                    ActualisationImage("changementTaille.bmp");
                }
            }
        }

        private void Miroir(object sender, RoutedEventArgs e)
        {
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            image.Miroir();
            image.From_Image_To_File("./miroir.bmp");
            ActualisationImage("miroir.bmp");
        }
        #endregion

        /// <summary>
        /// Idem que dans la section TD3, sauf qu'avant d'appliquer la fonction, on définit une matrice correspondant
        /// au filtre demandé (matrice trouvée sur internet)
        /// </summary>
        #region TD4
        private void Flou(object sender, RoutedEventArgs e)
        {
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            int[,] mat = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            int coeff = mat.GetLength(0) * mat.GetLength(1);
            image.Filtres(mat, coeff);
            image.From_Image_To_File("./flou.bmp");
            ActualisationImage("flou.bmp");
        }

        private void RenforcementDesBords(object sender, RoutedEventArgs e)
        {
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            int[,] mat = new int[,] { { 0, 0, 0 }, { -1, 1, 0 }, { 0, 0, 0 } };
            image.Filtres(mat, 1);
            image.From_Image_To_File("./renforcement.bmp");
            ActualisationImage("renforcement.bmp");
        }

        private void DetectionDesBords(object sender, RoutedEventArgs e)
        {
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            int[,] mat = new int[,] { { 0, 1, 0 }, { 1, -4, 1 }, { 0, 1, 0 } };
            image.Filtres(mat, 1);
            image.From_Image_To_File("./detection.bmp");
            ActualisationImage("detection.bmp");
        }

        private void Repoussage(object sender, RoutedEventArgs e)
        {
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            int[,] mat = new int[,] { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };
            image.Filtres(mat, 1);
            image.From_Image_To_File("./repoussage.bmp");
            ActualisationImage("repoussage.bmp");
        }

        #endregion


        #region TD5
        /// <summary>
        /// Idem que dans les deux sections précédentes, sauf que l'export et l'affichage dépendront de l'histogramme demandé par l'utilisateur
        /// </summary>
        public void Histo(object sender, RoutedEventArgs e)
        {
            MyImage image = new MyImage(Convert.ToString(FileNameLabel.Content));
            Histogramme histo = new Histogramme();
            if (histo.ShowDialog() == true)
            {
                switch (histo.Answer)
                {
                    case 1:
                        MyImage nuance_de_gris = image.Histogramme(1);
                        nuance_de_gris.From_Image_To_File("./Histogramme_nuance_de_gris.bmp");
                        ActualisationImage("Histogramme_nuance_de_gris.bmp");
                        break;
                    case 2:
                        MyImage rouge = image.Histogramme(2);
                        rouge.From_Image_To_File("./Histogramme_rouge.bmp");
                        ActualisationImage("Histogramme_rouge.bmp");
                        break;
                    case 3:
                        MyImage vert = image.Histogramme(3);
                        vert.From_Image_To_File("./Histogramme_vert.bmp");
                        ActualisationImage("Histogramme_vert.bmp");
                        break;
                    case 4:
                        MyImage bleu = image.Histogramme(4);
                        bleu.From_Image_To_File("./Histogramme_bleu.bmp");
                        ActualisationImage("Histogramme_bleu.bmp");
                        break;
                    case 5:
                        MyImage RGB = image.HistogrammeRGB();
                        RGB.From_Image_To_File("./Histogramme_RGB.bmp");
                        ActualisationImage("Histogramme_RGB.bmp");
                        break;
                }
            }
        }

        /// <summary>
        /// Idem que dans la section précédente, sauf que l'export et l'affichage dépendront de la fractale demandée par l'utilisateur
        /// </summary>
        public void Fractales(object sender, RoutedEventArgs e)
        {
            TypeFractale typefractale = new TypeFractale();
            if (typefractale.ShowDialog() == true)
            {
                MyImage image = new MyImage(new Pixel[1, 1] { { new Pixel(0, 0, 0) } });
                switch (typefractale.Answer)
                {
                    case 1:
                        MyImage Mandelbrot = image.Fractales(50, 100);
                        Mandelbrot.From_Image_To_File("./fractale_Mandelbrot.bmp");
                        ActualisationImage("fractale_Mandelbrot.bmp");
                        Past.IsEnabled = false;
                        break;
                    case 2:
                        MyImage Julia = image.Fractales(150, 100);
                        Julia.From_Image_To_File("./fractale_Julia.bmp");
                        ActualisationImage("fractale_Julia.bmp");
                        Past.IsEnabled = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Dans cette fonction, on va récupérer l'image entrée par l'utilisateur, puis lui en demander une seconde afin de cacher l'image 2 dans la 1
        /// </summary>
        public void Cryptographie(object sender, RoutedEventArgs e)
        {
            MyImage image1 = new MyImage(Convert.ToString(FileNameLabel.Content));
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files|*.bmp|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFileName = dlg.FileName;
                MyImage image2 = new MyImage(selectedFileName);
                image1.Cryptographie(image2).From_Image_To_File("./Cryptographie.bmp");
            }
        }

        /// <summary>
        /// Ici, on va simplement décrypter les deux images cachées dans celle ouverte par l'utilisateur, et les enregistrer séparément
        /// </summary>
        public void Cryptographie_inv(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files|*.bmp|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MyImage image = new MyImage(dlg.FileName);
                MyImage[] tab = image.DeCryptographie();
                tab[0].From_Image_To_File("./imageDecryptee1.bmp");
                tab[1].From_Image_To_File("./imageDecryptee2.bmp");
            }
        }
        #endregion

        /// <summary>
        /// Dans cette fonction, on demande à l'utilisateur une phrase et on va la convertir en QR Code
        /// si cette dernière n'est pas trop longue
        ///  /!\ NE PAS UTILISER DE CARACTÈRES NON PREVUS POUR LE QR CODE /!\
        ///  On exportera ensuite une version "normale" du QR Code et une seconde aggrandie pour faciliter l'affichage
        ///  sur WPF.
        /// </summary>
        public void QRCode(object sender, RoutedEventArgs e)
        {
            RotationAngle rotationAngle = new RotationAngle("Entrez une phrase maximum 47 caractères espaces compris");
            string phrase = "";
            if (rotationAngle.ShowDialog() == true)
            {
                phrase = rotationAngle.Answer;
                if (phrase.Length != 0 && phrase.Length < 47)
                {
                    Pixel[,] mat = new Pixel[1, 1] { { new Pixel(0, 0, 0) } };
                    MyImage a = new MyImage(mat);
                    MyImage QRCode = a.QRCode(phrase);
                    QRCode.From_Image_To_File("./QRCode.bmp");
                    QRCode.ChgmtTaille(10);
                    QRCode.From_Image_To_File("./NE_PAS_TOUCHER/QRCodeLisibleWPF.bmp");
                    ActualisationImage("NE_PAS_TOUCHER/QRCodeLisibleWPF.bmp");
                    Past.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Ici, nous allons simplement ouvrir un QR code et, si nous réussissons à lire le texte,
        /// l'afficher dans le cadre prévu normalement pour les chemins d'image, en haut à gauche de l'application
        /// </summary>
        public void QRCode_inv(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files|*.bmp|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MyImage image = new MyImage(Convert.ToString(dlg.FileName));
                FileNameLabel.Content = image.QRCode_inv();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cette fonction va nous servir d'historique, c'est-à-dire de revenir en arrière dans les modifications
        /// On supprime donc la dernière modification en date et on affiche l'avant-dernière
        /// </summary>
        public void Historique(object sender, RoutedEventArgs e)
        {
            historique.RemoveAt(historique.Count - 1);
            BitmapImage affichage = new BitmapImage();
            affichage.BeginInit();
            affichage.CacheOption = BitmapCacheOption.OnLoad;
            affichage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            affichage.UriSource = new Uri(historique[historique.Count - 1]);
            affichage.EndInit();
            ImageSource imgSource = affichage;
            FileNameLabel.Content = historique[historique.Count - 1];
            ImageAAfficher.Source = imgSource;
            if (historique.Count <= 1)
            {
                Past.IsEnabled = false;
            }
        }

        /// <summary>
        /// Bouton affichant à nouveau l'image entrée par l'utilisateur en tout premier lieu
        /// </summary>
        public void Reinitialisation(object sender, RoutedEventArgs e)
        {
            FileNameLabel.Content = imageDeBase;
            ImageSource imgSource = new BitmapImage(new Uri(imageDeBase));
            ImageAAfficher.Source = imgSource;
        }

        /// <summary>
        /// Bouton sortie, fermant la console
        /// </summary>
        public void Sortie(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
