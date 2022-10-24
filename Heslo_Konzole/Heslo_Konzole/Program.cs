using System;
using System.Collections.Generic;
using System.Threading;



namespace Heslo_Konzole
{
    internal class Program
    {
        private static List<char> hesloUzivatele;
        private static readonly bool zobrazitPosledniZnakHesla = true;
        public const int bezpecnaDelkaHesla = 1;
        public static readonly bool bezpecneHesloObsahujeSpecialniZnak = true;
        public static readonly bool bezpecneHesloObsahujeCislici = true;
        public const int prodlevaMeziZadaveniZnakuHesla = 75;
        public const int maximalniPocetPokusuProVlozeniHesla = 3;

        public static void Main(string[] args)
        {
            PrihlasovaciNabidka();
        }
        public static void PrihlasovaciNabidka()
        {
            RegistraceHesla(); // HESLO BY SE NIKDY NEMĚLO UKLÁDAT JAKO STRING!
            if (PrihlasitSe())
            {
                ZobrazitPoUspesnemPrihlaseni();
            }
        }
        public static bool PrihlasitSe()
        {
            List<char> vlozeneHeslo = new List<char>();
            bool hesloZadano = false;
            int zbyvajiciPocetPokusu = maximalniPocetPokusuProVlozeniHesla;
            bool vlozenoSpravneHeslo = false;
            do
            {
                string dodatecneInformace = $"Zbývající počet pokusů: {zbyvajiciPocetPokusu}";
                VlozitHeslo(ref vlozeneHeslo, ref hesloZadano, ref dodatecneInformace);
                zbyvajiciPocetPokusu--;
                hesloZadano = false;

            } while (!(vlozenoSpravneHeslo = ZkontrolovatSpravnostHesel(ref hesloUzivatele, ref vlozeneHeslo)) && zbyvajiciPocetPokusu > 0);

            if (vlozenoSpravneHeslo)
            {
                Console.WriteLine("Vložili jste správné heslo, nyní se můžete podívat na supertajný obsah.");
            }
            else
            {
                Console.WriteLine("Zadané heslo se neshoduje, shnijte v pekle, chlípníku, jak by řekl pan Končák.");
            }
            Console.WriteLine("Stiskněte libovolné tlačítko pro pokračování.");
            Console.ReadKey();
            
            return vlozenoSpravneHeslo;

        }
        public static void VlozitHeslo(ref List<char> znakyHesla, ref bool hesloZadano, ref string dodatecneInformace, bool opakovaniHesla = false, int delkaPuvodnihoHesla = -1)
        {
            do
            {
                VlozitZnakHesla(ref znakyHesla, ref hesloZadano, ref dodatecneInformace, opakovaniHesla, delkaPuvodnihoHesla);
            } while (!hesloZadano);
        }
        public static void RegistraceHesla()
        {
            List<char> znakyHesla_pokus_1 = new List<char>();
            List<char> znakyHesla_pokus_2 = new List<char>();
            bool hesloZadano = false;
            string dodatecneInformace = string.Empty;
            VlozitHeslo(ref znakyHesla_pokus_1, ref hesloZadano, ref dodatecneInformace);
            hesloUzivatele = znakyHesla_pokus_1;
            while (!ZkontrolovatSpravnostHesel(ref znakyHesla_pokus_1, ref znakyHesla_pokus_2))
            {
                hesloZadano = false;
                VlozitHeslo(ref znakyHesla_pokus_2, ref hesloZadano, ref dodatecneInformace, opakovaniHesla: true, delkaPuvodnihoHesla: znakyHesla_pokus_1.Count);
            }
            Console.WriteLine("Heslo úspěšně zaregistrováno. Stiskněte libovolné tlačítko pro vyzvání k přihlášení.");
            Console.ReadKey();
        }
        public static bool JsouHeslaStejna(ref List<char> heslo1, ref List<char> heslo2)
        {
            if (heslo1.Count != heslo2.Count) { return false; }
            for (int i = 0; i < heslo1.Count; i++)
            {
                if (heslo1[i] != heslo2[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static bool ZkontrolovatSpravnostHesel(ref List<char> heslo1, ref List<char> heslo2)
        {
            bool heslaJsouStejna = JsouHeslaStejna(ref heslo1, ref heslo2);
            if (!heslaJsouStejna && heslo2.Count != 0)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Hesla se neshodují.\nStiskněte libovolné tlačítko pro pokračování.");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ReadKey();
            }
            return heslaJsouStejna;
        }
        public static void VlozitZnakHesla(ref List<char> znakyHesla, ref bool hesloZadano, ref string dodatecneInformace, bool opakovaniHesla = false, int delkaPuvodnihoHesla = -1)
        {
            Console.Clear();
            if (!opakovaniHesla)
            {
                Console.WriteLine("Vložte heslo.");
            } else
            {
                Console.WriteLine("Pro kontrolu vložte své heslo znovu.");
                Console.WriteLine("Původní heslo: " + new string('*', delkaPuvodnihoHesla));
            }
            if (dodatecneInformace != string.Empty)
            {
                Console.WriteLine(dodatecneInformace);
            }
            Console.WriteLine("Používejte prosím jen latinské znaky bez diakritiky, číslice a základní speciální znaky ASCII tabulky.\n");
            _ = ZobrazitBezpecnostHesla(ref znakyHesla);
            Console.Write("Heslo: " + new string('*', znakyHesla.Count));
            ConsoleKey znakHesla = Console.ReadKey(true).Key;

            if (znakHesla == ConsoleKey.Enter)
            {
                bool[] bezpecnostHesla = ZobrazitBezpecnostHesla(ref znakyHesla);
                if (!bezpecnostHesla[0]) { return; }
                if (bezpecneHesloObsahujeSpecialniZnak)
                {
                    if (!bezpecnostHesla[1]) { return; }
                }
                if (bezpecneHesloObsahujeCislici)
                {
                    if (!bezpecnostHesla[2]) { return; }
                }
                hesloZadano = true;
                Console.Clear();
                return;
            }
            else if (znakHesla == ConsoleKey.Backspace)
            {
                if (znakyHesla.Count != 0)
                {
                    znakyHesla.RemoveAt(znakyHesla.Count - 1);
                    Console.Write("\b");
                    Console.Write(" ");
                    Console.Write("\b");
                }
            }
            else
            {
                char znakHeslaZpracovany = (char)znakHesla;
                znakyHesla.Add(znakHeslaZpracovany);
                if (zobrazitPosledniZnakHesla)
                {
                    ZobrazitPosledniZnakHesla(znakHeslaZpracovany);
                }
                else
                {
                    Console.Write("*");
                }
            }
        }
        public static bool[] ZobrazitBezpecnostHesla(ref List<char> znakyHesla)
        {
            bool[] bezpecnostZadanehoHesla = new bool[] {false, false, false};
            Console.WriteLine();
            if (znakyHesla.Count < bezpecnaDelkaHesla)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Vaše heslo je příliš krátké");
                Console.BackgroundColor = ConsoleColor.Black;
            }
            else if (znakyHesla.Count == bezpecnaDelkaHesla)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Délka Vašeho hesla je dostačující.");
                Console.BackgroundColor = ConsoleColor.Black;
                bezpecnostZadanehoHesla[0] = true;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("Délka Vašeho hesla je vynikající.");
                Console.BackgroundColor = ConsoleColor.Black;
                bezpecnostZadanehoHesla[0] = true;
            }

            if (bezpecneHesloObsahujeSpecialniZnak)
            {
                if (ObsahujeHesloSpecialniZnak(ref znakyHesla))
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine("Heslo obsahuje speciální znak.");
                    Console.BackgroundColor = ConsoleColor.Black;
                    bezpecnostZadanehoHesla[1] = true;
                } else
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine("Heslo neobsahuje speciální znak.");
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }

            if (bezpecneHesloObsahujeCislici)
            {
                if (ObsahujeHesloCislici(ref znakyHesla))
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.WriteLine("Heslo obsahuje číslici.");
                    Console.BackgroundColor = ConsoleColor.Black;
                    bezpecnostZadanehoHesla[2] = true;
                } else
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine("Heslo neobsahuje číslici.");
                    Console.BackgroundColor = ConsoleColor.Black;
                }
            }
            return bezpecnostZadanehoHesla;
        }
        public static bool ObsahujeHesloCislici(ref List<char> znakyHesla)
        {
            foreach (var item in znakyHesla)
            {
                if (int.TryParse(new string(item, 1), out _))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ObsahujeHesloSpecialniZnak(ref List<char> znakyHesla)
        {
            foreach (var item in znakyHesla)
            {
                int ASCIIznak = CharToASCII(item);
                if (!(IsNumberInRange(ASCIIznak, 41, 91) || IsNumberInRange(ASCIIznak, 97, 122)))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsNumberInRange(int number, int min, int max)
        {
            return number > min && number < max;
        }
        public static int CharToASCII(char znak)
        {
            return (int)znak;
        }
        public static void ZobrazitPosledniZnakHesla(char znak)
        {
            Console.Write(znak);
            Thread.Sleep(prodlevaMeziZadaveniZnakuHesla);
            Console.Write("\b");
            Console.Write("*");
        }
        public static void ZobrazitPoUspesnemPrihlaseni()
        {
            Console.WriteLine(
                "&&&&&&&&&&&&&&@@@&&@@@@&&%&%%%%%%%%%&#%@@@@@%%&%###%%%%&%%%%%%&@&@&&&%&&%%##%%%%\n&&&&&&&&&&&@@@@&&&@@&@@@@@@&%%%%&&%%%&@%##*,,*(%&&&#%#%%&&&@@&@@&&@@&&&&&%%%%%%%\n&&&&&&&&@@@&&&&&&@@@@@@&&&@@&&&@%%&@%&#*,,,.,*,//%#%&%%&%%#@@@@@@@&&@@@%%%%%%%%%\n&&&&&@@@@@@@@&&@@@@&&@@&&&@@&&&@@@@@&/*((######(*##%%%%&&&%%%%%@@@&&&%%@@@@%&&&&\n&&&&@@@@@@@@@&&&&@@&&@@@&&&&&&&@@@@@@*(/((/#/*##*&%#%%%&%%&&&&%%&&%%%&@@@@@@@&&&\n&@&&@@@@@@@@@@&@@&&&@@&&&@&%&&&&%@@@((((((/#%#%#&@@%%%%%@@%%%%%&&%&%%&@@@@@@@&&&\n&&&&&&@@@@@&&&&&&@&&@@@@@@@&&&@&%%%%%%&((*/(#######&%%%%@@&&&@@%%@@&%%@@@@@@&@&&\n@@@&@@@&&&&@&@@@@&@@%&@@&@@&&&%%@&@%%%(/(((###%%&&@%%%%%@@#@@@%%%%%%%%&&&&&&&&&&\n&@@@&@%&%&@@&@@&%%@&%%%%@@&&%%%@@%%&%%((((####,@%%&&@%%%%%#%&%@@%%&@&@@@@&&@@@@@\n&@@&&@@%&@@&@&@%%%&%%%%%@@@%%%#/,../(///(((/%%,,..,/&%%%&%&%%%&%%#%&@&@@@&&@@@@@\n%&@&%%%%%%%&&&%&@@&&%@&%%&%,../.....(//%(%%&%,...*((....,&%%@#%&#&&%%%%%@@&@@@&&\n%&@@@@%%&@@&@@%%%@@&%&@%@%,..........,*%*.*%..../((##(.,,,&%&%%&@@@%%&@@%%%%&@@@\n%%@&@%%%%&&&%%%&%%@%@%%%#,...........#*%%. .......(##/...,%%%&@%@@%%%@&@@@%%@@@@\n&@%%@@&@&@%%%@@&&&%%%@&&&............./%%*....... */#(....&&&&#%%%@@@%%%%%&&&&&&\n&&%%@@&&&@%%&&@@@@%%@&@&(............//##*......    ,,.,..,&&%&%%%%%%%%%####%%%%\n@&%%(&&&&&%#&&&&&&%%%#,.        (///(*,/((....        ......%####&@@@@&%&&&&@@%%\n%%%#&&&&%%##&&&%&&%%&%.*/.../..,///((*,,*/ .           ..... ..##%%&&&&%%&&&&&%%\n%%#######%##%%%%%%#%%#/........ ..   * ...               .....,(#%%%%%%##&&%%&%#\n"
                );
            Console.ReadKey();
        }
    }
}