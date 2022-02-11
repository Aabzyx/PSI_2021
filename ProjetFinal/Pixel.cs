using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetFinal
{
    class Pixel
    {
        private int red; //Variables de classe
        private int green;
        private int blue;

        public Pixel(int red, int green, int blue) //Constructeur de la classe
        {
            this.blue = blue;
            this.green = green;
            this.red = red;
        }

        // Propriétés des variables, toutes en lecture et écriture
        public int Red
        {
            get { return red; }
            set { red = value; }
        }

        public int Green
        {
            get { return green; }
            set { green = value; }
        }

        public int Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        // Fonction toString décrivant les couleurs d'un Pixel
        public string toString()
        {
            return red + " " + green + " " + blue + " ";
        }

        /// <summary>
        /// Fonction très basique qui permet de tester si un pixel est noir
        /// </summary>
        /// <returns>retourne vrai si le pixel est noir ou faux dans le cas contraire</returns>
        public bool Test_pixel_noir()
        {
            bool returned = false;
            if (Red == 0 && Green == 0 && Blue == 0)
            {
                returned = true;
            }
            return returned;
        }

        public bool Test_pixel_blanc()
        {
            bool returned = false;
            if (Red == 255 && Green == 255 && Blue == 255)
            {
                returned = true;
            }
            return returned;
        }
    }
}
