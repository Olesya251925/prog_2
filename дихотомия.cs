using System;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot.Axes;

namespace OptimizationApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Создание и запуск основной формы
            Application.Run(new MainForm());
        }
    }

    public partial class MainForm : Form
    {
        private TextBox textBoxA;
        private TextBox textBoxB;
        private TextBox textBoxE;
        private Button calculateButton;
        private Button clearButton;
        private Label labelA;
        private Label labelB;
        private Label labelE;
        private TextBox localPointsTextBox;
        private PlotView plotView;

        public MainForm()
        {
            InitializeComponents();
            this.Size = new System.Drawing.Size(900, 700);
        }

        private void InitializeComponents()
        {
            // Создаем основной контейнер
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            this.Controls.Add(mainPanel);

            // Введите значения a, b, e
            labelA = new Label();
            labelA.Text = "Введите значение a:";
            labelA.Location = new System.Drawing.Point(10, 10);
            labelA.AutoSize = true;
            mainPanel.Controls.Add(labelA);

            textBoxA = new TextBox();
            textBoxA.Location = new System.Drawing.Point(labelA.Right + 5, 10);
            textBoxA.Size = new System.Drawing.Size(100, 20);
            mainPanel.Controls.Add(textBoxA);

            labelB = new Label();
            labelB.Text = "Введите значение b:";
            labelB.Location = new System.Drawing.Point(10, 40);
            labelB.AutoSize = true;
            mainPanel.Controls.Add(labelB);

            textBoxB = new TextBox();
            textBoxB.Location = new System.Drawing.Point(labelB.Right + 5, 40);
            textBoxB.Size = new System.Drawing.Size(100, 20);
            mainPanel.Controls.Add(textBoxB);

            labelE = new Label();
            labelE.Text = "Введите значение точности e:";
            labelE.Location = new System.Drawing.Point(10, 70);
            labelE.AutoSize = true;
            mainPanel.Controls.Add(labelE);

            textBoxE = new TextBox();
            textBoxE.Location = new System.Drawing.Point(labelE.Right + 5, 70);
            textBoxE.Size = new System.Drawing.Size(100, 20);
            mainPanel.Controls.Add(textBoxE);

            // Кнопки Рассчитать и Очистить
            calculateButton = new Button();
            calculateButton.Text = "Рассчитать";
            calculateButton.Location = new System.Drawing.Point(10, 100);
            calculateButton.Click += new System.EventHandler(this.CalculateButton_Click);
            mainPanel.Controls.Add(calculateButton);

            clearButton = new Button();
            clearButton.Text = "Очистить";
            clearButton.Location = new System.Drawing.Point(calculateButton.Left + calculateButton.Width + 10, 100);
            clearButton.Click += new System.EventHandler(this.ClearButton_Click);
            mainPanel.Controls.Add(clearButton);

            // Текстовое поле для вывода результатов
            localPointsTextBox = new TextBox();
            localPointsTextBox.Multiline = true;
            localPointsTextBox.ScrollBars = ScrollBars.Vertical;
            localPointsTextBox.Location = new System.Drawing.Point(10, 130);
            localPointsTextBox.Size = new System.Drawing.Size(300, 60);
            mainPanel.Controls.Add(localPointsTextBox);

            // Высота под график 
            int graphHeight = 500;

            // Создаем контейнер для графика
            Panel graphContainer = new Panel();
            graphContainer.Dock = DockStyle.Bottom; // Док-стиль изменен на Bottom, чтобы занимать нижнюю часть основного контейнера
            graphContainer.Height = graphHeight; // Устанавливаем высоту контейнера
            mainPanel.Controls.Add(graphContainer);

            // Создаем и добавляем PlotView
            plotView = new PlotView();
            plotView.Dock = DockStyle.Fill; // Делаем его заполняющим для родительского контейнера
            graphContainer.Controls.Add(plotView);
        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            // Проверка введенных данных
            if (!double.TryParse(textBoxA.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double a) ||
                !double.TryParse(textBoxB.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double b) ||
                !double.TryParse(textBoxE.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double enteredValue))
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения для a, b и e.");
                return;
            }

            // Проверка и обмен значений, если a > b
            if (a > b)
            {
                MessageBox.Show("Значение b должно быть больше значения a.");
                return;
            }

            // Функция для оптимизации
            Func<double, double> function = x => (27 - 18 * x + 2 * Math.Pow(x, 2)) * Math.Pow(Math.E, -x / 3);

            // Создание объекта для поиска корня
            var rootFinder = new RootFinder(function);

            // Определение точности в зависимости от введенного числа
            double calculatedEpsilon = enteredValue >= 0 ? Math.Pow(10, -enteredValue) : enteredValue;

            // Вычисление корня с использованием нового класса
            double rootResult = rootFinder.FindRoot(a, b, calculatedEpsilon);

            // Вывод результатов в TextBox
            localPointsTextBox.AppendText($"Точка пересечения с осью X : {rootResult}");

            // Построение графика с точкой пересечения
            ShowGraphWithRootPoint(a, b, function, rootResult, calculatedEpsilon);
        }

        private void ShowGraphWithRootPoint(double a, double b, Func<double, double> function, double rootResult, double epsilon)
        {
            // Создание модели графика
            var model = new PlotModel { Title = "График функции" };

            // Добавление сетки
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MajorGridlineColor = OxyColors.LightGray });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MajorGridlineColor = OxyColors.LightGray });

            var lineSeries = new LineSeries();
            int numberOfPoints = (int)((b - a) / epsilon);

            for (int i = 0; i <= numberOfPoints; i++)
            {
                double x = a + i * epsilon;
                lineSeries.Points.Add(new DataPoint(x, function(x)));
            }

            model.Series.Add(lineSeries);

            // Добавление точки пересечения с осью X на график
            var rootAnnotation = new OxyPlot.Annotations.PointAnnotation
            {
                X = rootResult,
                Y = 0,
                Text = "Root",
                TextPosition = new DataPoint(rootResult, 1),
                TextColor = OxyColors.Green
            };
            model.Annotations.Add(rootAnnotation);

            // Установка модели для существующего PlotView
            plotView.Model = model;
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            textBoxA.Clear();
            textBoxB.Clear();
            textBoxE.Clear();
            localPointsTextBox.Text = string.Empty;
        }
    }

    public class RootFinder
    {
        private readonly Func<double, double> Function;

        public RootFinder(Func<double, double> function)
        {
            Function = function;
        }

        public double FindRoot(double a, double b, double epsilon, int maxIterations = 1000)
        {
            // Определение количества знаков после запятой для округления
            int decimalPlaces = GetDecimalPlaces(epsilon);

            double x;

            // Итеративный процесс метода дихотомии
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // Вычисление середины интервала
                x = (a + b) / 2;

                // Значения функции на концах и середине интервала
                double fa = Function(a);
                double fb = Function(b);
                double fx = Function(x);

                // Проверка знака функции на концах и середине интервала
                if (fa * fx < 0)
                {
                    // Если знаки разные, сдвигаем правый конец интервала
                    b = x;
                }
                else
                {
                    // Если знаки одинаковы, сдвигаем левый конец интервала
                    a = x;
                }

                // Проверка на достижение значения функции близкого к 0
                if (Math.Abs(fx) < epsilon)
                {
                    // Округление результата до указанного числа знаков после запятой
                    return Math.Round(x, decimalPlaces);
                }
            }

            // Если не удалось найти корень в заданное количество итераций
            throw new InvalidOperationException($"Не удалось найти корень в {maxIterations} итераций");
        }


        // Метод для определения количества знаков после запятой в числе
        private int GetDecimalPlaces(double number)
        {
            string epsilonString = number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            int decimalPlaces = epsilonString.Length - epsilonString.IndexOf('.') - 1;
            return decimalPlaces;
        }
    }
}
