namespace KNN
{
    public class Tuple //struktura do przechowywania szczegolow kazdej klasy = nazwa, liczebnosc, zestaw atrybutow
    {
        private int classIndex; //okresla do ktorej klasy przynalezy probka
        private int predictedClassIndex = 0;
        private static int size; //ile atrybutow
        private double[] attributesData = new double[size]; //w tablicy sa atrubuty dla konkretnej próbki

        public int ClassIndex { get => classIndex; set => classIndex = value; }
        public int Size { get => size; set => size = value; }
        public double[] AttributesData { get => attributesData; set => attributesData = value; }
        public int PredictedClassIndex { get => predictedClassIndex; set => predictedClassIndex = value; }

        public Tuple(int classIndex, int size, double[] data)
        {
            this.classIndex = classIndex;
            Tuple.size = size;
            this.attributesData = data;
        }
    };
}