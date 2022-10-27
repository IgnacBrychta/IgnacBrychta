using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EdHouse_UkolProStazisty
{
    class Program
    {
        static private OpenFileDialog openFileDialog;
        [STAThread]
        static void Main(string[] args)
        {
            Console.BufferHeight = short.MaxValue - 1;

            string filePath = "";
            openFileDialog = new OpenFileDialog
            {
                Title = "Select Valid Input Data",
                InitialDirectory = @"C:\users",
                Filter = "Text files (*.txt)|*.txt"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                Console.Title = "Processing: " + filePath;
            }
            else
            {
                Console.WriteLine("No file selected :'(");
                Console.Title = "No File Selected";
                Console.ReadKey();
                return;
            }
            


            string rawFileContents = LoadFile(filePath);
            string[] fileContents = ProcessFileContents(rawFileContents);

            string[] driverRangeIntervals = fileContents[0].Split('-');
            Driver.minimumRangeForBreak = int.Parse(driverRangeIntervals[0]);
            Driver.maximumRangeForBreak = int.Parse(driverRangeIntervals[1]);

            string[] driver2MoveVectors = fileContents[1].Split(',');
            string[] driver1MoveVectors = fileContents[2].Split(',');

            Driver driver1 = new Driver();
            Driver driver2 = new Driver();

            Point driverIntersection;
            List<Point> otherIntersections = new List<Point>();
            List<int[]> otherIntersections_Distances = new List<int[]>();
            bool doDriversIntersect = FindDriverIntersection(driver1, driver2, driver1MoveVectors, driver2MoveVectors, out driverIntersection, ref otherIntersections, ref otherIntersections_Distances);

            if (doDriversIntersect)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("A valid intersection has been found.");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine($"Coordinates: [{driverIntersection.X}; {driverIntersection.Y}].");
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("No valid intersection has been found.");
                Console.BackgroundColor = ConsoleColor.Black;
            }

            if (otherIntersections.Count != 0)
            {
                Console.WriteLine("\nOther intersections:\n");
                OtherIntersectionsOutput(otherIntersections, otherIntersections_Distances, driverIntersection);

                Console.WriteLine($"Limits: <{Driver.minimumRangeForBreak}; {Driver.maximumRangeForBreak}>");
            }

            Console.ReadLine();
        }

        static void OtherIntersectionsOutput(List<Point> otherIntersections, List<int[]> otherIntersections_Distances, Point intersection)
        {
            for (int i = 0; i < otherIntersections.Count; i++)
            {
                if (intersection != null)
                {
                    if (otherIntersections[i].X == intersection.X && otherIntersections[i].Y == intersection.Y)
                    {
                        continue;
                    }
                }

                int distance1 = otherIntersections_Distances[i][0];
                int distance2 = otherIntersections_Distances[i][1];
                Console.Write(otherIntersections[i] + ": Driver 1: ");
                if (distance1 > Driver.maximumRangeForBreak)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write(distance1);
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                else
                {
                    Console.Write(distance1);
                }
                Console.Write(" distance units | Driver 2: ");
                if (distance2 > Driver.maximumRangeForBreak)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write(distance2);
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                else
                {
                    Console.Write(distance2);
                }
                Console.WriteLine(" distance units");
            }

        }

        static bool FindDriverIntersection(Driver driver1, Driver driver2, string[] directionsOfDriver1, string[] directionsOfDriver2, out Point intersectionPoint, ref List<Point> otherIntersections, ref List<int[]> otherIntersections_Distances)
        {
            intersectionPoint = new Point(0, 0);
            int minimumAmountOfDirections = new[] { directionsOfDriver1.Length, directionsOfDriver2.Length }.Min();
            int turnsTillTimeIntervals = 0;

            for (; turnsTillTimeIntervals < minimumAmountOfDirections; turnsTillTimeIntervals++) // Let drivers ride around to reach minimum distance from start to intersection
            {
                driver1.Move(directionsOfDriver1[turnsTillTimeIntervals]);
                driver2.Move(directionsOfDriver2[turnsTillTimeIntervals]);
                if (driver1.reachedBreakInterval || driver2.reachedBreakInterval)
                {
                    break;
                }
            }
            List<DriverRoute> driver1Routes = new List<DriverRoute>();
            for (int i = turnsTillTimeIntervals; i < minimumAmountOfDirections; i++) // Map all different lines of driver1 after they are in interval for a break
            {
                driver1.Move(directionsOfDriver1[i], false);
                driver1Routes.Add(new DriverRoute(new Point(driver1.previousCoordX, driver1.previousCoordY), new Point(driver1.coordX, driver1.coordY), driver1.totalDistanceTraveled));
                if (driver1.totalDistanceTraveled > Driver.maximumRangeForBreak)
                {
                    break;
                }
            }

            List<Point> driver2RouteCoords = new List<Point>();

            MoveDriverByOneCellAtAtime(turnsTillTimeIntervals, minimumAmountOfDirections, directionsOfDriver2, driver2, ref driver2RouteCoords);
            for (int i = 0; i < driver1Routes.Count; i++) // iterate through routes of driver1
            {
                for (int j = 0; j < driver2RouteCoords.Count; j++) // iterate over route coordinates of driver2
                {
                    if (driver1Routes[i].DoesPointBelongOnRoute(driver2RouteCoords[j])) // check if driver1 and driver2 crossed paths (if any coordinate of driver2 belongs on any line of driver1)
                    {
                        int driver2_distanceToIntersection = driver2RouteCoords[j].totalDistanceTraveled;
                        int driver1_distanceToIntersection = driver1Routes[i - 1].totalDistanceTraveled +
                            Math.Abs(driver1Routes[i].pointA.X - driver2RouteCoords[j].X + driver1Routes[i].pointA.Y - driver2RouteCoords[j].Y);
                        otherIntersections.Add(driver2RouteCoords[j]);
                        otherIntersections_Distances.Add(new[] { driver1_distanceToIntersection, driver2_distanceToIntersection });
                        if (driver1_distanceToIntersection >= Driver.minimumRangeForBreak && driver1_distanceToIntersection <= Driver.maximumRangeForBreak &&
                            driver2_distanceToIntersection >= Driver.minimumRangeForBreak && driver2_distanceToIntersection <= Driver.maximumRangeForBreak)
                        {
                            intersectionPoint = driver2RouteCoords[j];
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        static void MoveDriverByOneCellAtAtime(int turnsTillTimeIntervals, int minimumAmountOfDirections, string[] directionsOfDriver2, Driver driver2, ref List<Point> driver2RouteCoords)
        {
            for (int i = turnsTillTimeIntervals; i < minimumAmountOfDirections; i++) // driver2 drives a single cell every time, their coordinates are saved
            {
                for (int j = 0; j < int.Parse(directionsOfDriver2[i].Substring(0, directionsOfDriver2[i].Length - 1)); j++)
                {
                    driver2.MoveOneCell(directionsOfDriver2[i].Last());
                    driver2RouteCoords.Add(new Point(driver2.coordX, driver2.coordY, driver2.totalDistanceTraveled));
                    if (driver2.totalDistanceTraveled > Driver.maximumRangeForBreak * 1.1)
                    {
                        return;
                    }
                }
            }
        }

        static string[] ProcessFileContents(string rawFileContents)
        {
            string[] fileContents = rawFileContents.Substring(0, rawFileContents.Length - 1)
                .Split('\n')
                .Select(el => el.Substring(0, el.Length - 1))
                .ToArray();
            return fileContents;
        }

        static string LoadFile(string pathToFile)
        {
            FileStream fileStream;
            StreamReader streamReader;

            fileStream = File.Open(pathToFile, FileMode.Open);

            streamReader = new StreamReader(fileStream);
            string rawFileContents = streamReader.ReadToEnd();
            fileStream.Close();

            return rawFileContents;
        }
    }

    /// <summary>
    /// Represents a point in the Cartesian coordinate system
    /// </summary>
    public class Point
    {
        public int X;
        public int Y;
        public int totalDistanceTraveled;
        public Point(int coordX, int coordY, int totalDistanceTraveled = 0)
        {
            X = coordX;
            Y = coordY;
            this.totalDistanceTraveled = totalDistanceTraveled;
        }
        public override string ToString()
        {
            return $"[{X}; {Y}]";
        }
    }

    /// <summary>
    /// Represents a line that a drivers makes from one point to the other
    /// </summary>
    public class DriverRoute
    {
        public Point pointA;
        public Point pointB;
        public int totalDistanceTraveled;
        public DriverRoute(Point start, Point end, int totalDistanceTraveled)
        {
            pointA = start;
            pointB = end;
            this.totalDistanceTraveled = totalDistanceTraveled;
        }
        public bool DoesPointBelongOnRoute(Point point)
        {
            if (pointA.X == pointB.X) // vertical line
            {
                if (point.X != pointA.X)
                {
                    return false;
                }
                if (pointA.Y > pointB.Y)
                {
                    return pointA.Y >= point.Y && pointB.Y <= point.Y;
                }
                else
                {
                    return pointB.Y >= point.Y && pointA.Y <= point.Y;
                }
            }
            else // horizontal line
            {
                if (point.Y != pointA.Y)
                {
                    return false;
                }
                if (pointA.X > pointB.X)
                {
                    return pointA.X >= point.X && pointB.X <= point.X;
                }
                else
                {
                    return pointB.X >= point.X && pointA.X <= point.X;
                }
            }
        }
        public override string ToString()
        {
            return pointA.ToString() + " | " + pointB.ToString();
        }
    }

    /// <summary>
    /// Represents a Driver, stores their position and distance traveled
    /// </summary>
    public class Driver
    {
        public int previousCoordX;
        public int previousCoordY;
        public int coordX;
        public int coordY;
        public static int minimumRangeForBreak;
        public static int maximumRangeForBreak;
        public int totalDistanceTraveled = 0;
        public bool reachedBreakInterval = false;
        public Driver(int StartCoordX = 0, int StartCoordY = 0)
        {
            coordX = StartCoordX;
            coordY = StartCoordY;
        }
        public void Move(string moveVector, bool stopWhenInBreakRangeInterval = true)
        {
            previousCoordX = coordX;
            previousCoordY = coordY;
            int moveVectorLength = moveVector.Length;
            char direction = moveVector[moveVectorLength - 1];
            int distance = int.Parse(moveVector.Substring(0, moveVectorLength - 1));

            if (distance + totalDistanceTraveled >= minimumRangeForBreak)
            {
                if (stopWhenInBreakRangeInterval)
                {
                    reachedBreakInterval = true;
                    return;
                }
            }
            else
            {
                reachedBreakInterval = false;
            }

            switch (direction)
            {
                case 'N':
                    coordY += distance;
                    break;
                case 'S':
                    coordY -= distance;
                    break;
                case 'E':
                    coordX += distance;
                    break;
                case 'W':
                    coordX -= distance;
                    break;
            }
            totalDistanceTraveled += distance;
        }
        public void MoveOneCell(char direction)
        {
            switch (direction)
            {
                case 'N':
                    coordY++;
                    break;
                case 'S':
                    coordY--;
                    break;
                case 'E':
                    coordX++;
                    break;
                case 'W':
                    coordX--;
                    break;
            }
            totalDistanceTraveled++;
        }
    }
}
