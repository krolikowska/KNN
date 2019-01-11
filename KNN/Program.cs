using System;

namespace KNN
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Knn knn = new Knn("../../wine_norm.csv");
            Console.WriteLine("Hipoteza:  obiekt należy do klasy X");
            Console.WriteLine("H0:  obiekt nie należy do klasy X");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine($"             |    H0 odrzucamy  ||  H0 potwierdzamy  |");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine($" H0 błędna   |   true positive  || II false negative |");
            Console.WriteLine($" H0 prawdziwa| I false positive ||    true negative  |");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("--------------Eculidian-----------------------");
            knn.CrossTest(1, new Knn.Distance(knn.GetEculidianDistance));
            knn.CrossTest(3, new Knn.Distance(knn.GetEculidianDistance));
            knn.CrossTest(5, new Knn.Distance(knn.GetEculidianDistance));
            knn.CrossTest(10, new Knn.Distance(knn.GetEculidianDistance));
            Console.WriteLine("---------------Chebyshev-------------------");
            knn.CrossTest(1, new Knn.Distance(knn.GetChebyshevDistance));
            knn.CrossTest(3, new Knn.Distance(knn.GetChebyshevDistance));
            knn.CrossTest(5, new Knn.Distance(knn.GetChebyshevDistance));
            knn.CrossTest(10, new Knn.Distance(knn.GetChebyshevDistance));
            Console.WriteLine("--------------Manhattan--------------------");
            knn.CrossTest(1, new Knn.Distance(knn.GetManhattanDistance));
            knn.CrossTest(3, new Knn.Distance(knn.GetManhattanDistance));
            knn.CrossTest(5, new Knn.Distance(knn.GetManhattanDistance));
            knn.CrossTest(10, new Knn.Distance(knn.GetManhattanDistance));

            Console.ReadKey();
        }
    }
}