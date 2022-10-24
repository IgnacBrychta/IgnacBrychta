using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caesarova_Sifra_Konzole
{
    class Program
    {
        public static readonly char[] znakyAbecedy = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        public static bool konec = false;
        static void Main(string[] args)
        {
            while (!konec)
            {
                Mainloop();
            }
            
        }
        public static void Mainloop()
        {
            Console.WriteLine("Co si přejete udělat?");
            Console.WriteLine("1. Šifrovat zprávu\n2. Dešifrovat zprávu\n3. Konec");
            ConsoleKey consoleKey = Console.ReadKey(true).Key;
            switch (consoleKey)
            {
                case ConsoleKey.NumPad1:
                    VlozitZasifrovaneHeslo();
                    break;
                case ConsoleKey.NumPad2:
                    VlozitDesifrovaneHeslo();
                    break;
                case ConsoleKey.NumPad3:
                    konec = true;
                    break;
                case ConsoleKey.F1:
                    VlozitZasifrovaneHeslo();
                    break;
                case ConsoleKey.F2:
                    VlozitDesifrovaneHeslo();
                    break;
                case ConsoleKey.F3:
                    konec = true;
                    break;
            }
        }
        public static void VlozitZasifrovaneHeslo()
        {
            Console.WriteLine("Vložte zprávu k šifrování.");
            string sifrovanaZprava = Console.ReadLine();

            int posun = VlozitCislo("Vložte posun.");

            string zasifrovanaZprava = ZasifrovatZpravu(sifrovanaZprava, posun);
            Console.WriteLine("Zasifrovana zprava: " + zasifrovanaZprava);
            Console.ReadKey();
            Console.Clear();
        }
        public static void VlozitDesifrovaneHeslo()
        {
            Console.WriteLine("Vložte zprávu k dešifrování.");
            string puvodniZprava = Console.ReadLine();

            int posun = VlozitCislo("Vložte posun.");

            string desifrovanaZprava = DesifrovatZpravu(puvodniZprava, posun);
            Console.WriteLine("Desifrovana zprava: " + desifrovanaZprava);
            Console.ReadKey();
            Console.Clear();
        }
        public static int VlozitCislo(string prompt)
        {
            Console.WriteLine(prompt);
            int posun;
            while (!int.TryParse(Console.ReadLine(), out posun))
            {
                Console.WriteLine("Neplatný posun.");
                Console.ReadKey();
            }
            return posun;
        }
        public static string ZasifrovatZpravu(string zprava, int posun)
        {
            posun %= znakyAbecedy.Length;
            char[] zasifrovanaZprava = new char[zprava.Length];
            for (int i = 0; i < zprava.Length; i++)
            {
                char znakZpravy = zprava[i];
                int poziceZnaku = Array.IndexOf(znakyAbecedy, znakZpravy);
                poziceZnaku += posun;
                while (poziceZnaku < 0)
                {
                    poziceZnaku += znakyAbecedy.Length;
                }
                poziceZnaku %= znakyAbecedy.Length;
                zasifrovanaZprava[i] = znakyAbecedy[poziceZnaku];
            }
            return string.Join("", zasifrovanaZprava);
        }
        public static string DesifrovatZpravu(string zprava, int posun)
        {
            posun %= znakyAbecedy.Length;
            char[] desifrovanaZprava = new char[zprava.Length];
            for (int i = 0; i < zprava.Length; i++)
            {
                char znakZpravy = zprava[i];
                int poziceZnaku = Array.IndexOf(znakyAbecedy, znakZpravy);
                poziceZnaku -= posun;
                while (poziceZnaku < 0)
                {
                    poziceZnaku += znakyAbecedy.Length;
                }
                poziceZnaku %= znakyAbecedy.Length;
                desifrovanaZprava[i] = znakyAbecedy[poziceZnaku];
            }
            return string.Join("", desifrovanaZprava);
        }
    }
}
