using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ReedSolomon;

namespace ProjetFinal
{
    class MyImage
    {
        #region TD2
        //Variables de classe
        private string typeImage;
        private int tailleDuFichier;
        private int tailleOffset;
        private int largeurImage;
        private int hauteurImage;
        private int nombreBytesParCouleur;
        private Pixel[,] pixels;

        /// <summary>
        /// Constructeur de la classe, on récupère les informations essentielles de l'offset
        /// et on les convertit en entier pour les stocker dans les variables de classe
        /// Puis on créé une matrice de Pixels qu'on remplit à l'aide de la fonction du même nom
        /// </summary>
        /// <param name="myFile">Chemin du fichier à étudier</param>
        public MyImage(string myFile)
        {
            byte[] image = File.ReadAllBytes(myFile);
            typeImage = TypeDImage(image);
            tailleDuFichier = Convertir_Endian_To_Int(image, 4, 2);
            tailleOffset = Convertir_Endian_To_Int(image, 4, 10);
            largeurImage = Convertir_Endian_To_Int(image, 4, 18);
            hauteurImage = Convertir_Endian_To_Int(image, 4, 22);
            nombreBytesParCouleur = Convertir_Endian_To_Int(image, 2, 28);
            pixels = new Pixel[hauteurImage, largeurImage];
            Remplir_pixels(pixels, image);
        }

        /// <summary>
        /// On va remplir de manière automatique la matrice de pixels en commençant
        /// au bon indice (fin de l'offset), et en faisant attention à deux détails :
        ///     - En bitmap, les pixels sont dans l'ordre BVR (ou BGR), il nous faut donc
        /// les inverser pour rester dans l'ordre classique RVB lors de l'utilisation de la matrice
        ///     - Si l'image a une largeur n'étant pas un multiple de 4, il y aura des bytes
        /// non utilisés, qu'il faudra ne pas prendre en compte à l'import.
        /// </summary>
        /// <param name="pixels">Matrice à remplir</param>
        /// <param name="image">Tableau de bytes à partir duquel on va remplir la matrice</param>
        public void Remplir_pixels(Pixel[,] pixels, byte[] image)
        {
            int indice = tailleOffset;
            for (int l = 0; l < pixels.GetLength(0); l++)
            {
                for (int c = 0; c < pixels.GetLength(1); c++)
                {
                    Pixel nouveauPixel = new Pixel(image[indice + 2], image[indice + 1], image[indice]);
                    pixels[l, c] = nouveauPixel;
                    indice += 3;
                }
                indice += largeurImage % 4;
            }
        }

        /// <summary>
        /// La fonction est longue mais très simple dans son fonctionnement :
        /// nous allons créer un tableau de bytes que nous remplirons avec les informations
        /// décrivant l'image exportée selon les paramètres bitmap.
        /// Puis nous ferons l'exact inverse de la fonction Remplir_pixels en remplissant
        /// le tableau à partir de la matrice de Pixels, en faisant attention à replacer les
        /// bytes inutiles.
        /// Puis on exporte le tout grâce à la fonction WriteAllBytes
        /// </summary>
        /// <param name="file">Chemin et nom du fichier utilisé pour l'export</param>
        public void From_Image_To_File(string file)
        {
            byte[] returned = new byte[tailleDuFichier];
            returned[0] = Convert.ToByte(66);
            returned[1] = Convert.ToByte(77);
            byte[] tailleDuFichierEndian = Convertir_Int_To_Endian(tailleDuFichier, 4);
            for (int i = 2; i < 6; i++)
            {
                returned[i] = tailleDuFichierEndian[i - 2];
            }
            byte[] tailleOffsetEndian = Convertir_Int_To_Endian(tailleOffset, 4);
            for (int i = 10; i < 14; i++)
            {
                returned[i] = tailleOffsetEndian[i - 10];
            }
            byte[] tailleHeader = Convertir_Int_To_Endian(tailleOffset - 14, 4);
            for (int i = 14; i < 18; i++)
            {
                returned[i] = tailleHeader[i - 14];
            }
            byte[] largeurImageEndian = Convertir_Int_To_Endian(largeurImage, 4);
            for (int i = 18; i < 22; i++)
            {
                returned[i] = largeurImageEndian[i - 18];
            }
            byte[] hauteurImageEndian = Convertir_Int_To_Endian(hauteurImage, 4);
            for (int i = 22; i < 26; i++)
            {
                returned[i] = hauteurImageEndian[i - 22];
            }
            returned[26] = Convert.ToByte(1);
            byte[] nombreBytesParCouleurEndian = Convertir_Int_To_Endian(nombreBytesParCouleur, 2);
            for (int i = 28; i < 30; i++)
            {
                returned[i] = nombreBytesParCouleurEndian[i - 28];
            }
            byte[] tailleImageEndian = Convertir_Int_To_Endian(tailleDuFichier - tailleOffset, 4);
            for (int i = 34; i < 38; i++)
            {
                returned[i] = tailleImageEndian[i - 34];
            }

            int cpt = tailleOffset;
            for (int i = 0; i < hauteurImage; i++)
            {
                for (int j = 0; j < largeurImage; j++)
                {
                    returned[cpt] = Convert.ToByte(pixels[i, j].Blue);
                    returned[cpt + 1] = Convert.ToByte(pixels[i, j].Green);
                    returned[cpt + 2] = Convert.ToByte(pixels[i, j].Red);
                    cpt += 3;
                }
                cpt += largeurImage % 4;
            }
            File.WriteAllBytes(file, returned);
        }

        //Fonction retournant sous forme de string les informations essentielles contenues dans l'offset
        public string toString()
        {
            string returned = "";
            returned += "Type de l'image : " + typeImage + "\n";
            returned += "Taille de l'image : " + Convert.ToString(tailleDuFichier) + "\n";
            returned += "Taille de l'offset : " + Convert.ToString(tailleOffset) + "\n";
            returned += "Largeur de l'image : " + Convert.ToString(largeurImage) + "\n";
            returned += "Hauteur de l'image : " + Convert.ToString(hauteurImage) + "\n";
            returned += "Nomvre de bytes par couleur : " + Convert.ToString(nombreBytesParCouleur) + "\n";
            return returned;
        }

        //Fonction extrêmement basique, identique à celle utilisée depuis un an
        public string AffichageMatricePixel()
        {
            string returned = "";
            for (int x = 0; x < pixels.GetLength(0); x++)
            {
                for (int y = 0; y < pixels.GetLength(1); y++)
                {
                    returned += pixels[x, y].toString();
                }
                returned += "\n";
            }
            return returned;
        }

        //Ici, on vérifie que l'image est bien de type BM, et on return "inconnu" dans tous les autres cas
        private string TypeDImage(byte[] image)
        {
            string returned = "";
            if (image[0] == 66 && image[1] == 77)
            {
                returned = "BM";
            }
            else returned = "Inconnu";
            return returned;
        }

        /// <summary>
        /// On va récupérer chaque byte sur lequel l'entier est codé, multiplier ledit byte
        /// à la puissance de 256 à laquelle il est associé, puis convertir cette valeur en int
        /// et l'ajouter au résultat final
        /// </summary>
        /// <param name="image">Tableau de bytes obtenu avec le ReadAllBytes</param>
        /// <param name="nombreOctets">Nombre d'octets sur lequel l'entier est codé</param>
        /// <param name="indiceDepart">Indice du byte du tableau Image à partir duquel le nombre est codé</param>
        /// <returns>Le même nombre mais entier</returns>
        public int Convertir_Endian_To_Int(byte[] image, int nombreOctets, int indiceDepart)
        {
            int returned = 0;
            for (int x = 0; x < nombreOctets; x++)
            {
                //NE PAS OUBLIER LA CONVERSION DE LA PUISSANCE EN INT, cela a été source de nombreux problèmes
                returned += Convert.ToInt32(image[indiceDepart + x] * (int)Math.Pow(256, x));
            }
            return returned;
        }

        /// <summary>
        /// Ici, on va diviser l'entier entré par chaque puissance de 256,
        /// de la puissance 'nombreOctets -1' à 0, attribuer le quotient au byte correspondant
        /// dans le tableau de bytes créé, et répéter l'opération avec le reste de la division
        /// </summary>
        /// <param name="val">Entier à convertir</param>
        /// <param name="nombreOctets">Nombre d'octets sur lequel on code la valeur</param>
        /// <returns>Le même nombre sous forme de tableau de bytes</returns>
        public byte[] Convertir_Int_To_Endian(int val, int nombreOctets)
        {
            byte[] returned = new byte[nombreOctets];
            for (int x = nombreOctets - 1; x >= 0; x--)
            {
                int puissance = (int)Math.Pow(256, x);
                returned[x] = Convert.ToByte(val / puissance);
                val = (int)(val % puissance);
            }
            return returned;
        }
        #endregion

        #region TD3
        /// <summary>
        /// On va simplement passer des coordonnées carthésiennes en coordonnées polaires
        /// grâce aux formules de cours, en faisant attention à définir les coordonnées carthésiennes
        /// en fonction du centre de l'image
        /// </summary>
        /// <param name="x">Coordonnée en x en prenant le centre de l'image pour (0,0)</param>
        /// <param name="y">Idem mais coordonnées en y</param>
        /// <returns>Un tableau contenant les coordonnées polaires (r et theta)</returns>
        public double[] CarthesiennesEnPolaire(int x, int y)
        {
            double[] returned = new double[2];
            returned[0] = Math.Sqrt(x * x + y * y);
            returned[1] = Math.Atan2(y, x);
            return returned;
        }

        /// <summary>
        /// On va faire l'exact inverse de la fonction précédente, soit tranformer les coordonnées
        /// polaires en coordonnées carthésiennes PAR RAPPORT AU CENTRE DE L'IMAGE
        /// </summary>
        /// <param name="r">Distance par rapport au centre de l'image</param>
        /// <param name="theta">Angle par rapport à l'axe x</param>
        /// <returns>Les deux coordonnées carthésiennes dans un tableau</returns>
        public int[] PolaireEnCarthesiennes(double r, double theta)
        {
            int[] returned = new int[2];
            returned[0] = (int)(r * Math.Cos(theta));
            returned[1] = (int)(r * Math.Sin(theta));
            return returned;
        }

        /// <summary>
        /// La fonction va s'exécuter en 4 parties :
        /// - elle va prendre les quatre coins de l'image et les passer en coordonnées polaires
        /// - puis elle va appliquer la rotation demandée, et repasser en coordonnées polaires
        /// - ainsi on aura la dimension de l'image finale (quand l'image pivote, il y aura des bandes monochromes)
        /// - pour enfin faire correspondre à chaque pixel de la nouvelle image un pixel de l'image d'origine ou du vert dans notre cas
        /// si le pixel n'appartenait pas à l'image (on procède de cette manière pour éviter les trous dans l'image dus aux arrondis)
        /// </summary>
        /// <param name="angle">Angle par lequel on veut faire pivoter l'image</param>
        public void Rotate(double angle)
        {
            //On récupère donc les 4 angles (qu'on stocke dans un double tableau)
            double[][] extremites = new double[4][];
            int[] centreImageDeBase = new int[2] { hauteurImage / 2, largeurImage / 2 };
            extremites[0] = CarthesiennesEnPolaire((-hauteurImage / 2), (-largeurImage / 2));
            extremites[1] = CarthesiennesEnPolaire((-hauteurImage / 2), (largeurImage / 2));
            extremites[2] = CarthesiennesEnPolaire((hauteurImage / 2), (-largeurImage / 2));
            extremites[3] = CarthesiennesEnPolaire((hauteurImage / 2), (largeurImage / 2));
            foreach (double[] table in extremites)
            {
                table[1] += angle;
            }

            //Que l'on passe en coordonnées polaires
            int[][] extremitesImageTournee = new int[4][];
            extremitesImageTournee[0] = PolaireEnCarthesiennes(extremites[0][0], extremites[0][1]);
            extremitesImageTournee[1] = PolaireEnCarthesiennes(extremites[1][0], extremites[1][1]);
            extremitesImageTournee[2] = PolaireEnCarthesiennes(extremites[2][0], extremites[2][1]);
            extremitesImageTournee[3] = PolaireEnCarthesiennes(extremites[3][0], extremites[3][1]);

            //Puis on détermine la hauteur et la largeur de l'image pivotée
            int maxX = 0;
            int minX = 0;
            int maxY = 0;
            int minY = 0;
            foreach (int[] element in extremitesImageTournee)
            {
                maxX = Math.Max(maxX, element[0]);
                minX = Math.Min(minX, element[0]);
                maxY = Math.Max(maxY, element[1]);
                minY = Math.Min(minY, element[1]);
            }
            hauteurImage = maxX - minX;
            largeurImage = maxY - minY;

            //Et ici, on récupère le pixel correspondant pour chaque pixel de l'image pivotée
            Pixel[,] nouvelleImage = new Pixel[hauteurImage, largeurImage];
            int[] centreImageTournee = new int[2] { hauteurImage / 2, largeurImage / 2 };
            for (int x = 0; x < hauteurImage; x++)
            {
                for (int y = 0; y < largeurImage; y++)
                {
                    double[] pixelEtudie = CarthesiennesEnPolaire(x - centreImageTournee[0], y - centreImageTournee[1]);
                    int[] pixelCorrespondant = PolaireEnCarthesiennes(pixelEtudie[0], pixelEtudie[1] - angle);
                    pixelCorrespondant[0] += centreImageDeBase[0];
                    pixelCorrespondant[1] += centreImageDeBase[1];

                    //Le pixel est vert flash de base, et s'il appartient bien à l'image d'origine, on lui attribue la valeur dudit pixel
                    Pixel nouveauPixel = new Pixel(0, 128, 0);
                    if (pixelCorrespondant[0] >= 0 && pixelCorrespondant[0] < pixels.GetLength(0) && pixelCorrespondant[1] >= 0 && pixelCorrespondant[1] < pixels.GetLength(1))
                    {
                        nouveauPixel = pixels[pixelCorrespondant[0], pixelCorrespondant[1]];
                    }
                    nouvelleImage[x, y] = nouveauPixel;
                }
            }
            pixels = nouvelleImage; //Et on définit l'image pivotée en tant qu'image de base...

            //... en modifiant également la taille du fichier en prenant en compte les compléments à 4
            tailleDuFichier = (largeurImage * hauteurImage * 3) + (largeurImage % 4 * hauteurImage) + tailleOffset;
        }

        /// <summary>
        /// Cette fonction va juste appliquer une symétrie axiale verticale à l'image pour l'afficher comme dans un miroir
        /// </summary>
        public void Miroir()
        {
            Pixel[,] newImage = new Pixel[hauteurImage, largeurImage]; //nouvelle matrice de Pixel qui va être notre nouvelle image
            for (int l = 0; l < pixels.GetLength(0); l++)
            {
                for (int c = 0; c < pixels.GetLength(1); c++)
                {
                    newImage[l, c] = pixels[l, largeurImage - c - 1]; //on prend chaque Pixel dans la nouvelle image et on le met à la (largeur_image - i-ème colonne)
                }
            }
            pixels = newImage; //on définit notre nouvelle matrice de Pixel comme nouvelle image
        }

        /// <summary>
        /// Dans cette fonction, on va simplement créer une image de la bonne taille, selon le coefficient,
        /// puis chercher le pixel correspondant dans l'image de base
        /// </summary>
        /// <param name="coeff">Coefficient d'aggrandissement ou de réduction</param>
        public void ChgmtTaille(double coeff)
        {
            int new_largeur = (int)(coeff * largeurImage);
            int new_hauteur = (int)(coeff * hauteurImage);
            Pixel[,] nouvelleImage = new Pixel[new_hauteur, new_largeur];
            for(int i = 0; i < new_hauteur; i++)
            {
                for(int j = 0; j < new_largeur; j++)
                {
                    int new_i = (int)(i / coeff);
                    int new_j = (int)(j / coeff);
                    nouvelleImage[i, j] = new Pixel(pixels[new_i, new_j].Red, pixels[new_i, new_j].Green, pixels[new_i, new_j].Blue);
                }
            }
            tailleDuFichier = (new_largeur * new_hauteur * 3) + (new_largeur % 4 * new_hauteur) + tailleOffset; //on définit la nouvelle taille du fichier
            largeurImage = new_largeur; //les nouvelles dimensions
            hauteurImage = new_hauteur;
            pixels = nouvelleImage;  //et la nouvelle matrice de pixel
        }

        /// <summary>
        /// Pour faire passer l'image en nuances de gris,
        /// on applique simplement sur chaque couleur de chaque Pixel
        /// la moyenne des valeurs des couleurs du Pixel en question
        /// </summary>
        public void NuancesDeGris()
        {
            for (int x = 0; x < pixels.GetLength(0); x++)
            {
                for (int y = 0; y < pixels.GetLength(1); y++)
                {
                    int rvbGRIS = (pixels[x, y].Red + pixels[x, y].Green + pixels[x, y].Blue) / 3;
                    Pixel newPixel = new Pixel(rvbGRIS, rvbGRIS, rvbGRIS);
                    pixels[x, y] = newPixel;
                }
            }
        }

        /// <summary>
        /// Très similaire à la fonction NuancesDeGris, mais ici on rendra le pixel noir uniquement
        /// si la moyenne des trois couleurs est inférieure strictement à 128 (valeur médiane de 0-255)
        /// et blanc si elle est supérieure à 128
        /// </summary>
        public void NoirEtBlanc()
        {
            for (int x = 0; x < pixels.GetLength(0); x++)
            {
                for (int y = 0; y < pixels.GetLength(1); y++)
                {
                    int moyenne = (pixels[x, y].Red + pixels[x, y].Green + pixels[x, y].Blue) / 3;
                    if (moyenne < 128)
                    {
                        Pixel newPixel = new Pixel(0, 0, 0);
                        pixels[x, y] = newPixel;
                    }
                    else
                    {
                        Pixel newPixel = new Pixel(255, 255, 255);
                        pixels[x, y] = newPixel;
                    }
                }
            }
        }
        #endregion

        #region TD4
        /// <summary>
        /// On va tester les quatre cas problématiques, et renvoyer les coordonnées fonctionnelles
        /// (on part donc du principe que l'image est continue et que les bords sont reliés entre eux)
        /// </summary>
        /// <param name="x">Coordonnée à vérifier en ligne</param>
        /// <param name="y">Coordonnée à vérifier en colonne</param>
        /// <returns>Un tableau de taille 2 contenant les deux coordonnées</returns>
        public int[] NouvellesCoordonnées(int x, int y)
        {
            int[] returned = new int[2] { x, y };
            returned[0] = (x < 0) ? (pixels.GetLength(0) + x) : returned[0];
            returned[0] = (x >= pixels.GetLength(0)) ? (x - pixels.GetLength(0)) : returned[0];
            returned[1] = (y < 0) ? (pixels.GetLength(1) + y) : returned[1];
            returned[1] = (y >= pixels.GetLength(1)) ? (y - pixels.GetLength(1)) : returned[1];
            return returned;
        }

        /// <summary>
        /// On va donc appliquer la matrice de convolution à chaque pixel de l'image
        /// (en utilisant la fonction NouvellesCoordonnées, on s'assure de ne pas avoir de problème
        /// d'index), puis on applique le coefficient dans le cas du flou, pour enfin ramener toutes
        /// les valeurs des pixels à 0 si elles sont négatives et à 255 si elles sont supérieures à 256
        /// </summary>
        /// <param name="convolution">Matrice de convolution correspondant au filtre choisi</param>
        /// <param name="coeffInverse">Vaut 1 par défaut, utilisé seulement pour le flou</param>
        public void Filtres(int[,] convolution, int coeffInverse)
        {
            int decalage = convolution.GetLength(0) / 2;
            Pixel[,] nouvelleImage = new Pixel[hauteurImage, largeurImage];
            for (int x = 0; x < hauteurImage; x++)
            {
                for (int y = 0; y < largeurImage; y++)
                {
                    Pixel nouveauPixel = new Pixel(0, 0, 0);
                    for (int i = x - decalage; i <= x + decalage; i++)
                    {
                        for (int j = y - decalage; j <= y + decalage; j++)
                        {
                            int[] newCos = NouvellesCoordonnées(i, j);
                            nouveauPixel.Red += convolution[i + decalage - x, j + decalage - y] * pixels[newCos[0], newCos[1]].Red;
                            nouveauPixel.Green += convolution[i + decalage - x, j + decalage - y] * pixels[newCos[0], newCos[1]].Green;
                            nouveauPixel.Blue += convolution[i + decalage - x, j + decalage - y] * pixels[newCos[0], newCos[1]].Blue;
                        }
                    }
                    if (coeffInverse != 1)
                    {
                        nouveauPixel.Red /= coeffInverse;
                        nouveauPixel.Green /= coeffInverse;
                        nouveauPixel.Blue /= coeffInverse;
                    }
                    nouveauPixel.Red = (nouveauPixel.Red < 0) ? (nouveauPixel.Red = 0) : nouveauPixel.Red;
                    nouveauPixel.Red = (nouveauPixel.Red > 255) ? (nouveauPixel.Red = 255) : nouveauPixel.Red;
                    nouveauPixel.Green = (nouveauPixel.Green < 0) ? (nouveauPixel.Green = 0) : nouveauPixel.Green;
                    nouveauPixel.Green = (nouveauPixel.Green > 255) ? (nouveauPixel.Green = 255) : nouveauPixel.Green;
                    nouveauPixel.Blue = (nouveauPixel.Blue < 0) ? (nouveauPixel.Blue = 0) : nouveauPixel.Blue;
                    nouveauPixel.Blue = (nouveauPixel.Blue > 255) ? (nouveauPixel.Blue = 255) : nouveauPixel.Blue;
                    nouvelleImage[x, y] = nouveauPixel;
                }
            }
            pixels = nouvelleImage;
        }

        #endregion

        #region TD5
        /// <summary>
        /// Un second constructeur qui ne prend pas un "fichier" bmp en paramètre mais une matrice de pixel
        /// Elle est utile pour les histogrammes et la cryptographie car ici on construit une nouvelle image par rapport à une autre
        /// On remplit donc les variables d'instances par rapport à cette matrice
        /// En prenant par défaut un fichier type "BM", une taille d'offset de 54 et le nombre de byte par couleur de 24
        /// </summary>
        /// <param name="mat"></param>
        public MyImage(Pixel[,] mat)
        {
            typeImage = "BM";
            tailleDuFichier = mat.GetLength(0) * mat.GetLength(1) * 3 + (mat.GetLength(1) % 4 * mat.GetLength(0)) + 54;
            tailleOffset = 54;
            largeurImage = mat.GetLength(1);
            hauteurImage = mat.GetLength(0);
            nombreBytesParCouleur = 24;
            pixels = mat;
        }

        /// <summary>
        /// Méthode qui réalise l'histogramme de nuance de gris, de rouge, de vert ou de bleu au choix (choisi par l'utilisateur avant)
        /// en fonction du choix on va attribuer des coefficients multiplicateurs 
        /// les premiers coefficient coefR, coefG et coefB servent pour l'image en question et les autres servent à construire l'histogramme
        /// on va ensuite utiliser un tableau de 256 valeurs ou l'on va ranger le nombre de pixel rouge, vert, bleu ou gris en fonction du choix
        /// le pic le plus haut correspondra à la couleur la teinte de couleur la plus présente dans l'image
        /// et les autres seront calculé avec un rapport par rapport à la taille de l'histogramme fixe et le pic le plus haut
        /// </summary>
        /// <param name="type">permet de savoir si on fait un histogramme de gris, de rouge, vert, ou bleu</param>
        /// <returns>on retourne à la fin l'image de l'histogramme du type objet MyImage</returns>
        public MyImage Histogramme(int type)
        {
            double coefR = 0;
            double coefG = 0;
            double coefB = 0;
            int R = 0;
            int G = 0;
            int B = 0;
            switch (type)
            {
                case 1: //pour un histogramme de gris
                    coefR = 0.2126;
                    coefG = 0.7152;
                    coefB = 0.0722;
                    R = 1;
                    G = 1;
                    B = 1;
                    break;
                case 2: //pour un histogramme rouge
                    coefR = 1;
                    R = 1;
                    break;
                case 3: //pour un histogramme vert
                    coefG = 1;
                    G = 1;
                    break;
                case 4: //pour un histogramme bleu
                    coefB = 1;
                    B = 1;
                    break;
            }
            int[] nuance = new int[256]; //la tableau pour ranger le nombre de pixel par teinte de couleur
            int temp = 0;
            for (int l = 0; l < pixels.GetLength(0); l++)
            {
                for (int c = 0; c < pixels.GetLength(1); c++)
                {
                    temp = Convert.ToInt32(Math.Round(coefR * pixels[l, c].Red + coefG * pixels[l, c].Green + coefB * pixels[l, c].Blue)); //on multiplie chaque couleur du pixel avec son coefficient
                    nuance[temp]++; //on incrémente le nombre de pixel correspondant au calcul précédent
                }
            }
            int max = -1;
            for (int i = 0; i < 256; i++) //on recherche le max pour pouvoir calculer le rapport pour remplir correctement l'histogramme
            {
                if (nuance[i] > max)
                {
                    max = nuance[i];
                }
            }
            Pixel[,] mat = new Pixel[910, 512]; //la matrice de pixel correspondant à l'histogramme de taille fixe
            int indice = 0;
            int fond = 0; // correspond à la couleur du fond de l'histogramme
            if (type == 1)
            {
                fond = 200; //on met la couleur de fond en (0,200,0) pour l'histogramme de gris pour une meilleure lisibilité
            }
            for (int c = 0; c < mat.GetLength(1); c += 2)
            {
                int taille = Convert.ToInt32(Math.Floor(((double)nuance[indice] / (double)max) * (double)mat.GetLength(0)));
                //on calcule la hauteur du pic de la n-ième naunce avec le rapport nuance[i]/nuance[max] * la hauteur de la matrice
                if (taille == mat.GetLength(0))
                {
                    taille--; //si on est dans le cas ou on a la nuance max, le rapport vaut 1 donc taille vaudra la hauteur de la matrice + 1, il faut donc le décrémenté d'un pour ne pas dépassé
                }
                for (int i = 0; i <= taille; i++) //on va du bas de l'histogramme jusqu'au sommet du pic
                {
                    for (int j = 0; j < 2; j++) //on le fait sur une largeur de 2 (car la largeur vaut 512, or 512/2 = 256 soit le nombre de nuance)
                    {
                        mat[i, c + j] = new Pixel(R * (c / 2), G * (c / 2), B * (c / 2)); //en utilisant les coefficients de couleurs défini au début, on ajoute des pixels de haut en bas avec la couleur correspondant au type d'histogramme
                    }
                }
                indice++;
                for (int i = taille + 1; i < mat.GetLength(0); i++) //on va de la case + 1 du sommet du pic jusqu'au sommet de l'histogramme
                {
                    for (int j = 0; j < 2; j++)
                    {
                        mat[i, c + j] = new Pixel(0, fond, 0); //on compléte l'écart entre le sommet du pic et le sommet de l'histogramme par des pixels noirs pour les histogrammes rouge, vert ou bleu, ou des pixels vert pour la nuance de gris
                    }
                }
            }
            MyImage image = new MyImage(mat); //on construit l'image avec le constructeur approprié
            return image;
        }

        /// <summary>
        /// Fonctionnement simple, on construit dans un premier temps les histogrammes rouge vert et bleu de l'image avec la méthode Histogramme
        /// ensuite on assemble les trois histogrammes en prenant la composante rouge de l'histogramme et respectivement la même chose pour les autres
        /// afin de former les pixels de l'histogramme RGB
        /// </summary>
        /// <returns>on retourne à la fin l'image de l'histogramme RGB du type objet MyImage</returns>
        public MyImage HistogrammeRGB()
        {
            MyImage Rouge = Histogramme(2); //on réalise les trois histogrammes rouge, vert et bleu
            MyImage Vert = Histogramme(3);
            MyImage Bleu = Histogramme(4);
            Pixel[,] mat = new Pixel[Rouge.pixels.GetLength(0), Rouge.pixels.GetLength(1)]; //on créé la matrice de pixel de même taille que les histogrammes
            for (int l = 0; l < Rouge.pixels.GetLength(0); l++)
            {
                for (int c = 0; c < Rouge.pixels.GetLength(1); c++)
                {
                    mat[l, c] = new Pixel(Rouge.pixels[l, c].Red, Vert.pixels[l, c].Green, Bleu.pixels[l, c].Blue);
                    //on assemble chaque pixel de l'histogramme RGB en prenant chaque pixel respectif des trois histogrammes
                }
            }
            return new MyImage(mat);
        }

        /// <summary>
        /// fonction simple qui prend en paramètre un entier correspondant à un nombre binaire puis le converti en base 10
        /// </summary>
        /// <param name="nombre">nombre binaire</param>
        /// <returns>on retourne un entier correspondant à la conversion en base 10 de nombre</returns>
        static int Binaire_to_Decimal(int nombre)
        {
            char[] tab = Convert.ToString(nombre).ToCharArray();
            Array.Reverse(tab);
            int returned = 0;
            for (int i = 0; i < tab.Length; i++)
            {
                if (tab[i] == '1')
                {
                    if (i == 0)
                    {
                        returned += 1;
                    }
                    else
                    {
                        returned += (int)Math.Pow(2, i);
                    }
                }
            }
            return returned;
        }

        /// <summary>
        /// fonction simple qui prend en paramètre un entier en base 10 et le converti en binaire puis le retourne en chaine de caractère
        /// la chaine de caractère est juste utile pour la méthode cryptographie rien de plus
        /// </summary>
        /// <param name="nombre">entier en base 10</param>
        /// <returns>retourne la conversion de nombre en binaire en chaine de caractère</returns>
        static string Decimale_to_Binaire(int nombre)
        {
            string bin = "";
            while (nombre != 0)
            {
                bin += Convert.ToInt32(nombre % 2);
                nombre = Convert.ToInt32(Math.Floor((double)nombre / 2));
            }
            string returned = "";
            for (int i = 0; i < bin.Length; i++)
            {
                returned += bin[bin.Length - i - 1];
            }
            return returned;
        }

        /// <summary>
        /// Fonction utile pour la cryptographie pour éviter de cryptographier 2 images de tailles différentes
        /// on prend en paramètre une matrice de pixel dans une certaines et on aimerait réajuster sa hauteur, sa largeur ou bien les deux
        /// on va donc créer une nouvelle matrice de pixel dans la nouvelle taille et "placer au milieu l'image"
        /// on va remplir ensuite les contours de l'image par des pixels noirs
        /// </summary>
        /// <param name="mat">la matrice de pixel à changer de taille</param>
        /// <param name="hauteur">nouvelle hauteur souhaité</param>
        /// <param name="largeur">nouvelle largeur souhaité</param>
        /// <returns>on retourne la matrice de pixel dans les bonnes dimensions</returns>
        static Pixel[,] Resize(Pixel[,] mat, int hauteur, int largeur)
        {
            Pixel[,] newmat = new Pixel[hauteur, largeur]; //la nouvelle matrice de pixel qui correspond à la nouvelle image dans les bonnes dimensions
            int ecart_largeur = largeur - mat.GetLength(1); //on calcul le nombre de nouvelles cases en largeur puis en hauteur
            int ecart_hauteur = hauteur - mat.GetLength(0);
            ecart_hauteur = Convert.ToInt32(Math.Floor((double)ecart_hauteur / 2));
            ecart_largeur = Convert.ToInt32(Math.Floor((double)ecart_largeur / 2));
            //on divise par deux les écarts pour obtenir la position centrale de l'image sur la nouvelle image de pixel
            for (int l = 0; l < mat.GetLength(0); l++)
            {
                for (int c = 0; c < mat.GetLength(1); c++)
                {
                    newmat[l + ecart_hauteur, c + ecart_largeur] = mat[l, c];
                    //ensuite on place l'image au milieu de la nouvelle matrice de pixel d'ou le (l + ecart_hauteur et c + ecart_largeur)
                }
            }
            for (int l = 0; l < newmat.GetLength(0); l++)
            {
                for (int c = 0; c < newmat.GetLength(1); c++)
                {
                    if (newmat[l, c] == null) //ensuite on remplit toutes les cases vides de la nouvelle matrice de pixel par des pixels noirs
                    {
                        newmat[l, c] = new Pixel(0, 0, 0);
                    }
                }
            }
            return newmat;
        }

        /// <summary>
        /// Cette méthode est utile pour la décryptographie
        /// en effet en cryptant une image dans une autre avec notre algorithme 
        /// on est obligé de changer la taille des images en remplissants leur contour par des pixels noirs afin d'avoir des images de même taille
        /// Nous avons choisi de remplir par des pixels noirs donc ici on supprime que les bords noirs, la méthode ne fonctionne pas avec une autre couleur
        /// Dans la méthode, on va regarder si chaque ligne ou colonne ne comporte que pixels noirs dans ce cas on supprime cette ligne ou cette colonne
        /// Il est vrai que si l'image comporte une ligne ou une colonne noir, l'algorithme peut crash, mais la probabilité est faible
        /// </summary>
        /// <param name="mat">la matrice de pixel dont on souhaite supprimer les bords noirs</param>
        /// <returns>on retourne la nouvelle matrice de pixel sans les bords noirs</returns>
        static Pixel[,] Supp_bords_noir(Pixel[,] mat)
        {
            int[] hauteur = new int[2]; //le tableau de hauteur qui va comprendre en première case la coordonnée de la première ligne de l'image et la dernière ligne de l'image pour la deuxième case
            hauteur[0] = -1;
            bool test_hauteur = false;
            for (int l = 0; l < mat.GetLength(0); l++)
            {
                for (int c = 0; c < mat.GetLength(1); c++)
                {
                    if (mat[l, c].Test_pixel_noir() == false) //si le pixel n'est pas noir
                    {
                        test_hauteur = true; //le test est vrai
                    }
                }
                if (test_hauteur == true && hauteur[0] == -1) //si le test précédent est vrai et qu'on a pas encore modifier la première coordonnée
                {
                    hauteur[0] = l; //on enregistre la première ligne de l'image dans la première case du tableau hauteur
                }
                else if (test_hauteur == true && hauteur[0] != -1) //si le test précédent est vrai et qu'on a déjà modifier la première coordonnée
                {
                    hauteur[1] = l + 1; //on enregistre la dernière ligne + 1 de l'image
                }
                test_hauteur = false; // on remet le test en false
            }
            int[] largeur = new int[2]; //même principe mais pour la largeur, donc les colonnes
            largeur[0] = -1;
            bool test_largeur = false;
            for (int c = 0; c < mat.GetLength(1); c++)
            {
                for (int l = 0; l < mat.GetLength(0); l++)
                {
                    if (mat[l, c].Test_pixel_noir() == false)
                    {
                        test_largeur = true;
                    }
                }
                if (test_largeur == true && largeur[0] == -1)
                {
                    largeur[0] = c;
                }
                else if (test_largeur == true && largeur[0] != -1)
                {
                    largeur[1] = c + 1;
                }
                test_largeur = false;
            }
            Pixel[,] returned = new Pixel[hauteur[1] - hauteur[0], largeur[1] - largeur[0]];
            //la nouvelle matrice de pixel correspondant à la taille de l'image sans les bords (donc dernière ligne - première ligne et dernière colonne - première colonne)
            for (int l = 0; l < hauteur[1] - hauteur[0]; l++)
            {
                for (int c = 0; c < largeur[1] - largeur[0]; c++)
                {
                    returned[l, c] = mat[hauteur[0] + l, largeur[0] + c]; //on remplit la nouvelle image sans les bord noirs (d'où le + hauteur[0] et le + largeur[0])
                }
            }
            return returned;
        }

        /// <summary>
        /// On prend les deux matrices pixel de chaque image, on identifie laquelle à la plus grand lageur ou la plus grande hauteur
        /// Si besoin on change les tailles des images avec la fonction Resize pour avoir deux images de même taille
        /// Ensuite on va convertir chaque pixel de chaque image en binaire, puis on va prendre les 4 bits de poids fort dans chaque composante RGB d'un pixel
        /// afin de les assembler en 8 bits (donc un octet) dans la nouvelle image pour chaque composante RGB d'un pixel
        /// </summary>
        /// <param name="image">la seconde image à cryptographier</param>
        /// <returns>retourne l'image crytographiée</returns>
        public MyImage Cryptographie(MyImage image)
        {
            Pixel[,] mat1 = pixels; //les deux matrices pixel de chaque image
            Pixel[,] mat2 = image.pixels;
            Pixel[,] newmat = new Pixel[hauteurImage, largeurImage]; //la matrice de la nouvelle image
            if (hauteurImage != image.pixels.GetLength(0) || largeurImage != image.pixels.GetLength(1)) //ici on teste si les deux images sont de même taille
            {
                int max_hauteur = hauteurImage; //ensuite on recherche la hauteur max et la largeur max
                if (hauteurImage > image.pixels.GetLength(0))
                {
                    max_hauteur = hauteurImage;
                }
                else if (hauteurImage < image.pixels.GetLength(0))
                {
                    max_hauteur = image.pixels.GetLength(0);
                }
                int max_largeur = largeurImage;
                if (largeurImage > image.pixels.GetLength(1))
                {
                    max_largeur = largeurImage;
                }
                else if (largeurImage < image.pixels.GetLength(1))
                {
                    max_largeur = image.pixels.GetLength(1);
                }
                if (max_hauteur != mat1.GetLength(0) || max_largeur != mat1.GetLength(1))
                {
                    mat1 = Resize(mat1, max_hauteur, max_largeur);
                }
                if (max_hauteur != mat2.GetLength(0) || max_largeur != mat2.GetLength(1))
                {
                    mat2 = Resize(mat2, max_hauteur, max_largeur);
                }
                newmat = new Pixel[max_hauteur, max_largeur]; //on redefinit la taille de la nouvelle image
            }
            for (int l = 0; l < mat1.GetLength(0); l++)
            {
                for (int c = 0; c < mat2.GetLength(1); c++)
                {
                    //on converti chaque composante de chaque pixel en binaire pour les deux images
                    string binaire1_red = Resize_bits(8, Decimale_to_Binaire(mat1[l, c].Red));
                    string binaire1_green = Resize_bits(8, Decimale_to_Binaire(mat1[l, c].Green));
                    string binaire1_blue = Resize_bits(8, Decimale_to_Binaire(mat1[l, c].Blue));
                    string binaire2_red = Resize_bits(8, Decimale_to_Binaire(mat2[l, c].Red));
                    string binaire2_green = Resize_bits(8, Decimale_to_Binaire(mat2[l, c].Green));
                    string binaire2_blue = Resize_bits(8, Decimale_to_Binaire(mat2[l, c].Blue));
                    string byte_red = "";
                    string byte_green = "";
                    string byte_blue = "";
                    for (int i = 0; i < 4; i++) //ensuite on ajoute les 4 bits de poid fort de la première image pour chaque composante du pixel dans la nouvelle image
                    {
                        byte_red += binaire1_red[i];
                        byte_green += binaire1_green[i];
                        byte_blue += binaire1_blue[i];
                    }
                    for (int i = 0; i < 4; i++) //de même pour la deuxième image
                    {
                        byte_red += binaire2_red[i];
                        byte_green += binaire2_green[i];
                        byte_blue += binaire2_blue[i];
                    }
                    //ensuite on remet dans la matrice pixel de nouvelle image en convertissant en decimal
                    newmat[l, c] = new Pixel(Binaire_to_Decimal(Convert.ToInt32(byte_red)), Binaire_to_Decimal(Convert.ToInt32(byte_green)), Binaire_to_Decimal(Convert.ToInt32(byte_blue)));
                }
            }
            MyImage returned = new MyImage(newmat);
            return returned;
        }

        /// <summary>
        /// L'inverse de la cryptographie, ici on va sortir deux images d'une seule image
        /// On va convertir chaque composante RGB de chaque pixel en binaire
        /// puis on va prendre les 4 premiers bits et rajouter 4 0 à la fin pour chaque composante RGB du pixel pour la première image
        /// on fait de même pour les 4 derniers bits et on l'insère dans la deuxième image
        /// ensuite on vérifie si les "4 coins" des deux nouvelles images sont noir dans ce cas cette image comporte probablement des bords noirs
        /// ces bords noirs sont du à la cryptographie ajuste la taille des images en rajoutant des bords noirs
        /// et dans ce cas, on utilise la méthode Supp_bords_noir pour enlever les bords noirs
        /// </summary>
        /// <returns>ensuite on retourne un tableau de MyImage avec les deux nouvelles images</returns>
        public MyImage[] DeCryptographie()
        {
            Pixel[,] mat1 = new Pixel[hauteurImage, largeurImage]; //les deux nouvelles matrice de pixel pour les deux nouvelles images
            Pixel[,] mat2 = new Pixel[hauteurImage, largeurImage];
            for (int l = 0; l < hauteurImage; l++)
            {
                for (int c = 0; c < largeurImage; c++)
                {
                    string binaire_red = Resize_bits(8, Decimale_to_Binaire(pixels[l, c].Red)); //on converti en binaire chaque composante RGB du pixel
                    string binaire_green = Resize_bits(8, Decimale_to_Binaire(pixels[l, c].Green));
                    string binaire_blue = Resize_bits(8, Decimale_to_Binaire(pixels[l, c].Blue));
                    string byte1_red = "";
                    string byte1_green = "";
                    string byte1_blue = "";
                    string byte2_red = "";
                    string byte2_green = "";
                    string byte2_blue = "";
                    for (int i = 0; i < 4; i++) //on prend les 4 premiers bits binaire pour la première image et les 4 dernier pour la deuxième image
                    {
                        byte1_red += binaire_red[i];
                        byte1_green += binaire_green[i];
                        byte1_blue += binaire_blue[i];
                        byte2_red += binaire_red[i + 4];
                        byte2_green += binaire_green[i + 4];
                        byte2_blue += binaire_blue[i + 4];
                    }
                    for (int i = 4; i < 8; i++) //on compléte ensuite les 4 derniers bits par des 0
                    {
                        byte1_red += "0";
                        byte1_green += "0";
                        byte1_blue += "0";
                        byte2_red += "0";
                        byte2_green += "0";
                        byte2_blue += "0";
                    }
                    //on injecte dans les deux images en convertissant en base 10
                    mat1[l, c] = new Pixel(Binaire_to_Decimal(Convert.ToInt32(byte1_red)), Binaire_to_Decimal(Convert.ToInt32(byte1_green)), Binaire_to_Decimal(Convert.ToInt32(byte1_blue)));
                    mat2[l, c] = new Pixel(Binaire_to_Decimal(Convert.ToInt32(byte2_red)), Binaire_to_Decimal(Convert.ToInt32(byte2_green)), Binaire_to_Decimal(Convert.ToInt32(byte2_blue)));
                }
            }
            //on test pour les deux images si les "4 coins" sont des pixels noirs dans ce cas, on supprime les bords noirs des images avec la méthode Supp_bords_noir
            if (mat1[0, 0].Test_pixel_noir() == true && mat1[mat1.GetLength(0) - 1, 0].Test_pixel_noir() == true && mat1[0, mat1.GetLength(1) - 1].Test_pixel_noir() == true && mat1[mat1.GetLength(0) - 1, mat1.GetLength(1) - 1].Test_pixel_noir() == true)
            {
                mat1 = Supp_bords_noir(mat1);
            }
            if (mat2[0, 0].Test_pixel_noir() == true && mat2[mat2.GetLength(0) - 1, 0].Test_pixel_noir() == true && mat2[0, mat2.GetLength(1) - 1].Test_pixel_noir() == true && mat2[mat2.GetLength(0) - 1, mat2.GetLength(1) - 1].Test_pixel_noir() == true)
            {
                mat2 = Supp_bords_noir(mat2);
            }
            //on créé les deux images avec les matrices de pixel
            MyImage image1 = new MyImage(mat1);
            MyImage image2 = new MyImage(mat2);
            MyImage[] tab = new MyImage[2] { image1, image2 }; //on les range dans un tableau de MyImage de deux dimensions
            return tab;
        }
        /// <summary>
        /// Cette méthode permet de réaliser des fractales de Mandelbrot ou de Julia
        /// On choisit dans un premier temps les coordonnées des variables selon le type de fractales
        /// Les coordonnées sont pour le moment fixe, plus tard l'utilisateur pourra les modifier à sa guise
        /// Ensuite on définit la hauteur et la largeur de la matrice de pixel en fonction du zoom et des coordonnées
        /// Ensuite on déifinie chaque pixel selon la suite Zn+1 = Zn^2 + c
        /// </summary>
        /// <param name="iteration_max">Pour une fractale de Mandelbrot iteration_max = 50 et pour Julia iteration_max = 150</param>
        /// <param name="zoom">Permet de régler le zoom sur l'image selon le souhait de l'utilisateur</param>
        /// <returns></returns>
        public MyImage Fractales(int iteration_max, double zoom)
        {
            double xmin = 0;
            double xmax = 0;
            double ymin = 0;
            double ymax = 0;
            if (iteration_max == 50)
            {
                xmin = -2.1;
                xmax = 0.6;
                ymin = -1.2;
                ymax = 1.2;
            }
            else
            {
                xmin = -1;
                xmax = 1;
                ymin = -1.2;
                ymax = 1.2;
            }
            int hauteur = Convert.ToInt32((xmax - xmin) * zoom);
            int largeur = Convert.ToInt32((ymax - ymin) * zoom);
            Pixel[,] mat = new Pixel[hauteur, largeur];
            for (int x = 0; x < mat.GetLength(0); x++)
            {
                for (int y = 0; y < mat.GetLength(1); y++)
                {
                    double c_r = 0;
                    double c_i = 0;
                    double z_r = 0;
                    double z_i = 0;
                    if (iteration_max == 50)
                    {
                        c_r = x / zoom + xmin;
                        c_i = y / zoom + ymin;
                    }
                    else
                    {
                        z_r = x / zoom + xmin;
                        z_i = y / zoom + ymin;
                        c_r = 0.285;
                        c_i = 0.01;
                    }
                    int i = 0;
                    do
                    {
                        double tmp = z_r;
                        z_r = z_r * z_r - z_i * z_i + c_r;
                        z_i = 2 * z_i * tmp + c_i;
                        i++;
                    } while (Math.Sqrt(z_r * z_r + z_i * z_i) < 2 && i < iteration_max);
                    if (iteration_max == i)
                    {
                        mat[x, y] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        mat[x, y] = new Pixel(0, 0, i * 255 / iteration_max);
                    }
                }
            }
            return new MyImage(mat);
        }

        #endregion

        #region TD6
        /// <summary>
        /// fonction basique qui permet de rajouter des 0 ou pas pour faire une suite de bits à une taille souhaité
        /// </summary>
        /// <param name="nombre_bits">nombre de bits souhaité</param>
        /// <param name="binaire">chaine de bits binaire</param>
        /// <returns></returns>
        public string Resize_bits(int nombre_bits, string binaire)
        {
            if (binaire.Length < nombre_bits)
            {
                int longueur = binaire.Length;
                for (int i = 0; i < nombre_bits - longueur; i++)
                {
                    binaire = "0" + binaire;
                }
            }
            return binaire;
        }
        /// <summary>
        /// Fonction qui converti une suite de bits binaire en byte
        /// </summary>
        /// <param name="tab">tableau contenant les bits à convertir</param>
        /// <returns></returns>
        static byte ConvertirBinaireEnByte(int[] tab)
        {
            int temp = 0;
            for (int i = 0; i < tab.Length; i++)
            {
                temp += tab[i] * Convert.ToInt32(Math.Pow(2, tab.Length - 1 - i));
            }
            return Convert.ToByte(temp);
        }
        /// <summary>
        /// Fonction utile pour le masquage des données du QRCode
        /// On prend en paramètre la donnée du QRCode et le pixel correspondant sur le masque
        /// Puis la fonction choisi si le pixel retourné est noir ou blanc en fonction de la règle de masquage
        /// </summary>
        /// <param name="QRCode">donnée du QRCode soit '0' ou '1'</param>
        /// <param name="masquage">pixel du masque correspondant soit blanc ou noir</param>
        /// <returns></returns>
        public Pixel PixelQRCode(char QRCode, Pixel masquage)
        {
            Pixel returned;
            if (QRCode == '0' && masquage.Test_pixel_blanc() == true)
            {
                returned = new Pixel(255, 255, 255);
            }
            else if (QRCode == '0' && masquage.Test_pixel_noir() == true)
            {
                returned = new Pixel(0, 0, 0);
            }
            else if (QRCode == '1' && masquage.Test_pixel_blanc() == true)
            {
                returned = new Pixel(0, 0, 0);
            }
            else
            {
                returned = new Pixel(255, 255, 255);
            }
            return returned;
        }
        /// <summary>
        /// Fonction qui permet de créer un QRCode alaphanumérique version 1 ou 2
        /// Il prend en paramètre un message de maximum 47 caractères
        /// </summary>
        /// <param name="message">message a coder sur le qrcode</param>
        /// <returns></returns>
        public MyImage QRCode(string message)
        {
            Pixel[,] QRCode;
            int taille;
            Pixel[,] masquage;
            if (message.Length < 26) //version du qrcode 1
            {
                QRCode = new Pixel[21, 21];
                masquage = new Pixel[21, 21];
                taille = 152;
            }
            else //version du qrcode 2
            {
                QRCode = new Pixel[25, 25];
                masquage = new Pixel[25, 25];
                taille = 272;
            }
            string chainebinaire = "0010" + Resize_bits(9, Decimale_to_Binaire(message.Length));
            //le "0010" correspondant au mode alaphanumérique puis la longueur de la phrase sur 9 bits
            for (int i = 0; i < message.Length; i = i + 2)
            {
                string sous_chaine = "";
                if (message.Length - i != 1) //on divisile message par groupe de 2 caractères (ou 1 s'il en reste qu'un)
                {
                    sous_chaine = message.Substring(i, 2).ToLower();
                }
                else
                {
                    sous_chaine = message.Substring(i, 1).ToLower();
                }
                int[] valeur = new int[2];
                valeur[1] = -1;
                string binary = "";
                for (int j = 0; j < sous_chaine.Length; j++) //on convertit le caractère en alphanumérique
                {
                    char caractere = sous_chaine[j];
                    int entier = -1;
                    switch (caractere)
                    {
                        case '0':
                            entier = 0;
                            break;
                        case '1':
                            entier = 1;
                            break;
                        case '2':
                            entier = 2;
                            break;
                        case '3':
                            entier = 3;
                            break;
                        case '4':
                            entier = 4;
                            break;
                        case '5':
                            entier = 5;
                            break;
                        case '6':
                            entier = 6;
                            break;
                        case '7':
                            entier = 7;
                            break;
                        case '8':
                            entier = 8;
                            break;
                        case '9':
                            entier = 9;
                            break;
                        case 'a':
                            entier = 10;
                            break;
                        case 'b':
                            entier = 11;
                            break;
                        case 'c':
                            entier = 12;
                            break;
                        case 'd':
                            entier = 13;
                            break;
                        case 'e':
                            entier = 14;
                            break;
                        case 'f':
                            entier = 15;
                            break;
                        case 'g':
                            entier = 16;
                            break;
                        case 'h':
                            entier = 17;
                            break;
                        case 'i':
                            entier = 18;
                            break;
                        case 'j':
                            entier = 19;
                            break;
                        case 'k':
                            entier = 20;
                            break;
                        case 'l':
                            entier = 21;
                            break;
                        case 'm':
                            entier = 22;
                            break;
                        case 'n':
                            entier = 23;
                            break;
                        case 'o':
                            entier = 24;
                            break;
                        case 'p':
                            entier = 25;
                            break;
                        case 'q':
                            entier = 26;
                            break;
                        case 'r':
                            entier = 27;
                            break;
                        case 's':
                            entier = 28;
                            break;
                        case 't':
                            entier = 29;
                            break;
                        case 'u':
                            entier = 30;
                            break;
                        case 'v':
                            entier = 31;
                            break;
                        case 'w':
                            entier = 32;
                            break;
                        case 'x':
                            entier = 33;
                            break;
                        case 'y':
                            entier = 34;
                            break;
                        case 'z':
                            entier = 35;
                            break;
                        case ' ':
                            entier = 36;
                            break;
                        case '$':
                            entier = 37;
                            break;
                        case '%':
                            entier = 38;
                            break;
                        case '*':
                            entier = 39;
                            break;
                        case '+':
                            entier = 40;
                            break;
                        case '-':
                            entier = 41;
                            break;
                        case '.':
                            entier = 42;
                            break;
                        case '/':
                            entier = 43;
                            break;
                        case ':':
                            entier = 44;
                            break;
                    }
                    valeur[j] = entier;
                }
                if (valeur[1] != -1) //on créer la chaine de 11 bits s'il y a 2 caractères
                {
                    binary = Resize_bits(11, Decimale_to_Binaire(45 * valeur[0] + valeur[1]));
                }
                else //ou de 6 bits s'il y en a qu'un
                {
                    binary = Resize_bits(6, Decimale_to_Binaire(valeur[0]));
                }
                chainebinaire += binary;
            }
            if (chainebinaire.Length <= taille - 4) //on ajoute la terminaison de 4 "0" maximum ou moins pour ne pas dépasser la taille du QRCode
            {
                for (int i = 0; i < 4; i++)
                {
                    chainebinaire += "0";
                }
            }
            else
            {
                int taille_chaine = chainebinaire.Length;
                for (int i = 0; i < taille - taille_chaine; i++)
                {
                    chainebinaire += "0";
                }
            }
            while (chainebinaire.Length % 8 != 0) //on rajoute des "0" le temps que la chaine de bits n'est pas multiple de 8
            {
                chainebinaire += "0";
            }
            int compteur = 0;
            while (chainebinaire.Length < taille) //on rajoute des bits de padding si le taille de la chaine de bits n'est toujours pas assez grande
            {
                if (compteur % 2 == 0)
                {
                    chainebinaire += "11101100";
                }
                else
                {
                    chainebinaire += "00010001";
                }
                compteur++;
            }
            byte[] chaine_byte = new byte[chainebinaire.Length / 8];
            string temp = "";
            int[] chaine_entier = new int[8];
            int indice = 0;
            for (int i = 0; i < chainebinaire.Length; i += 8) //on va diviser la chaine de bits en octet pour la convertir en byte
            {
                temp = chainebinaire.Substring(i, 8);
                for (int l = 0; l < temp.Length; l++)
                {
                    chaine_entier[l] = Convert.ToInt32(Convert.ToString(temp[l]));
                }
                chaine_byte[i / 8] = ConvertirBinaireEnByte(chaine_entier);
            }
            byte[] error_solomon = null;
            if (QRCode.GetLength(0) == 21) //version 1 on génére le code erreur reed solomon sur 7 octets avec la chaine byte précédemment créé
            {
                error_solomon = ReedSolomonAlgorithm.Encode(chaine_byte, 7, ErrorCorrectionCodeType.QRCode);
            }
            else //la même chose pour la version 2 sur 10 octets
            {
                error_solomon = ReedSolomonAlgorithm.Encode(chaine_byte, 10, ErrorCorrectionCodeType.QRCode);
            }
            string error_solomon_binaire = "";
            for (int i = 0; i < error_solomon.Length; i++) //on convertit le code erreur solomon en octet
            {
                error_solomon_binaire += Resize_bits(8, Convert.ToString(Decimale_to_Binaire(Convert.ToInt32(error_solomon[i]))));
            }
            chainebinaire += error_solomon_binaire; //puis on l'ajoute à la suite de la chaine binaire
            for (int l = 0; l < masquage.GetLength(0); l++) //on créé le le masque 000
            {
                for (int c = 0; c < masquage.GetLength(1); c++)
                {
                    if ((l + c) % 2 == 0)
                    {
                        masquage[l, c] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        masquage[l, c] = new Pixel(255, 255, 255);
                    }
                }
            }
            //Partie dédié au remplissage des motifs d'alignements
            for (int i = 0; i < 7; i++) //premier contour noir
            {
                QRCode[0, i] = new Pixel(0, 0, 0);
                QRCode[i, 0] = new Pixel(0, 0, 0);
                QRCode[6, i] = new Pixel(0, 0, 0);
                QRCode[i, 6] = new Pixel(0, 0, 0);
                QRCode[QRCode.GetLength(0) - 1, i] = new Pixel(0, 0, 0);
                QRCode[QRCode.GetLength(0) - 7, i] = new Pixel(0, 0, 0);
                QRCode[QRCode.GetLength(0) - 1 - i, 0] = new Pixel(0, 0, 0);
                QRCode[QRCode.GetLength(0) - 1 - i, 6] = new Pixel(0, 0, 0);
                QRCode[QRCode.GetLength(0) - 1, QRCode.GetLength(1) - 1 - i] = new Pixel(0, 0, 0);
                QRCode[QRCode.GetLength(0) - 7, QRCode.GetLength(1) - 1 - i] = new Pixel(0, 0, 0);
                QRCode[QRCode.GetLength(0) - 1 - i, QRCode.GetLength(1) - 7] = new Pixel(0, 0, 0);
                QRCode[QRCode.GetLength(0) - 1 - i, QRCode.GetLength(1) - 1] = new Pixel(0, 0, 0);
            }
            for (int i = 0; i < 8; i++) //premier contour blanc
            {
                QRCode[7, i] = new Pixel(255, 255, 255);
                QRCode[i, 7] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 8, i] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 1 - i, 7] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 8, QRCode.GetLength(1) - 1 - i] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 1 - i, QRCode.GetLength(1) - 8] = new Pixel(255, 255, 255);
            }
            for (int i = 1; i < 6; i++) //deuxième contour blanc
            {
                QRCode[1, i] = new Pixel(255, 255, 255);
                QRCode[i, 1] = new Pixel(255, 255, 255);
                QRCode[5, i] = new Pixel(255, 255, 255);
                QRCode[i, 5] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 1 - i, 1] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 1 - i, 5] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 2, i] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 6, i] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 2, QRCode.GetLength(1) - 1 - i] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 6, QRCode.GetLength(1) - 1 - i] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - i - 1, QRCode.GetLength(1) - 2] = new Pixel(255, 255, 255);
                QRCode[QRCode.GetLength(0) - 1 - i, QRCode.GetLength(1) - 6] = new Pixel(255, 255, 255);
            }
            for (int l = 2; l < 5; l++) //interieur noir
            {
                for (int c = 2; c < 5; c++)
                {
                    QRCode[l, c] = new Pixel(0, 0, 0);
                    QRCode[QRCode.GetLength(0) - l - 1, c] = new Pixel(0, 0, 0);
                    QRCode[QRCode.GetLength(0) - l - 1, QRCode.GetLength(1) - c - 1] = new Pixel(0, 0, 0);
                }
            }
            QRCode[7, 8] = new Pixel(0, 0, 0);
            if (QRCode.GetLength(0) == 25) //ajout du motif d'alignement pour la version 2
            {
                for (int i = 0; i < 5; i++)
                {
                    QRCode[8 - i, 16] = new Pixel(0, 0, 0);
                    QRCode[8 - i, 20] = new Pixel(0, 0, 0);
                    QRCode[8, 16 + i] = new Pixel(0, 0, 0);
                    QRCode[4, 16 + i] = new Pixel(0, 0, 0);
                }
                for (int i = 0; i < 3; i++)
                {
                    QRCode[7, 17 + i] = new Pixel(255, 255, 255);
                    QRCode[5, 17 + i] = new Pixel(255, 255, 255);
                    QRCode[7 - i, 17] = new Pixel(255, 255, 255);
                    QRCode[7 - i, 19] = new Pixel(255, 255, 255);
                }
                QRCode[6, 18] = new Pixel(0, 0, 0);
                for (int i = 0; i < 9; i++)
                {
                    if (i % 2 == 0)
                    {
                        QRCode[8 + i, 6] = new Pixel(0, 0, 0);
                        QRCode[QRCode.GetLength(0) - 7, i + 8] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        QRCode[8 + i, 6] = new Pixel(255, 255, 255);
                        QRCode[QRCode.GetLength(0) - 7, i + 8] = new Pixel(255, 255, 255);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i % 2 == 0)
                    {
                        QRCode[8 + i, 6] = new Pixel(0, 0, 0);
                        QRCode[QRCode.GetLength(0) - 7, i + 8] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        QRCode[8 + i, 6] = new Pixel(255, 255, 255);
                        QRCode[QRCode.GetLength(0) - 7, i + 8] = new Pixel(255, 255, 255);
                    }
                }
            }
            //fin de la partie des motifs d'alignements
            //implémentation du code du masque dans le QR code
            string masque = "111011111000100";
            for (int i = 0; i < 7; i++)
            {
                if (masque[i] == '0')
                {
                    QRCode[i, 8] = new Pixel(255, 255, 255);
                }
                else
                {
                    QRCode[i, 8] = new Pixel(0, 0, 0);
                }
            }
            indice = 0;
            int indice_ligne = 0;
            while (indice != 7)
            {
                if (QRCode[QRCode.GetLength(0) - 8 + indice_ligne, 8] == null)
                {
                    if (masque[indice + 8] == '0')
                    {
                        QRCode[QRCode.GetLength(0) - 8 + indice_ligne, 8] = new Pixel(255, 255, 255);
                    }
                    else
                    {
                        QRCode[QRCode.GetLength(0) - 8 + indice_ligne, 8] = new Pixel(0, 0, 0);
                    }
                    indice++;
                }
                indice_ligne++;
            }
            indice = 0;
            int indice_colonne = 0;
            while (indice != 8)
            {
                if (QRCode[QRCode.GetLength(0) - 10, indice_colonne] == null)
                {
                    if (masque[indice] == '0')
                    {
                        QRCode[QRCode.GetLength(0) - 9, indice_colonne] = new Pixel(255, 255, 255);
                    }
                    else
                    {
                        QRCode[QRCode.GetLength(0) - 9, indice_colonne] = new Pixel(0, 0, 0);
                    }
                    indice++;
                }
                indice_colonne++;
            }
            for (int i = 0; i < 8; i++)
            {
                if (masque[7 + i] == '0')
                {
                    QRCode[QRCode.GetLength(0) - 9, QRCode.GetLength(1) - 8 + i] = new Pixel(255, 255, 255);
                }
                else
                {
                    QRCode[QRCode.GetLength(0) - 9, QRCode.GetLength(1) - 8 + i] = new Pixel(0, 0, 0);
                }
            }
            //pour la version 2 on rajoute des 0 à la fin de la chaine binaire si elle ne fait pas 359
            if (QRCode.GetLength(0) == 25)
            {
                while (chainebinaire.Length < 359)
                {
                    chainebinaire += "0";
                }
            }
            //partie pour le remplissage des données dans le QRCode en utilisant la chaine de bits binaire et le masque 000
            indice = 0;
            for (int c = QRCode.GetLength(1) - 1; c >= 0; c -= 4)
            {
                for (int l = 0; l < QRCode.GetLength(0); l++)
                {
                    if (QRCode[l, c] == null)
                    {
                        QRCode[l, c] = PixelQRCode(chainebinaire[indice], masquage[l, c]);
                        indice++;
                    }
                    if (c > 0 && QRCode[l, c - 1] == null)
                    {
                        QRCode[l, c - 1] = PixelQRCode(chainebinaire[indice], masquage[l, c - 1]);
                        indice++;
                    }
                }
                for (int l = QRCode.GetLength(0) - 1; l >= 0; l--)
                {
                    if (c > 1 && QRCode[l, c - 2] == null)
                    {
                        QRCode[l, c - 2] = PixelQRCode(chainebinaire[indice], masquage[l, c - 2]);
                        indice++;
                    }
                    if (c > 2 && QRCode[l, c - 3] == null)
                    {
                        QRCode[l, c - 3] = PixelQRCode(chainebinaire[indice], masquage[l, c - 3]);
                        indice++;
                    }
                }
            }
            MyImage image = new MyImage(QRCode); //on crée l'image du QRCode
            return image; //puis on retourne
        }
        /// <summary>
        /// Fonction qui est utilisé pour décodé un QRCode, c'est l'inverse de la méthode PixelQRCode_inv
        /// </summary>
        /// <param name="QRCode">Pixel correspondant au QRCode</param>
        /// <param name="masquage">Pixel correspondant au masque</param>
        /// <returns>Retourne le résultat de l'opérateur XOR en string entre les deux paramètres</returns>
        public string PixelQRCode_inv(Pixel QRCode, Pixel masquage)
        {
            string returned;
            if (QRCode.Test_pixel_blanc() == true && masquage.Test_pixel_blanc() == true)
            {
                returned = "0";
            }
            else if (QRCode.Test_pixel_blanc() == true && masquage.Test_pixel_noir() == true)
            {
                returned = "1";
            }
            else if (QRCode.Test_pixel_noir() == true && masquage.Test_pixel_blanc() == true)
            {
                returned = "1";
            }
            else
            {
                returned = "0";
            }
            return returned;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>retourne le message du QRCode</returns>
        public string QRCode_inv()
        {
            Miroir(); //on utilise la fonction miroir pour retourner QRCode (facilite grandement le traitement des données
            Pixel[,] masque = null;
            if (pixels.GetLength(0) == 21)
            {
                masque = new Pixel[21, 21];
            }
            else
            {
                masque = new Pixel[25, 25];
            }
            for (int l = 0; l < masque.GetLength(0); l++) //on créé le le masque 000
            {
                for (int c = 0; c < masque.GetLength(1); c++)
                {
                    if ((l + c) % 2 == 0)
                    {
                        masque[l, c] = new Pixel(0, 0, 0);
                    }
                    else
                    {
                        masque[l, c] = new Pixel(255, 255, 255);
                    }
                }
            }
            string chaine_bits = "";
            if (pixels.GetLength(0) == 21) //décodage des données pour le QRCode version 1
            {
                for (int c = 0; c < 5; c += 4)
                {
                    for (int l = 0; l < 12; l++)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, c], masque[l, c]);
                        chaine_bits += PixelQRCode_inv(pixels[l, c + 1], masque[l, c + 1]);
                    }
                    for (int l = 11; l >= 0; l--)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, c + 2], masque[l, c + 2]);
                        chaine_bits += PixelQRCode_inv(pixels[l, c + 3], masque[l, c + 3]);
                    }
                }
                for (int l = 0; l < pixels.GetLength(0); l++)
                {
                    if (l != 14)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, 8], masque[l, 8]);
                        chaine_bits += PixelQRCode_inv(pixels[l, 9], masque[l, 9]);
                    }
                }
                for (int l = pixels.GetLength(0) - 1; l >= 0; l--)
                {
                    if (l != 14)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, 10], masque[l, 10]);
                        chaine_bits += PixelQRCode_inv(pixels[l, 11], masque[l, 11]);
                    }
                }
                for (int c = 12; c < pixels.GetLength(1); c += 4)
                {
                    for (int l = 8; l < 12; l++)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, c], masque[l, c]);
                        if (c < 20)
                        {
                            chaine_bits += PixelQRCode_inv(pixels[l, c + 1], masque[l, c + 1]);
                        }
                    }
                    if (c < 18)
                    {
                        for (int l = 12; l >= 8; l--)
                        {
                            if (c + 2 != 14)
                            {
                                chaine_bits += PixelQRCode_inv(pixels[l, c + 2], masque[l, c + 2]);
                            }
                            chaine_bits += PixelQRCode_inv(pixels[l, c + 3], masque[l, c + 3]);
                        }
                    }
                }
            }
            else //décodage des données pour le QRCode version 2
            {
                for (int l = 0; l < 16; l++)
                {
                    chaine_bits += PixelQRCode_inv(pixels[l, 0], masque[l, 0]);
                    chaine_bits += PixelQRCode_inv(pixels[l, 1], masque[l, 1]);
                }
                for (int l = 15; l >= 0; l--)
                {
                    chaine_bits += PixelQRCode_inv(pixels[l, 2], masque[l, 2]);
                    chaine_bits += PixelQRCode_inv(pixels[l, 3], masque[l, 3]);
                }
                for (int l = 0; l < 16; l++)
                {
                    if (l < 4 || l > 8)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, 4], masque[l, 4]);
                        chaine_bits += PixelQRCode_inv(pixels[l, 5], masque[l, 5]);
                    }
                }
                for (int l = 15; l >= 0; l--)
                {
                    if (l < 4 || l > 8)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, 6], masque[l, 6]);
                        chaine_bits += PixelQRCode_inv(pixels[l, 7], masque[l, 7]);
                    }
                }
                for (int l = 0; l < 4; l++)
                {
                    chaine_bits += PixelQRCode_inv(pixels[l, 8], masque[l, 8]);
                    chaine_bits += PixelQRCode_inv(pixels[l, 9], masque[l, 9]);
                }
                for (int l = 4; l < 9; l++)
                {
                    chaine_bits += PixelQRCode_inv(pixels[l, 9], masque[l, 9]);
                }
                for (int l = 9; l < pixels.GetLength(0); l++)
                {
                    if (l != 18)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, 8], masque[l, 8]);
                        chaine_bits += PixelQRCode_inv(pixels[l, 9], masque[l, 9]);
                    }
                }
                for (int l = pixels.GetLength(0) - 1; l >= 0; l--)
                {
                    if (l != 18)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, 10], masque[l, 10]);
                        chaine_bits += PixelQRCode_inv(pixels[l, 11], masque[l, 11]);
                    }
                }
                for (int l = 9; l < pixels.GetLength(0); l++)
                {
                    if (l != 18)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, 12], masque[l, 12]);
                        chaine_bits += PixelQRCode_inv(pixels[l, 13], masque[l, 13]);
                    }
                }
                for (int l = 0; l < pixels.GetLength(0); l++)
                {
                    if (l != 18)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, 14], masque[l, 14]);
                        chaine_bits += PixelQRCode_inv(pixels[l, 15], masque[l, 15]);
                    }
                }
                for (int c = 16; c < pixels.GetLength(1) - 1; c += 4)
                {
                    for (int l = 8; l < 16; l++)
                    {
                        chaine_bits += PixelQRCode_inv(pixels[l, c], masque[l, c]);
                        chaine_bits += PixelQRCode_inv(pixels[l, c + 1], masque[l, c + 1]);
                    }
                    for (int l = 15; l >= 8; l--)
                    {
                        if (c + 2 != 18)
                        {
                            chaine_bits += PixelQRCode_inv(pixels[l, c + 2], masque[l, c + 2]);
                        }
                        chaine_bits += PixelQRCode_inv(pixels[l, c + 3], masque[l, c + 3]);
                    }
                }
                for (int l = 8; l < 16; l++)
                {
                    chaine_bits += PixelQRCode_inv(pixels[l, 24], masque[l, 24]);
                }
            }
            int longueur = Binaire_to_Decimal(Convert.ToInt32(chaine_bits.Substring(4, 9))); //on calcule la longueur du message
            string[] message_bits = new string[longueur / 2 + 1]; //tableau contenant chaque "morceaux" de 11 ou 6 bits concernant le message
            int test_impaire = longueur % 2; //on test la parité de la longueur du message
            for (int i = 0; i < message_bits.Length - test_impaire; i++)
            {
                message_bits[i] = chaine_bits.Substring(13 + i * 11, 11); //on découpe les bits du message en groupe de 11 bits
            }
            if (test_impaire == 1)
            {
                message_bits[message_bits.Length - 1] = chaine_bits.Substring(13 + (message_bits.Length - 1) * 11, 6); //si la longueur du message est parie on découpe les 6 bits de la dernière lettre
            }
            double[] message_entier = new double[longueur / 2 + test_impaire]; //tableau contenant les valeurs numériques des lettres en base 45
            for (int i = 0; i < message_entier.Length; i++)
            {
                message_entier[i] = Binaire_to_Decimal(Convert.ToDouble(message_bits[i])); //on converti les bits binaires de chaque lettres du message en entier
            }
            int[] message_alpha = new int[longueur];
            int indice = 0;
            for (int i = 0; i < message_entier.Length - test_impaire; i++)
            {
                int[] tab = Convertir_Base45_to10(Convert.ToDouble(message_entier[i])); //puis on converti en alaphanumérique les données de chauqe lettre 
                for (int l = 0; l < 2; l++)
                {
                    message_alpha[indice] = tab[l];
                    indice++;
                }
            }
            if (test_impaire == 1)
            {
                message_alpha[indice] = Convert.ToInt32(Math.Floor((double)message_entier[message_entier.Length - 1])); //si la taille du message est impaire on ajoute la dernière lettre en alphanumérique dans le tableau
            }
            string message = "";
            for (int i = 0; i < message_alpha.Length; i++)
            {
                char entier = ' ';
                switch (message_alpha[i])
                {
                    case 0:
                        entier = '0';
                        break;
                    case 1:
                        entier = '1';
                        break;
                    case 2:
                        entier = '2';
                        break;
                    case 3:
                        entier = '3';
                        break;
                    case 4:
                        entier = '4';
                        break;
                    case 5:
                        entier = '5';
                        break;
                    case 6:
                        entier = '6';
                        break;
                    case 7:
                        entier = '7';
                        break;
                    case 8:
                        entier = '8';
                        break;
                    case 9:
                        entier = '9';
                        break;
                    case 10:
                        entier = 'a';
                        break;
                    case 11:
                        entier = 'b';
                        break;
                    case 12:
                        entier = 'c';
                        break;
                    case 13:
                        entier = 'd';
                        break;
                    case 14:
                        entier = 'e';
                        break;
                    case 15:
                        entier = 'f';
                        break;
                    case 16:
                        entier = 'g';
                        break;
                    case 17:
                        entier = 'h';
                        break;
                    case 18:
                        entier = 'i';
                        break;
                    case 19:
                        entier = 'j';
                        break;
                    case 20:
                        entier = 'k';
                        break;
                    case 21:
                        entier = 'l';
                        break;
                    case 22:
                        entier = 'm';
                        break;
                    case 23:
                        entier = 'n';
                        break;
                    case 24:
                        entier = 'o';
                        break;
                    case 25:
                        entier = 'p';
                        break;
                    case 26:
                        entier = 'q';
                        break;
                    case 27:
                        entier = 'r';
                        break;
                    case 28:
                        entier = 's';
                        break;
                    case 29:
                        entier = 't';
                        break;
                    case 30:
                        entier = 'u';
                        break;
                    case 31:
                        entier = 'v';
                        break;
                    case 32:
                        entier = 'w';
                        break;
                    case 33:
                        entier = 'x';
                        break;
                    case 34:
                        entier = 'y';
                        break;
                    case 35:
                        entier = 'z';
                        break;
                    case 36:
                        entier = ' ';
                        break;
                    case 37:
                        entier = '$';
                        break;
                    case 38:
                        entier = '%';
                        break;
                    case 39:
                        entier = '*';
                        break;
                    case 40:
                        entier = '+';
                        break;
                    case 41:
                        entier = '-';
                        break;
                    case 42:
                        entier = '.';
                        break;
                    case 43:
                        entier = '/';
                        break;
                    case 44:
                        entier = ':';
                        break;
                } //on converti les données alphanumériques en caractère
                message += entier;
            }
            return message; //puis on retourne le message
        }
        /// <summary>
        /// fonction pour le décodage du QRCode ou on converti un nombre en base 45 en base 10
        /// </summary>
        /// <param name="nombre">nombre a convertir</param>
        /// <returns>on retourne le nombre converti en base 10</returns>
        public int[] Convertir_Base45_to10(double nombre)
        {
            int entier = Convert.ToInt32(Math.Floor(nombre / 45));
            int reste = Convert.ToInt32(nombre - (double)entier * 45);
            int[] tab = new int[2];
            tab[0] = entier;
            tab[1] = reste;
            return tab;
        }
        /// <summary>
        /// Fonction basique qui converti un nombre binaire en base 10
        /// </summary>
        /// <param name="nombre">nombre à convertir en base 10</param>
        /// <returns>retourne le nombre converti en base 10</returns>
        static int Binaire_to_Decimal(double nombre)
        {
            char[] tab = Convert.ToString(nombre).ToCharArray();
            Array.Reverse(tab);
            int returned = 0;
            for (int i = 0; i < tab.Length; i++)
            {
                if (tab[i] == '1')
                {
                    if (i == 0)
                    {
                        returned += 1;
                    }
                    else
                    {
                        returned += (int)Math.Pow(2, i);
                    }
                }
            }
            return returned;
        }
        #endregion
    }
}
