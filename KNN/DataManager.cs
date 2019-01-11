﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KNN
{
    public class DataManager
    {
        private string path;
        private int numOfAttributes;
        private List<Tuple> sampleA = new List<Tuple>();
        private List<Tuple> sampleB = new List<Tuple>();
        private List<Tuple> sampleC = new List<Tuple>();
        public string Path { get => path; set => path = value; }
        public int NumOfAttributes { get => numOfAttributes; set => numOfAttributes = value; }
        public List<Tuple> SampleA { get => sampleA; set => sampleA = value; }
        public List<Tuple> SampleB { get => sampleB; set => sampleB = value; }
        public List<Tuple> SampleC { get => sampleC; set => sampleC = value; }

        public DataManager(string path)
        {
            this.Path = path;
            this.PrepareTestSamples();
        }

        public void PrepareTestSamples()
        {
            List<Tuple> allSamples = this.ReadFromFile(path);
            var classes = (from a in allSamples
                           select a.ClassIndex).Distinct();

            for (int i = 0; i < allSamples.Count - 2; i += 3) //po kolumnach, bierzemy po jednym z kazdej klasy
            {
                sampleA.Add(allSamples[i]);
                sampleB.Add(allSamples[i + 1]);
                sampleC.Add(allSamples[i + 2]);
            }

            if (allSamples.Count % 3 == 1)
                sampleA.Add(allSamples[allSamples.Count - 1]);
            if (allSamples.Count % 3 == 2)
                sampleB.Add(allSamples[allSamples.Count - 1]);
        }

        public void CountClassesInSamples()
        {
            Console.WriteLine("sample A");
            for (int i = 1; i < 4; i++)
            {
                int num = (from a in sampleA
                           where a.ClassIndex == i
                           select a).Count();
                Console.Write($"class {i} : {num}\t");
            }

            Console.WriteLine("\nsample B");
            for (int i = 1; i < 4; i++)
            {
                int num = (from a in sampleB
                           where a.ClassIndex == i
                           select a).Count();
                Console.Write($"class {i} : {num}\t");
            }

            Console.WriteLine("\nsample C");
            for (int i = 1; i < 4; i++)
            {
                int num = (from a in sampleC
                           where a.ClassIndex == i
                           select a).Count();
                Console.Write($"class {i} : {num}\t");
            }
        }

        public List<Tuple> ReadFromFile(string path)
        {
            string[] file = File.ReadAllLines(path);
            List<Tuple> allSamples = new List<Tuple>();
            Dictionary<string, int> classNamesAndSize = new Dictionary<string, int>();
            List<int> classes = new List<int>();

            for (int i = 0; i < file.Length; i++)
            {
                string[] tmp = file[i].Split(new char[] { ',' });
                this.NumOfAttributes = tmp.Length - 1;
                double[] attributes = new double[numOfAttributes];

                for (int j = 1; j < tmp.Length; j++)
                {
                    attributes[j - 1] = Convert.ToDouble(tmp[j], new CultureInfo("en-US"));
                }
                Tuple tuple = new Tuple(Convert.ToInt16(tmp[0]), NumOfAttributes, attributes);

                allSamples.Add(tuple);
            }

            return allSamples;
        }
    }
}