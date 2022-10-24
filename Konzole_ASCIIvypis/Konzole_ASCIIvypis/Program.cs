using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konzole_ASCIIvypis
{
    class Program
    {
        static readonly string rozdeleniStranky = new string('=', 43);
        const int pocetZnaku = 255;
        const int pocetSloupcu = 3;
        const int pocetRadku = pocetZnaku / pocetSloupcu;
        const int pocetRadkuNaStranku = 20;

        const int velikostOdsazeni = 8;
        static void Main(string[] args)
        {
            while (true)
            {
                Vypis();
                Console.WriteLine("Dosáhli jste konce stránky.\n");
                Console.ReadKey();
            }
        }
        public static void Vypis()
        {
            Console.WriteLine("int32: ASCII".PadLeft(24) + "\n");
            for (int i = 0; i < pocetRadku; i++)
            {
                for (int j = 0; j < pocetSloupcu; j++)
                {
                    int indexZnaku = i * pocetSloupcu + j;
                    Console.Write($"{indexZnaku + 1}: {IntToAscii(indexZnaku)}\t");
                }

                Console.WriteLine();

                if ((i + 1) % pocetRadkuNaStranku == 0)
                {
                    Console.WriteLine(rozdeleniStranky);
                    Console.ReadKey();
                    Console.WriteLine("int32: ASCII".PadLeft(24) + "\n");
                }
            }
        }
        public static string ZarovnatStringNaUrcitouDelku(string str, int delka)
        {
            str = str.PadRight(delka - str.Length);
            return str;
        }
        public static string IntToAscii(int ASCIIznak)
        {
            // Get ASCII character.
            char c = (char)ASCIIznak;

            // Get display string.
            string display = string.Empty;
            if (char.IsWhiteSpace(c))
            {
                display = c.ToString();
                switch (c)
                {
                    case '\t':
                        display = "\\t";
                        break;
                    case ' ':
                        display = "space";
                        break;
                    case '\n':
                        display = "\\n";
                        break;
                    case '\r':
                        display = "\\r";
                        break;
                    case '\v':
                        display = "\\v";
                        break;
                    case '\f':
                        display = "\\f";
                        break;
                }
            }
            else if (char.IsControl(c))
            {
                display = "control";
            }
            else
            {
                display = c.ToString();
            }
            display = ZarovnatStringNaUrcitouDelku(display, velikostOdsazeni);
            return display;
        }
    }
}
