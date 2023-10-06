using System;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.WindowsForms;

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
        //private Label resultLabel;
        private TextBox localPointsTextBox;


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

            // Создание Label для вывода результатов
           /* resultLabel = new Label();
            resultLabel.AutoSize = true;
            resultLabel.Location = new System.Drawing.Point(10, 160);
            this.Controls.Add(resultLabel);*/


            localPointsTextBox = new TextBox();
            localPointsTextBox.Multiline = true;
            localPointsTextBox.ScrollBars = ScrollBars.Vertical;
            localPointsTextBox.Location = new System.Drawing.Point(10, 190); // Выберите подходящее положение
            localPointsTextBox.Size = new System.Drawing.Size(300, 60); // Выберите подходящий размер
            this.Controls.Add(localPointsTextBox);

        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            // Проверка введенных данных
            if (!double.TryParse(textBoxA.Text, out double a) ||
              !double.TryParse(textBoxB.Text, out double b) ||
              !double.TryParse(textBoxE.Text, out double epsilon))
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения для a, b и e.");
                return;
            }

            // Функция для оптимизации
            Func<double, double> function = x => (27 - 18 * x + 2 * Math.Pow(x, 2)) * Math.Pow(Math.E, -x / 3);
            MessageBox.Show("Начинаю вычисления...");

            // Вычисление минимума и максимума с использованием метода половинного деления
            double minResult = OptimizationMethod(function, a, b, epsilon, OptimizationType.Minimum);
            double maxResult = OptimizationMethod(function, a, b, epsilon, OptimizationType.Maximum);

            // Вывод результатов в Label
            localPointsTextBox.AppendText($"Локальный минимум: {minResult}\n");
            localPointsTextBox.AppendText($"Локальный максимум: {maxResult}\\n");

            MessageBox.Show($"Локальный минимум: {minResult}\nЛокальный максимум: {maxResult}");
            //MessageBox.Show(resultLabel.Text, "Результаты выполнены");
            MessageBox.Show(" Перехожу к отображению графика.");

            // Показать график автоматически после расчета
            ShowGraph(a, b, function);
        }


        private void ClearButton_Click(object sender, EventArgs e)
        {
            // Очистка текстовых полей и Label
            textBoxA.Clear();
            textBoxB.Clear();
            textBoxE.Clear();
            //resultLabel.Text = string.Empty;
            localPointsTextBox.Text = string.Empty;
        }

        private double OptimizationMethod(Func<double, double> function, double a, double b, double epsilon, OptimizationType type)
        {
            double x1, x2;
            do
            {
                x1 = (a + b - epsilon) / 2;
                x2 = (a + b + epsilon) / 2;

                double f1 = function(x1);
                double f2 = function(x2);

                if ((type == OptimizationType.Minimum && f1 < f2) || (type == OptimizationType.Maximum && f1 > f2))
                {
                    b = x2;
                }
                else
                {
                    a = x1;
                }
            } while (Math.Abs(b - a) > epsilon);

            return (a + b) / 2;
        }

        private void ShowGraph(double a, double b, Func<double, double> function)
        {
            // Увеличиваем интервал для построения графика
            double expandedA = a - 1;
            double expandedB = b + 1;

            // Создание новой формы с графиком
            using (GraphForm graphForm = new GraphForm(expandedA, expandedB, function))
            {
                graphForm.ShowDialog();
            }
        }

        private enum OptimizationType
        {
            Minimum,
            Maximum
        }
    }

    public class GraphForm : Form
    {
        private PlotView plotView;

        public GraphForm(double a, double b, Func<double, double> function)
        {
            InitializeComponents(a, b, function);
        }

        private void InitializeComponents(double a, double b, Func<double, double> function)
        {
            // Увеличиваем интервал для построения графика
            double expandedA = a - 1;
            double expandedB = b + 1;

            // Создание PlotView для отображения графика
            plotView = new PlotView();
            plotView.Dock = DockStyle.Fill;
            this.Controls.Add(plotView);

            // Создание модели графика
            var model = new PlotModel { Title = "График функции" };

            // Добавление серии для графика
            var lineSeries = new OxyPlot.Series.LineSeries();
            for (double x = expandedA; x <= expandedB; x += 0.1)
            {
                lineSeries.Points.Add(new OxyPlot.DataPoint(x, function(x)));
            }
            model.Series.Add(lineSeries);

            // Вычисление и добавление точек минимума и максимума
            double minResult = OptimizationMethod(function, a, b, 0.001, OptimizationType.Minimum);
            double maxResult = OptimizationMethod(function, a, b, 0.001, OptimizationType.Maximum);

            model.Annotations.Add(new OxyPlot.Annotations.PointAnnotation { X = minResult, Y = function(minResult), Text = "Min" });
            model.Annotations.Add(new OxyPlot.Annotations.PointAnnotation { X = maxResult, Y = function(maxResult), Text = "Max" });

            // Установка модели для PlotView
            plotView.Model = model;
        }

        private double OptimizationMethod(Func<double, double> function, double a, double b, double epsilon, OptimizationType type)
        {
            double x1, x2;
            do
            {
                x1 = (a + b - epsilon) / 2;
                x2 = (a + b + epsilon) / 2;

                double f1 = function(x1);
                double f2 = function(x2);

                if ((type == OptimizationType.Minimum && f1 < f2) || (type == OptimizationType.Maximum && f1 > f2))
                {
                    b = x2;
                }
                else
                {
                    a = x1;
                }
            } while (Math.Abs(b - a) > epsilon && a <= b);

            return (a + b) / 2;
        }

        private enum OptimizationType
        {
            Minimum,
            Maximum
        }
    }
}
