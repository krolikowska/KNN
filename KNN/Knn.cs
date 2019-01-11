using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNN
{
    public class Knn
    {
        private int k = 0; //inormacja ilu sąsiadów
        private DataManager dm;

        public Knn(string path)
        {
            Dm = new DataManager(path);
        }

        public int K { get => k; set => k = value; }
        public DataManager Dm { get => dm; set => dm = value; }

        public delegate double Distance(Tuple t1, Tuple t2);

        public void KnnClassyfication(List<Tuple> train, List<Tuple> test, int k, Distance ComputeDistance)
        {
            SimplePriorityQueue<Tuple> distances = new SimplePriorityQueue<Tuple>();

            for (int i = 0; i < test.Count; i++)
            {
                for (int j = 0; j < train.Count; j++)
                {
                    float prio = (float)ComputeDistance(test[i], train[j]);
                    distances.Enqueue(train[j], prio);
                }

                var kNeighb = distances.Take(k).ToList(); // wybieramy k elementow ze zbioru treningowego, dla ktorych obliczona odleglosc jest najmniejsza

                Dictionary<int, int> classCounts = new Dictionary<int, int>(); //indeks klasy + info ile wystapien
                int predictedClass = 0;
                for (int j = 1; j < 4; j++)
                {
                    var classes = (from n in kNeighb
                                   where n.ClassIndex == j
                                   select n.ClassIndex).Count();
                    classCounts.Add(j, classes);
                }

                //zwracamy najlicznijsza klase, w szczególnym wypadku może być > 1
                var classCountsList = from c in classCounts
                                      where c.Value == classCounts.Values.Max()
                                      select c;

                if (classCountsList.Count() == 1)
                {
                    predictedClass = classCountsList.First().Key;
                }
                else //jesli jest wiecej niz jedna klasa, to  wybieramy klasę zawierającą najbliższego sąsiada
                {
                    foreach (var neigh in kNeighb)
                    {
                        if (predictedClass == 0)
                        {
                            foreach (var c in classCounts)
                            {
                                if (neigh.ClassIndex == c.Key)
                                {
                                    predictedClass = c.Key;
                                    break;
                                }
                            }
                        }
                    }
                }

                test[i].PredictedClassIndex = predictedClass;
                predictedClass = 0;
                distances.Clear();
            }
        }

        public double Accuracy(List<Tuple> test)
        {
            double badClass = (from t in test
                               where t.ClassIndex != t.PredictedClassIndex
                               select t).Count();

            double tmp = badClass / test.Count;
            return (1 - tmp);
        }

        private static List<Tuple> PrepareTrainData(List<Tuple> toTrain1, List<Tuple> toTrain2)
        {
            List<Tuple> train = new List<Tuple>();
            train.AddRange(toTrain1);
            train.AddRange(toTrain2);
            return train;
        }

        public void CrossTest(int k, Distance ComputeDistance)
        {
            // dm.CountClassesInSamples();

            List<Tuple> train = PrepareTrainData(dm.SampleA, dm.SampleB);
            this.KnnClassyfication(train, dm.SampleC, k, ComputeDistance);
            train = PrepareTrainData(dm.SampleB, dm.SampleC);
            this.KnnClassyfication(train, dm.SampleA, k, ComputeDistance);
            train = PrepareTrainData(dm.SampleC, dm.SampleA);
            this.KnnClassyfication(train, dm.SampleB, k, ComputeDistance);

            Console.WriteLine("\ntest: SampleA, sambleB, SampleC, k={0}\n", k);
            train.AddRange(dm.SampleB);

            for (int i = 1; i < 4; i++)
            {
                PrintErrors(this.ComputeErrors(train, i), i);
            }

            //foreach (var i in train)
            //{
            //    if (i.ClassIndex != i.PredictedClassIndex)
            //    {
            //        Console.WriteLine($"rzeczywista {i.ClassIndex} obliczona {i.PredictedClassIndex} dane:{i.AttributesData[0]}, {i.AttributesData[1]}, {i.AttributesData[2]}, {i.AttributesData[3]}");
            //    }
            //}

            Console.WriteLine("Accuracy: {0:p}", this.Accuracy(train));
        }

        public void PrintErrors(int[] errors, int classIndx)
        {
            Console.WriteLine("Hipoteza: dane należą do klasy {0}", classIndx);
            // Console.WriteLine("H0: dane należą do klasy {0}", classIndx);
            Console.WriteLine("------------------------------------");
            Console.WriteLine($"|   {errors[0]:d2} || II {errors[1]:d2} |");
            Console.WriteLine($"| I {errors[2]:d2} ||    {errors[3]:d2} |");
            Console.WriteLine("------------------------------------");
        }

        public int[] ComputeErrors(List<Tuple> test, int classIndx)
        {   // Hipoteza: jest to obiekt nalezacy do klasy classIndx
            // H0 : nie jest to obiekt nalezacy do classIndx, t != classIndx

            int truePositive = 0; int falseNegative = 0;
            int falsePositive = 0; int trueNegative = 0;
            //  for(classIndx = 1; classIndx < 4; classIndx++)
            {
                for (int i = 0; i < test.Count; i++)
                {
                    //odrzucamy H0 (t==1) i w rzeczywisstosci tpr ==1
                    if (test[i].PredictedClassIndex == classIndx && test[i].ClassIndex == classIndx)
                        truePositive++;
                    //odrzucamy H0 (t==1) i a to inny obiekt tpr !=1 (I rodzaj bledu)
                    if (test[i].PredictedClassIndex == classIndx && test[i].ClassIndex != classIndx)
                        falsePositive++;
                    //potwierdzamy h0(ten obiekt nie jest klasy t!=1), a to jest klasa 1 == 1, blad II rodzaju
                    if (test[i].PredictedClassIndex != classIndx && test[i].ClassIndex == classIndx)
                        falseNegative++;
                    //potwierdzamy h0(h!= 1) i w rzeczywistci tpr != 1
                    if (test[i].PredictedClassIndex != classIndx && test[i].ClassIndex != classIndx)
                        trueNegative++;
                }
            }
            int[] errors = new int[4] { truePositive, falseNegative, falsePositive, trueNegative };
            return errors;
        }

        public double GetManhattanDistance(Tuple t1, Tuple t2)
        {
            double distance = 0;
            for (int i = 0; i < t1.Size; i++)
            {
                distance += Math.Abs(t1.AttributesData[i] - t2.AttributesData[i]);
            }
            return distance;
        }

        public double GetEculidianDistance(Tuple t1, Tuple t2)
        {
            double distance = 0;
            for (int i = 0; i < t1.Size; i++)
            {
                distance += (t1.AttributesData[i] - t2.AttributesData[i]) * (t1.AttributesData[i] - t2.AttributesData[i]);
            }
            return Math.Sqrt(distance);
        }

        public double GetChebyshevDistance(Tuple t1, Tuple t2)
        {
            double distance = 0;

            List<double> distances = new List<double>();
            for (int i = 0; i < t1.Size; i++)
            {
                distances.Add(Math.Abs(t1.AttributesData[i] - t2.AttributesData[i]));
            }

            distance = distances.Max();
            return distance;
        }
    }
}