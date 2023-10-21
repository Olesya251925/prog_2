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

            calculateButton = new Button();
            calculateButton.Text = "Рассчитать";
            calculateButton.Location = new System.Drawing.Point(10, 100);
            calculateButton.Click += new System.EventHandler(this.CalculateButton_Click);
            this.Controls.Add(calculateButton);

            clearButton = new Button();
            clearButton.Text = "Очистить";
            clearButton.Location = new System.Drawing.Point(calculateButton.Left + calculateButton.Width + 10, 100);
            clearButton.Click += new System.EventHandler(this.ClearButton_Click);
            this.Controls.Add(clearButton);

            localPointsTextBox = new TextBox();
            localPointsTextBox.Multiline = true;
            localPointsTextBox.ScrollBars = ScrollBars.Vertical;
            localPointsTextBox.Location = new System.Drawing.Point(10, 190);
            localPointsTextBox.Size = new System.Drawing.Size(300, 150);
            this.Controls.Add(localPointsTextBox);

            plotView = new PlotView();
            plotView.Dock = DockStyle.Fill;
            this.Controls.Add(plotView);
        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(textBoxA.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double a) ||
                !double.TryParse(textBoxB.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double b) ||
                !double.TryParse(textBoxE.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double epsilon))
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения для a, b и e.");
                return;
            }

            int epsilonLength = textBoxE.Text.Length - textBoxE.Text.IndexOf('.') - 1;
            epsilon = Math.Pow(10, -epsilonLength);

            if (a > b)
            {
                MessageBox.Show("Значение b должно быть больше значения a.");
                return;
            }

            Func<double, double> function = x => (27 - 18 * x + 2 * Math.Pow(x, 2)) * Math.Pow(Math.E, -x / 3);
            Func<double, double> derivative = x => -1 * (2 * x * x * Math.Exp(-x / 3) / 3) + 10 * x * Math.Exp(-x / 3) - 27 * Math.Exp(-x / 3);

            //Func<double, double> function = x => (10 * x - 10);
            //Func<double, double> derivative = x => 10;

            var minFinder = new LocalMinimumFinder(function, derivative);
            var maxFinder = new LocalMaximumFinder(function, derivative);

            var rootFinder = new RootFinder(function, derivative);

            double minResult = minFinder.FindMinimum(a, b, epsilon);
            double maxResult = maxFinder.FindMaximum(a, b, epsilon);
            double rootResult = rootFinder.FindRoot(a, b, epsilon);

            double functionMinResult = function(minResult);
            double functionMaxResult = function(maxResult);
            double functionRootResult = function(rootResult);

            localPointsTextBox.AppendText($"Локальный минимум: {minResult}\n");
            localPointsTextBox.AppendText($"Локальный максимум: {maxResult}\n");
            localPointsTextBox.AppendText($"Точка пересечения с осью X: {rootResult}\n");
            localPointsTextBox.AppendText($"Значение функции: {functionRootResult}\n");

            ShowGraphWithExtremumAndRootPoints(a, b, function, minResult, maxResult, rootResult);
        }

        private void ShowGraphWithExtremumAndRootPoints(double a, double b, Func<double, double> function, double minResult, double maxResult, double rootResult)
        {
            double expandedA = a - 1;
            double expandedB = b + 1;

            using (GraphForm graphForm = new GraphForm(expandedA, expandedB, function, minResult, maxResult, rootResult))
            {
                graphForm.ShowDialog();
            }
        }

        public class RootFinder
        {
            private readonly Func<double, double> Function;
            private readonly Func<double, double> Derivative;

            public RootFinder(Func<double, double> function, Func<double, double> derivative)
            {
                Function = function;
                Derivative = derivative;
            }

            public double FindRoot(double a, double b, double epsilon)
            {
                int decimalPlaces = GetDecimalPlaces(epsilon);

                double x = (a + b) / 2;

                while (Math.Abs(Function(x)) > epsilon)
                {
                    x = x - Function(x) / Derivative(x);
                }

                return Math.Round(x, decimalPlaces);
            }

            // Метод для определения количества знаков после запятой в числе
            private int GetDecimalPlaces(double number)
            {
                string epsilonString = number.ToString(System.Globalization.CultureInfo.InvariantCulture);
                int decimalPlaces = epsilonString.Length - epsilonString.IndexOf('.') - 1;
                return decimalPlaces;
            }
        }

        public class LocalMinimumFinder
        {
            private readonly Func<double, double> Function;
            private readonly Func<double, double> Derivative;

            public LocalMinimumFinder(Func<double, double> function, Func<double, double> derivative)
            {
                Function = function;
                Derivative = derivative;
            }

            public double FindMinimum(double a, double b, double epsilon)
            {
                double min;
                double delta = epsilon / 10;

                while (b - a >= epsilon)
                {
                    double middle = (a + b) / 2;
                    double lambda = middle - delta, mu = middle + delta;

                    if (Function(lambda) < Function(mu))
                        b = mu;
                    else
                        a = lambda;
                }

                min = (a + b) / 2;

                return min;
            }
        }

        public class LocalMaximumFinder
        {
            private readonly Func<double, double> Function;
            private readonly Func<double, double> Derivative;

            public LocalMaximumFinder(Func<double, double> function, Func<double, double> derivative)
            {
                Function = function;
                Derivative = derivative;
            }

            public double FindMaximum(double a, double b, double epsilon)
            {
                double max;
                double delta = epsilon / 10;

                while (b - a >= epsilon)
                {
                    double middle = (a + b) / 2;
                    double lambda = middle - delta, mu = middle + delta;

                    if (Function(lambda) > Function(mu))
                        b = mu;
                    else
                        a = lambda;
                }

                max = (a + b) / 2;

                return max;
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

            var model = new PlotModel { Title = "График функции" };

            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MajorGridlineColor = OxyColors.LightGray });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MajorGridlineColor = OxyColors.LightGray });

            var lineSeries = new LineSeries();
            for (double x = expandedA; x <= expandedB; x += 0.01)
            {
                lineSeries.Points.Add(new DataPoint(x, function(x)));
            }
            model.Series.Add(lineSeries);

            model.Annotations.Add(new OxyPlot.Annotations.PointAnnotation
            {
                X = minResult,
                Y = function(minResult),
                Text = "Min",
                TextPosition = new DataPoint(minResult, function(minResult) - 0.5),
                TextColor = OxyColors.Blue
            });
            model.Annotations.Add(new OxyPlot.Annotations.PointAnnotation
            {
                X = maxResult,
                Y = function(maxResult),
                Text = "Max",
                TextPosition = new DataPoint(maxResult, function(maxResult) + 0.5),
                TextColor = OxyColors.Blue
            });

            var rootAnnotation = new OxyPlot.Annotations.PointAnnotation
            {
                X = rootResult,
                Y = 0,
                Text = "Root",
                TextPosition = new DataPoint(rootResult, 1),
                TextColor = OxyColors.Green
            };
            model.Annotations.Add(rootAnnotation);

            model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation { Type = OxyPlot.Annotations.LineAnnotationType.Horizontal, Color = OxyColors.Green, Y = rootResult });
            model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation { Type = OxyPlot.Annotations.LineAnnotationType.Vertical, Color = OxyColors.Green, X = rootResult });

            plotView.Model = model;
        }
    }
}
