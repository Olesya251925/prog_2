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
        }

        private void InitializeComponents()
        {
            // Создание текстовых полей
            labelA = new Label();
            labelA.Text = "Введите значение a:";
            labelA.Location = new System.Drawing.Point(10, 10);
            labelA.AutoSize = true;
            this.Controls.Add(labelA);

            textBoxA = new TextBox();
            textBoxA.Location = new System.Drawing.Point(labelA.Right + 5, 10);
            textBoxA.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(textBoxA);

            labelB = new Label();
            labelB.Text = "Введите значение b:";
            labelB.Location = new System.Drawing.Point(10, 40);
            labelB.AutoSize = true;
            this.Controls.Add(labelB);

            textBoxB = new TextBox();
            textBoxB.Location = new System.Drawing.Point(labelB.Right + 5, 40);
            textBoxB.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(textBoxB);

            labelE = new Label();
            labelE.Text = "Введите значение точности e:";
            labelE.Location = new System.Drawing.Point(10, 70);
            labelE.AutoSize = true;
            this.Controls.Add(labelE);

            textBoxE = new TextBox();
            textBoxE.Location = new System.Drawing.Point(labelE.Right + 5, 70);
            textBoxE.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(textBoxE);

            // Создание кнопки "Рассчитать"
            calculateButton = new Button();
            calculateButton.Text = "Рассчитать";
            calculateButton.Location = new System.Drawing.Point(10, 100);
            calculateButton.Click += new System.EventHandler(this.CalculateButton_Click);
            this.Controls.Add(calculateButton);

            // Создание кнопки "Очистить"
            clearButton = new Button();
            clearButton.Text = "Очистить";
            clearButton.Location = new System.Drawing.Point(calculateButton.Left + calculateButton.Width + 10, 100);
            clearButton.Click += new System.EventHandler(this.ClearButton_Click);
            this.Controls.Add(clearButton);

            localPointsTextBox = new TextBox();
            localPointsTextBox.Multiline = true;
            localPointsTextBox.ScrollBars = ScrollBars.Vertical;
            localPointsTextBox.Location = new System.Drawing.Point(10, 190);
            localPointsTextBox.Size = new System.Drawing.Size(300, 60);
            this.Controls.Add(localPointsTextBox);

            // Создание PlotView для отображения графика с точками
            plotView = new PlotView();
            plotView.Dock = DockStyle.Fill;
            this.Controls.Add(plotView);
        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            // Проверка введенных данных
            if (!double.TryParse(textBoxA.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double a) ||
                !double.TryParse(textBoxB.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double b) ||
                !double.TryParse(textBoxE.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double epsilon))
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
            MessageBox.Show("Начинаю вычисления...");

            // Создание объектов для поиска локальных минимумов и максимумов
            var minFinder = new LocalMinimumFinder(function);
            var maxFinder = new LocalMaximumFinder(function);

            // Вычисление минимума и максимума с использованием новых классов
            double minResult = minFinder.FindExtremum(a, b, epsilon);
            double maxResult = maxFinder.FindExtremum(a, b, epsilon);

            // Вывод результатов в TextBox
            localPointsTextBox.AppendText($"Локальный минимум: {minResult}\n");
            localPointsTextBox.AppendText($"Локальный максимум: {maxResult}\n");

            // Создание объекта для поиска корня
            var rootFinder = new RootFinder(function);

            // Вычисление корня с использованием нового класса
            double rootResult = rootFinder.FindRoot(a, b, epsilon);

            // Вывод результатов в TextBox
            localPointsTextBox.AppendText($"Точка пересечения с осью X: {rootResult}\n");

            // Построение графика с точками минимума, максимума и точкой пересечения
            ShowGraphWithExtremumAndRootPoints(a, b, function, minResult, maxResult, rootResult);
        }

        private void ShowGraphWithExtremumAndRootPoints(double a, double b, Func<double, double> function, double minResult, double maxResult, double rootResult)
        {
            // Увеличиваем интервал для построения графика
            double expandedA = a - 1;
            double expandedB = b + 1;

            // Создание новой формы с графиком
            using (GraphForm graphForm = new GraphForm(expandedA, expandedB, function, minResult, maxResult, rootResult))
            {
                graphForm.ShowDialog();
            }
        }

        public class RootFinder
        {
            private readonly Func<double, double> Function;

            public RootFinder(Func<double, double> function)
            {
                Function = function;
            }

            public double FindRoot(double a, double b, double epsilon)
            {
                double x;

                do
                {
                    x = (a + b) / 2;

                    double fa = Function(a);
                    double fb = Function(b);
                    double fx = Function(x);

                    if (fa * fx < 0)
                    {
                        b = x;
                    }
                    else
                    {
                        a = x;
                    }
                } while (Math.Abs(Function(x)) > epsilon);

                return x;
            }
        }


        public class LocalMinimumFinder
        {
            private readonly Func<double, double> Function;

            public LocalMinimumFinder(Func<double, double> function)
            {
                Function = function;
            }

            public double FindExtremum(double a, double b, double epsilon)
            {
                double x;

                do
                {
                    x = (a + b) / 2;

                    double f = Function(x);
                    double fa = Function(a);
                    double fb = Function(b);

                    if (f < fa && f < fb)
                    {
                        b = x;
                    }
                    else if (fa < fb)
                    {
                        b = x;
                    }
                    else
                    {
                        a = x;
                    }
                } while (Math.Abs(b - a) > epsilon);

                return (a + b) / 2;
            }
        }

        public class LocalMaximumFinder
        {
            private readonly Func<double, double> Function;

            public LocalMaximumFinder(Func<double, double> function)
            {
                Function = function;
            }

            public double FindExtremum(double a, double b, double epsilon)
            {
                double x;

                do
                {
                    x = (a + b) / 2;

                    double f = Function(x);
                    double fa = Function(a);
                    double fb = Function(b);

                    if (f > fa && f > fb)
                    {
                        b = x;
                    }
                    else if (fa > fb)
                    {
                        b = x;
                    }
                    else
                    {
                        a = x;
                    }
                } while (Math.Abs(b - a) > epsilon);

                return (a + b) / 2;
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            textBoxA.Clear();
            textBoxB.Clear();
            textBoxE.Clear();
            localPointsTextBox.Text = string.Empty;
        }
    }

    public class GraphForm : Form
    {
        private PlotView plotView;

        public GraphForm(double a, double b, Func<double, double> function, double minResult, double maxResult, double rootResult)
        {
            InitializeComponents(a, b, function, minResult, maxResult, rootResult);
        }

        private void InitializeComponents(double a, double b, Func<double, double> function, double minResult, double maxResult, double rootResult)
        {
            double expandedA = a - 1;
            double expandedB = b + 1;

            plotView = new PlotView();
            plotView.Dock = DockStyle.Fill;
            this.Controls.Add(plotView);

            // Создание модели графика
            var model = new PlotModel { Title = "График функции" };

            // Добавление сетки
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MajorGridlineColor = OxyColors.LightGray });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MajorGridlineColor = OxyColors.LightGray });

            var lineSeries = new LineSeries();
            for (double x = expandedA; x <= expandedB; x += 0.1)
            {
                lineSeries.Points.Add(new DataPoint(x, function(x)));
            }
            model.Series.Add(lineSeries);

            // Добавление точек минимума и максимума на график
            model.Annotations.Add(new OxyPlot.Annotations.PointAnnotation { X = minResult, Y = function(minResult), Text = "Min", TextPosition = new DataPoint(minResult, function(minResult) - 0.5),
                TextColor = OxyColors.Blue
            });
            model.Annotations.Add(new OxyPlot.Annotations.PointAnnotation { X = maxResult, Y = function(maxResult), Text = "Max", TextPosition = new DataPoint(maxResult, function(maxResult) + 0.5),
                TextColor = OxyColors.Blue });

            // Добавление точки пересечения с осью X на график
            var rootAnnotation = new OxyPlot.Annotations.PointAnnotation { X = rootResult, Y = 0, Text = "Root", TextPosition = new DataPoint(rootResult, 1),
                TextColor = OxyColors.Green };
            model.Annotations.Add(rootAnnotation);

            // Добавление линий, проходящих через точку пересечения
            model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation { Type = OxyPlot.Annotations.LineAnnotationType.Horizontal, Color = OxyColors.Green, Y = rootResult });
            model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation { Type = OxyPlot.Annotations.LineAnnotationType.Vertical, Color = OxyColors.Green, X = rootResult });

            // Установка модели для PlotView
            plotView.Model = model;

        }

    }
 }
