using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ProjektTrpaslik
{
    
    /// //////////////////////////////PARENT TRIDA
   
    public class Trpaslik
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Symbol { get; set; }
        public int dX { get; set; } = 1;
        public int dY { get; set; } = 0;
        public char ZnakPod { get; set; } = ' ';
        public List<string> Tracker { get; set; } = new List<string>();
        public bool Cil { get; set; } = false;

        public Trpaslik(int x, int y, char symbol = 'T')
        {
            X = x;
            Y = y;
            Symbol = symbol;
        }
        public virtual void OtocSe() 
        { 
            int temp = dX;
            dX = -dY;
            dY = temp;
        }

        public virtual void Pohyb(int dx, int dy, char[,] bludiste)
        {
            int nx = X + dx;
            int ny = Y + dy;
            if (JeVolno(nx, ny, bludiste))
                Posun(nx, ny, bludiste);
        }

        

        public bool JeVolno(int x, int y, char[,] bludiste)
        {
            if (x < 0 || y < 0 || y >= bludiste.GetLength(0) || x >= bludiste.GetLength(1))
                return false;
            return bludiste[y, x] != '#' && bludiste[y, x] != 'S';
        }

        protected void ZkusSmer(int[][] smery, char[,] bludiste)
        {
            foreach (var s in smery)
            {
                if (JeVolno(X + s[0], Y + s[1], bludiste))
                {
                    dX = s[0];
                    dY = s[1];
                    Posun(X + dX, Y + dY, bludiste);
                    return;
                }
            }
        }

        public void Posun(int nx, int ny, char[,] bludiste)
        {
            bludiste[Y, X] = ZnakPod;
            char novy = bludiste[ny, nx];
            ZnakPod = (novy == 'S' || novy == ' ' || novy == 'F') ? novy : ' ';
            X = nx;
            Y = ny;
            bludiste[Y, X] = Symbol;
            Tracker.Add($"[{Y},{X}]");
            File.AppendAllText($"vystup_{Symbol}.txt", $"{Symbol}: [{Y},{X}]\n");
        }
    }


    /// ///////////////////////////////PRAVOTOCIVY
  
    class PravoTocivyTrpaslik : Trpaslik
    {
        public PravoTocivyTrpaslik(int x, int y) : base(x, y, 'P') { }

        public override void Pohyb(int dx, int dy, char[,] bludiste)
        {
           
            int[][] smery = {
                new[] { -dy, dx },   // prave
                new[] { dx, dy },    // rovne
                new[] { dy, -dx },   // levo
                new[] { -dx, -dy }   // zpet
            };
            ZkusSmer(smery, bludiste);
        }
    }


    /// ////////////////////////LEVOTOCIVY
  
    class LevoTocivyTrpaslik : Trpaslik
    {
        public LevoTocivyTrpaslik(int x, int y) : base(x, y, 'L') { }

        public override void Pohyb(int dx, int dy, char[,] bludiste)
        {
            
            int[][] smery = {
                new[] { dy, -dx },   // levo
                new[] { dx, dy },    // rovne
                new[] { -dy, dx },   // pravo
                new[] { -dx, -dy }   // zpet
            };
            ZkusSmer(smery, bludiste);
        }
    }
    
    /// ///////////////////////////////////PODVADEJICI
    
    class PodvadejiciTrpaslik : Trpaslik
    {
        private static Random rnd = new Random();
        int casTeleportu;
        int ubehlo = 0;

        public PodvadejiciTrpaslik(int x, int y) : base(x, y, 'C')
        {
            casTeleportu = rnd.Next(0, 15000);
        }

        public override void Pohyb(int dx, int dy, char[,] bludiste)
        {
            ubehlo += 100;
            if (ubehlo >= casTeleportu)
            {
                TeleportNaCil(bludiste);
            }
            else
            {
                base.Pohyb(dx, dy, bludiste);
            }
        }

        private void TeleportNaCil(char[,] bludiste)
        {
            for (int y = 0; y < bludiste.GetLength(0); y++)
                for (int x = 0; x < bludiste.GetLength(1); x++)
                    if (bludiste[y, x] == 'F')
                    {
                        bludiste[Y, X] = ZnakPod;
                        ZnakPod = 'F';
                        X = x;
                        Y = y;
                        bludiste[Y, X] = Symbol;
                        Tracker.Add($"[{Y},{X}] TELEPORT");
                        File.AppendAllText($"vystup_{Symbol}.txt", $"{Symbol}: [{Y},{X}] TELEPORT\n");
                        return;
                    }
        }
    }
    
    /// ///////////////////////////////////////TRPASLIK KTERY CESTU VYRESI PRED VLOZENIM 
    
    class ChytryTrpaslik : Trpaslik
    {
        Queue<int[]> cesta = new Queue<int[]>();

        public ChytryTrpaslik(int x, int y) : base(x, y, 'O') { }

        public void HledaniCesty(char[,] bludiste)
        {
            int vyska = bludiste.GetLength(0);
            int sirka = bludiste.GetLength(1);
            bool[,] navstiveno = new bool[vyska, sirka];
            int[,] predX = new int[vyska, sirka];
            int[,] predY = new int[vyska, sirka];

            Queue<int[]> fronta = new Queue<int[]>();
            fronta.Enqueue(new[] { X, Y });
            navstiveno[Y, X] = true;

            int[] sx = { 0, 0, 1, -1 };
            int[] sy = { 1, -1, 0, 0 };
            int cilX = -1, cilY = -1;

            // BFS Algoritmus
            while (fronta.Count > 0)
            {
                int[] a = fronta.Dequeue();
                int ax = a[0], ay = a[1];

                if (bludiste[ay, ax] == 'F')
                {
                    cilX = ax;
                    cilY = ay;
                    break;
                }

                for (int i = 0; i < 4; i++)
                {
                    int nx = ax + sx[i];
                    int ny = ay + sy[i];
                    if (nx < 0 || ny < 0 || nx >= sirka || ny >= vyska) continue;
                    if (navstiveno[ny, nx]) continue;
                    if (bludiste[ny, nx] == '#' || bludiste[ny, nx] == 'S') continue;

                    navstiveno[ny, nx] = true;
                    predX[ny, nx] = ax;
                    predY[ny, nx] = ay;
                    fronta.Enqueue(new[] { nx, ny });
                }
            }

            if (cilX == -1) return; //Kdyz nebude existovat cesta

            // Zpetne sestaveni cesty
            List<int[]> seznam = new List<int[]>();
            int cx = cilX, cy = cilY;
            while (cx != X || cy != Y)
            {
                seznam.Add(new[] { cx, cy });
                int px = predX[cy, cx];
                int py = predY[cy, cx];
                cx = px;
                cy = py;
            }
            seznam.Reverse();

            foreach (var bod in seznam)
                cesta.Enqueue(bod);
        }

        public override void Pohyb(int dx, int dy, char[,] bludiste)
        {
            if (cesta.Count == 0) return;
            int[] dalsi = cesta.Dequeue();
            Posun(dalsi[0], dalsi[1], bludiste);
        }
    }

    internal class Program
    {
        static List<Trpaslik> trpaslici = new List<Trpaslik>();
        static char[,] bludiste;

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            string cesta = @"C:\Users\atakm\Desktop\CSharp\ProjektTrpaslik\Maze.dat";

            if (!File.Exists(cesta))
            {
                Console.WriteLine("Soubor neexistuje: " + cesta);
                return;
            }

            // Mazani starych vystupu               /////////////////Vystupy trpasliku vypisuji do souboru, protoze mi to prislo jako citelnejsi reseni nez v konzoli
            foreach (string soubor in Directory.GetFiles(".", "vystup_*.txt"))
                File.Delete(soubor);

            NacteniBludiste(cesta);
            NastaveniKonzole();  //Pouzivam Windows console Host, jelikoz mi neslo skalovat Windows Terminal
            NajdiStart(out int sx, out int sy);

            
            Queue<Trpaslik> fronta = new Queue<Trpaslik>();
            fronta.Enqueue(new PravoTocivyTrpaslik(sx, sy));
            fronta.Enqueue(new LevoTocivyTrpaslik(sx, sy));
            fronta.Enqueue(new PodvadejiciTrpaslik(sx, sy));

            ChytryTrpaslik chytry = new ChytryTrpaslik(sx, sy);
            chytry.HledaniCesty(bludiste);
            fronta.Enqueue(chytry);

            
            PridejTrpaslika(fronta.Dequeue());
            Console.Clear();
            VykresleniBludiste();

            
            int casovac = 0;
            int intervalPridani = 5000;
            int tickMs = 100;

            while (true)
            {
                Thread.Sleep(tickMs);
                casovac += tickMs;

                //Pridavani trpasliku do bludiste
                if (casovac >= intervalPridani && fronta.Count > 0)
                {
                    PridejTrpaslika(fronta.Dequeue());
                    PrekresleniBodu(trpaslici[trpaslici.Count - 1].X, trpaslici[trpaslici.Count - 1].Y);
                    casovac = 0;
                }

                
                foreach (var t in trpaslici)
                {
                    if (t.Cil) continue;

                    int staryX = t.X, staryY = t.Y;
                    t.Pohyb(t.dX, t.dY, bludiste);

                    if (t.X != staryX || t.Y != staryY)
                    {
                        PrekresleniBodu(staryX, staryY);

                        if (t.ZnakPod == 'F')
                        {
                            t.Cil = true;
                            bludiste[t.Y, t.X] = 'F';
                        }

                        PrekresleniBodu(t.X, t.Y);
                    }
                    else
                    {
                        t.OtocSe();
                    }

                }
                if(fronta.Count == 0 && trpaslici.All(t => t.Cil))
                {
                    break;
                }
            }
            Console.Clear();
            Console.WriteLine("Vsichni trpaslici dorazili do cile");
            Console.ReadLine();
        }

        
        /// //////POMOCNE METODY
      
        

        static void NacteniBludiste(string soubor)
        {
            string[] vsechnyRadky = File.ReadAllLines(soubor);
            int vyska = vsechnyRadky.Length;

            for (int i = 1; i < vsechnyRadky.Length; i++)
                if (vsechnyRadky[i].StartsWith("#S"))
                {
                    vyska = i;
                    break;
                }

            string[] radky = vsechnyRadky.Take(vyska).ToArray();
            int sirka = radky.Max(r => r.Length);
            bludiste = new char[vyska, sirka];

            for (int i = 0; i < vyska; i++)
                for (int x = 0; x < radky[i].Length; x++)
                    bludiste[i, x] = radky[i][x];
        }

        static void NastaveniKonzole()
        {
            int vyska = bludiste.GetLength(0) + 1;
            int sirka = bludiste.GetLength(1) + 1;

            if (Console.WindowWidth > sirka) Console.WindowWidth = sirka;
            if (Console.WindowHeight > vyska) Console.WindowHeight = vyska;
            Console.BufferWidth = sirka;
            Console.BufferHeight = vyska;
            Console.WindowWidth = sirka;
            Console.WindowHeight = vyska;
        }

        static void VykresleniBludiste()
        {
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < bludiste.GetLength(0); y++)
            {
                for (int x = 0; x < bludiste.GetLength(1); x++)
                    Console.Write(bludiste[y, x]);
                Console.WriteLine();
            }
        }

        static void PrekresleniBodu(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(bludiste[y, x]);
        }

        static void NajdiStart(out int startX, out int startY)
        {
            startX = 0;
            startY = 0;
            for (int y = 0; y < bludiste.GetLength(0); y++)
                for (int x = 0; x < bludiste.GetLength(1); x++)
                    if (bludiste[y, x] == 'S')
                    {
                        startX = x;
                        startY = y;
                        return;
                    }
        }

        static void PridejTrpaslika(Trpaslik t)
        {
            t.ZnakPod = bludiste[t.Y, t.X];
            trpaslici.Add(t);
            bludiste[t.Y, t.X] = t.Symbol;
        }
    }
}