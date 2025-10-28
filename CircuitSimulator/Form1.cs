using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CircuitSimulator
{
    public class MainForm : Form
    {
        // Элементы управления
        private NumericUpDown nudR1, nudR2, nudL, nudC, nudJ, nudTimeStep, nudDuration;
        private Button btnSimulate, btnReset;
        private Chart chartVoltage, chartCurrent, chartAll;
        private TabControl tabControl;
        private Label lblStatus;

        // Параметры схемы (значения по умолчанию)
        private double R1 = 100;   // Ом
        private double R2 = 200;   // Ом
        private double L = 0.1;    // Гн
        private double C = 0.0001; // Ф (100 мкФ)
        private double J = 0.05;   // А (50 мА)
        private double h = 0.0001; // шаг интегрирования (с)
        private double duration = 0.05; // длительность моделирования (с)

        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Моделирование электрической цепи - Метод переменных состояния";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель параметров
            Panel paramPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                Padding = new Padding(10),
                BackColor = Color.WhiteSmoke
            };

            int yPos = 10;

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "Параметры схемы",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            paramPanel.Controls.Add(lblTitle);
            yPos += 35;

            // Создание элементов управления для параметров
            CreateParameter(paramPanel, "R1 (Ом):", ref nudR1, R1, 1, 10000, ref yPos);
            CreateParameter(paramPanel, "R2 (Ом):", ref nudR2, R2, 1, 10000, ref yPos);
            CreateParameter(paramPanel, "L (Гн):", ref nudL, L, 0.001, 10, ref yPos, 0.001);
            CreateParameter(paramPanel, "C (Ф):", ref nudC, C, 0.00001, 0.01, ref yPos, 0.00001);
            CreateParameter(paramPanel, "J (А):", ref nudJ, J, 0.001, 1, ref yPos, 0.001);

            yPos += 20;
            Label lblSim = new Label
            {
                Text = "Параметры моделирования",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            paramPanel.Controls.Add(lblSim);
            yPos += 30;

            CreateParameter(paramPanel, "Шаг (с):", ref nudTimeStep, h, 0.00001, 0.01, ref yPos, 0.00001);
            CreateParameter(paramPanel, "Время (с):", ref nudDuration, duration, 0.001, 1, ref yPos, 0.001);

            // Кнопки
            yPos += 20;
            btnSimulate = new Button
            {
                Text = "Запустить моделирование",
                Location = new Point(10, yPos),
                Size = new Size(220, 40),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnSimulate.Click += BtnSimulate_Click;
            paramPanel.Controls.Add(btnSimulate);
            yPos += 50;

            btnReset = new Button
            {
                Text = "Сбросить",
                Location = new Point(10, yPos),
                Size = new Size(220, 35),
                BackColor = Color.LightCoral
            };
            btnReset.Click += BtnReset_Click;
            paramPanel.Controls.Add(btnReset);

            // Статус
            yPos += 50;
            lblStatus = new Label
            {
                Text = "Готов к моделированию",
                Location = new Point(10, yPos),
                Size = new Size(220, 60),
                ForeColor = Color.Blue,
                Font = new Font("Arial", 9)
            };
            paramPanel.Controls.Add(lblStatus);

            // Создаем графики
            chartVoltage = CreateChart("Напряжение на конденсаторе и ток в индуктивности");
            chartCurrent = CreateChart("Выходные токи i2 и i3");
            chartAll = CreateChart("Все переменные состояния");

            // Создание TabControl с вкладками
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            // Вкладка 1
            TabPage tab1 = new TabPage("Uc и iL");
            chartVoltage.Dock = DockStyle.Fill;
            tab1.Controls.Add(chartVoltage);
            tabControl.TabPages.Add(tab1);

            // Вкладка 2
            TabPage tab2 = new TabPage("Токи i2 и i3");
            chartCurrent.Dock = DockStyle.Fill;
            tab2.Controls.Add(chartCurrent);
            tabControl.TabPages.Add(tab2);

            // Вкладка 3
            TabPage tab3 = new TabPage("Все вместе");
            chartAll.Dock = DockStyle.Fill;
            tab3.Controls.Add(chartAll);
            tabControl.TabPages.Add(tab3);

            // Добавляем элементы на форму В ПРАВИЛЬНОМ ПОРЯДКЕ
            this.Controls.Add(tabControl);  // Сначала TabControl
            this.Controls.Add(paramPanel);  // Потом панель параметров
        }

        private void CreateParameter(Panel panel, string label, ref NumericUpDown nud,
            double defaultValue, double min, double max, ref int yPos, double increment = 1)
        {
            Label lbl = new Label
            {
                Text = label,
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lbl);

            nud = new NumericUpDown
            {
                Location = new Point(100, yPos - 3),
                Size = new Size(130, 25),
                Minimum = (decimal)min,
                Maximum = (decimal)max,
                Value = (decimal)defaultValue,
                DecimalPlaces = 5,
                Increment = (decimal)increment
            };
            panel.Controls.Add(nud);
            yPos += 35;
        }

        private Chart CreateChart(string title)
        {
            Chart chart = new Chart
            {
                BackColor = Color.White
            };

            ChartArea chartArea = new ChartArea
            {
                BackColor = Color.WhiteSmoke,
                AxisX = {
                    Title = "Время (с)",
                    TitleFont = new Font("Arial", 10, FontStyle.Bold),
                    LabelStyle = { Format = "0.####" }
                },
                AxisY = {
                    Title = "Значение",
                    TitleFont = new Font("Arial", 10, FontStyle.Bold),
                    LabelStyle = { Format = "0.####" }
                }
            };
            chart.ChartAreas.Add(chartArea);

            Legend legend = new Legend
            {
                Docking = Docking.Top,
                Font = new Font("Arial", 9)
            };
            chart.Legends.Add(legend);

            Title chartTitle = new Title
            {
                Text = title,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            chart.Titles.Add(chartTitle);

            return chart;
        }

        private void BtnSimulate_Click(object sender, EventArgs e)
        {
            try
            {
                // Считывание параметров
                R1 = (double)nudR1.Value;
                R2 = (double)nudR2.Value;
                L = (double)nudL.Value;
                C = (double)nudC.Value;
                J = (double)nudJ.Value;
                h = (double)nudTimeStep.Value;
                duration = (double)nudDuration.Value;

                lblStatus.Text = "Моделирование...";
                lblStatus.ForeColor = Color.Orange;
                Application.DoEvents();

                // Запуск моделирования
                SimulateCircuit();

                lblStatus.Text = "Моделирование завершено!";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Ошибка моделирования";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void SimulateCircuit()
        {
            // Начальные условия (ключ разомкнут)
            double uC = J * R2;  // напряжение на конденсаторе
            double iL = 0;       // ток в индуктивности

            List<double> timePoints = new List<double>();
            List<double> ucValues = new List<double>();
            List<double> ilValues = new List<double>();
            List<double> i2Values = new List<double>();
            List<double> i3Values = new List<double>();

            int N = (int)(duration / h);
            double t = 0;

            // Метод Эйлера
            for (int n = 0; n <= N; n++)
            {
                // Сохранение текущих значений
                timePoints.Add(t);
                ucValues.Add(uC);
                ilValues.Add(iL);

                // Вычисление выходных токов
                double i2 = uC / R2;
                double i3 = -uC / R2 - iL + J;
                i2Values.Add(i2);
                i3Values.Add(i3);

                // Матрицы A, B 
                // dX/dt = A*X + B*V
                // X = [uC; iL]
                // A = [[-1/(C*R2), -1/C], [1/L, -R1/L]]
                // B = [[1/C], [0]]

                double duc_dt = -1.0 / (C * R2) * uC - 1.0 / C * iL + 1.0 / C * J;
                double dil_dt = 1.0 / L * uC - R1 / L * iL;

                // Обновление переменных состояния (метод Эйлера)
                uC = uC + h * duc_dt;
                iL = iL + h * dil_dt;
                t = t + h;
            }

            // Построение графиков
            PlotResults(timePoints, ucValues, ilValues, i2Values, i3Values);
        }

        private void PlotResults(List<double> time, List<double> uC, List<double> iL,
            List<double> i2, List<double> i3)
        {
            // График 1: напряжение и ток в индуктивности
            chartVoltage.Series.Clear();

            Series seriesUc = new Series("Uc (В)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Blue,
                BorderWidth = 2
            };

            Series seriesIl = new Series("iL (А)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Red,
                BorderWidth = 2
            };

            for (int i = 0; i < time.Count; i++)
            {
                seriesUc.Points.AddXY(time[i], uC[i]);
                seriesIl.Points.AddXY(time[i], iL[i]);
            }

            chartVoltage.Series.Add(seriesUc);
            chartVoltage.Series.Add(seriesIl);
            chartVoltage.ChartAreas[0].RecalculateAxesScale();

            // График 2: выходные токи
            chartCurrent.Series.Clear();

            Series seriesI2 = new Series("i2 (А)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Green,
                BorderWidth = 2
            };

            Series seriesI3 = new Series("i3 (А)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Purple,
                BorderWidth = 2
            };

            for (int i = 0; i < time.Count; i++)
            {
                seriesI2.Points.AddXY(time[i], i2[i]);
                seriesI3.Points.AddXY(time[i], i3[i]);
            }

            chartCurrent.Series.Add(seriesI2);
            chartCurrent.Series.Add(seriesI3);
            chartCurrent.ChartAreas[0].RecalculateAxesScale();

            // График 3: все переменные вместе
            chartAll.Series.Clear();

            Series sUc = new Series("Uc (В)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Blue,
                BorderWidth = 2
            };
            Series sIl = new Series("iL (А)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Red,
                BorderWidth = 2
            };
            Series sI2 = new Series("i2 (А)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Green,
                BorderWidth = 2
            };
            Series sI3 = new Series("i3 (А)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Purple,
                BorderWidth = 2
            };

            for (int i = 0; i < time.Count; i++)
            {
                sUc.Points.AddXY(time[i], uC[i]);
                sIl.Points.AddXY(time[i], iL[i]);
                sI2.Points.AddXY(time[i], i2[i]);
                sI3.Points.AddXY(time[i], i3[i]);
            }

            chartAll.Series.Add(sUc);
            chartAll.Series.Add(sIl);
            chartAll.Series.Add(sI2);
            chartAll.Series.Add(sI3);
            chartAll.ChartAreas[0].RecalculateAxesScale();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            nudR1.Value = 100;
            nudR2.Value = 200;
            nudL.Value = 0.1m;
            nudC.Value = 0.0001m;
            nudJ.Value = 0.05m;
            nudTimeStep.Value = 0.0001m;
            nudDuration.Value = 0.05m;

            chartVoltage.Series.Clear();
            chartCurrent.Series.Clear();
            chartAll.Series.Clear();

            lblStatus.Text = "Параметры сброшены";
            lblStatus.ForeColor = Color.Blue;
        }


    }
}