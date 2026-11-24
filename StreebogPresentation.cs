using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Streebog
{
    public class StreebogPresentation : UserControl
    {
        private TextBlock titleText;
        private TextBlock phaseText;
        private TextBlock descText;
        private TextBlock infoText;
        private TextBlock opText;
        private TextBox ivBox;
        private TextBox hBox;
        private TextBox nBox;
        private TextBox sigmaBox;
        private TextBox mBox;
        private TextBox dataBox;
        private TextBox kiBox;
        private TextBox digestBox;
        private ListBox logBox;
        private UniformGrid stateGrid;
        private readonly TextBlock[] stateCells = new TextBlock[64];
        private Button nextButton;
        private Button autoButton;
        private Button skipgButton;
        private Button skipButton;
        private Slider speedSlider;
        private Grid introPanel;
        private Grid mainPanel;

        private readonly AutoResetEvent nextEvent = new AutoResetEvent(false);
        private volatile bool auto;
        private volatile bool skip;
        private volatile bool skipg;
        private volatile bool stopped;
        private int delayMs = 200;

        public StreebogPresentation()
        {
            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            introPanel = BuildIntro();
            mainPanel = BuildMain();
            mainPanel.Visibility = Visibility.Collapsed;

            root.Children.Add(introPanel);
            root.Children.Add(mainPanel);

            Content = root;
            MinWidth = 860;
            MinHeight = 560;
        }

        private Grid BuildIntro()
        {
            Grid g = new Grid { Margin = new Thickness(16), Background = Brushes.White };
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            titleText = new TextBlock { Text = "ГОСТ Р 34.11-2012 — Стрибог", FontSize = 28, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 8) };
            Grid.SetRow(titleText, 0);
            g.Children.Add(titleText);

            TextBlock intro = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 12),
                Text = "Стрибог — российский криптографический стандарт хэширования (ГОСТ Р 34.11-2012). " +
                       "Он обрабатывает 512-битные блоки функцией сжатия g_N(h, m) на основе преобразований LPS и X, " +
                       "накапливает N (длину сообщения в битах) и Σ (сумму блоков сообщения), а завершает вычисление вызовами g_0(h, N) и g_0(h, Σ). " +
                       "Поддерживаемые размеры хэша: 256 и 512 бит.\n\n" +
                       "Эта презентация показывает внутреннее состояние алгоритма по шагам: сжатие блоков, 12 раундов g и финализация.\n" +
                       "Все 64-битные слова показаны как байты в порядке следования в хэше (little-endian), поэтому h совпадает с итоговым хэшем."
            };
            Grid.SetRow(intro, 1);
            g.Children.Add(intro);

            Border info = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1), Padding = new Thickness(10), Background = new SolidColorBrush(Color.FromRgb(245, 248, 252)) };
            info.Child = new TextBlock
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Text = "Инициализация: h := IV (по умолчанию 512 бит: 00…00; 256 бит: 01…01 в каждом байте; IV можно задать входом);  N := 0;  Σ := 0\n" +
                       "Для каждого полного блока m (512 бит):\n" +
                       "  h := g_N(h, m);  N := N + 512;  Σ := Σ + m      (mod 2^512)\n" +
                       "Последний блок (|m| бит, 0 ≤ |m| < 512):  m_pad := m ‖ 0x01 ‖ 00…00\n" +
                       "  h := g_N(h, m_pad);  N := N + |m|;  Σ := Σ + m_pad\n" +
                       "  h := g_0(h, N);  h := g_0(h, Σ)\n" +
                       "Результат: 512 бит — h целиком;  256 бит — старшие 256 бит h\n\n" +
                       "g_N(h, m):\n" +
                       "  K := LPS(h ⊕ N)\n" +
                       "  data := K ⊕ m\n" +
                       "  for r = 1..12:\n" +
                       "    data := LPS(data)\n" +
                       "    K := LPS(K ⊕ C[r-1])\n" +
                       "    data := data ⊕ K\n" +
                       "  h := data ⊕ h ⊕ m\n\n" +
                       "LPS(x) = L(P(S(x)));  g_0(h, m) — то же, что g_N(h, m) при N = 0"
            };
            Grid.SetRow(info, 2);
            g.Children.Add(info);

            Button startBtn = new Button { Content = "Старт / Продолжить", MinWidth = 140, Height = 32, Margin = new Thickness(0, 14, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
            startBtn.Click += (s, e) => { introPanel.Visibility = Visibility.Collapsed; mainPanel.Visibility = Visibility.Visible; nextEvent.Set(); };
            Grid.SetRow(startBtn, 3);
            g.Children.Add(startBtn);

            return g;
        }

        private Grid BuildMain()
        {
            Grid g = new Grid { Margin = new Thickness(8) };
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            StackPanel header = new StackPanel();
            phaseText = new TextBlock { FontSize = 22, FontWeight = FontWeights.Bold, Text = "Streebog" };
            descText = new TextBlock { FontSize = 14, Margin = new Thickness(0, 2, 0, 0), TextWrapping = TextWrapping.Wrap };
            infoText = new TextBlock { FontSize = 12, Foreground = Brushes.DimGray, Margin = new Thickness(0, 2, 0, 6), TextWrapping = TextWrapping.Wrap };
            header.Children.Add(phaseText);
            header.Children.Add(descText);
            header.Children.Add(infoText);
            Grid.SetRow(header, 0);
            g.Children.Add(header);

            Grid center = new Grid();
            center.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
            center.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(center, 1);

            StackPanel left = new StackPanel { Margin = new Thickness(0, 0, 8, 0) };
            left.Children.Add(new TextBlock { Text = "Состояние h (8×8 байт)", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) });
            stateGrid = new UniformGrid { Rows = 8, Columns = 8, Margin = new Thickness(0, 0, 0, 8) };
            for (int i = 0; i < 64; i++)
            {
                Border b = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0.5), Background = Brushes.WhiteSmoke, Padding = new Thickness(2) };
                TextBlock tb = new TextBlock { Text = "00", FontFamily = new FontFamily("Consolas"), FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center };
                stateCells[i] = tb;
                b.Child = tb;
                stateGrid.Children.Add(b);
            }
            left.Children.Add(stateGrid);
            left.Children.Add(new TextBlock { Text = "Хэш:", FontWeight = FontWeights.SemiBold });
            digestBox = MakeBox(60);
            digestBox.Text = "(нажмите кнопку Далее для вычисления хэша)";
            left.Children.Add(digestBox);
            left.Children.Add(new TextBlock { Text = "Журнал шагов:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 6, 0, 2) });
            logBox = new ListBox { Height = 140, FontFamily = new FontFamily("Consolas"), FontSize = 11 };
            left.Children.Add(logBox);
            Grid.SetColumn(left, 0);
            center.Children.Add(left);

            StackPanel right = new StackPanel();
            opText = new TextBlock { FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(30, 80, 160)), Margin = new Thickness(0, 0, 0, 6), TextWrapping = TextWrapping.Wrap };
            right.Children.Add(opText);
            right.Children.Add(MakeLabel("IV (вектор инициализации, байты по порядку)"));
            ivBox = MakeBox(44); right.Children.Add(ivBox);
            right.Children.Add(MakeLabel("h (хэш-состояние, байты по порядку)"));
            hBox = MakeBox(44); right.Children.Add(hBox);
            right.Children.Add(MakeLabel("N (счётчик битов mod 2^512, байты по порядку)"));
            nBox = MakeBox(28); right.Children.Add(nBox);
            right.Children.Add(MakeLabel("Σ (сумма блоков mod 2^512, байты по порядку)"));
            sigmaBox = MakeBox(28); right.Children.Add(sigmaBox);
            right.Children.Add(MakeLabel("m (текущий блок)"));
            mBox = MakeBox(28); right.Children.Add(mBox);
            right.Children.Add(MakeLabel("data (внутреннее состояние g)"));
            dataBox = MakeBox(28); right.Children.Add(dataBox);
            right.Children.Add(MakeLabel("K (раундовый ключ g)"));
            kiBox = MakeBox(28); right.Children.Add(kiBox);
            Grid.SetColumn(right, 1);
            center.Children.Add(right);

            g.Children.Add(center);

            StackPanel controls = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
            nextButton = MakeButton("Далее", Next_Click);
            autoButton = MakeButton("Авто", Auto_Click);
            skipgButton = MakeButton("Пропустить g", Skipg_Click);
            skipButton = MakeButton("Пропустить всё", Skip_Click);
            controls.Children.Add(nextButton);
            controls.Children.Add(autoButton);
            controls.Children.Add(skipgButton);
            controls.Children.Add(skipButton);
            controls.Children.Add(new TextBlock { Text = "Скорость:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 4, 0) });
            speedSlider = new Slider { Minimum = 10, Maximum = 1200, Value = 200, Width = 150, VerticalAlignment = VerticalAlignment.Center };
            speedSlider.ValueChanged += (s, e) => delayMs = (int)speedSlider.Value;
            controls.Children.Add(speedSlider);
            Button introBtn = MakeButton("Интро", (s, e) => { mainPanel.Visibility = Visibility.Collapsed; introPanel.Visibility = Visibility.Visible; });
            introBtn.Margin = new Thickness(12, 0, 0, 0);
            controls.Children.Add(introBtn);
            controls.Children.Add(new TextBlock { Text = "Таблицы:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 4, 0) });
            controls.Children.Add(MakeButton("S", (s, e) => ShowTable("S")));
            controls.Children.Add(MakeButton("P", (s, e) => ShowTable("P")));
            controls.Children.Add(MakeButton("L", (s, e) => ShowTable("L")));
            Grid.SetRow(controls, 2);
            g.Children.Add(controls);

            return g;
        }

        private static TextBlock MakeLabel(string text)
        {
            return new TextBlock { Text = text, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 4, 0, 1) };
        }

        private static TextBox MakeBox(double minHeight)
        {
            return new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                MinHeight = minHeight,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.LightGray,
                Background = Brushes.WhiteSmoke,
                Padding = new Thickness(4),
                Margin = new Thickness(0, 0, 0, 2)
            };
        }

        private static Button MakeButton(string text, RoutedEventHandler handler)
        {
            Button b = new Button { Content = text, MinWidth = 76, Margin = new Thickness(0, 0, 6, 0), Padding = new Thickness(8, 4, 8, 4) };
            b.Click += handler;
            return b;
        }

        public void Reset()
        {
            stopped = false;
            skip = false;
            skipg = false;
            auto = false;
            nextEvent.Reset();
            RunOnUi(() =>
            {
                introPanel.Visibility = Visibility.Collapsed;
                mainPanel.Visibility = Visibility.Visible;
                phaseText.Text = "Стрибог";
                descText.Text = "Готово";
                infoText.Text = "";
                opText.Text = "";
                ivBox.Text = hBox.Text = nBox.Text = sigmaBox.Text = mBox.Text = dataBox.Text = kiBox.Text = "";
                digestBox.Text = "(выполняется...)";
                logBox.Items.Clear();
                autoButton.Content = "Авто";
                ClearStateGrid();
            });
        }

        public void Stop()
        {
            stopped = true;
            skip = true;
            nextEvent.Set();
        }

        public bool IsSkip { get { return skip; } }
        public bool IsStopped { get { return stopped; } }

        public void OnStep(StreebogTraceStep step)
        {
            if (step == null) return;
            RunOnUi(() => RenderStep(step));

            if (stopped || skip) return;

            if (skipg)
            {
                bool inG = step.Phase != null && step.Phase.IndexOf("g/", StringComparison.Ordinal) == 0;
                bool gEnd = step.IsGEnd;
                if (inG && !gEnd) return;
                skipg = false;
            }

            if (auto)
            {
                int waited = 0;
                while (waited < delayMs && !stopped && !skip && !skipg && auto)
                {
                    if (nextEvent.WaitOne(20)) break;
                    waited += 20;
                }
                return;
            }

            while (!stopped && !skip && !skipg && !auto)
            {
                if (nextEvent.WaitOne(50)) break;
            }
        }

        public void SetOutput(byte[] value)
        {
            RunOnUi(() =>
            {
                digestBox.Text = value == null || value.Length == 0 ? "(нет вывода)" : ToHex(value);
            });
        }

        private void RenderStep(StreebogTraceStep step)
        {
            phaseText.Text = "Стрибог " + step.DigestSize + " — " + (step.Phase ?? "");
            descText.Text = step.Description ?? "";
            infoText.Text = "Блок: " + (step.BlockIndex + 1) + (step.BlockCount > 0 ? "/" + step.BlockCount : "") +
                            "   Раунд: " + (step.Round > 0 ? step.Round + "/12" : (step.Round == 0 ? "подготовка" : "-")) +
                            (step.IsFinal ? "   [ГОТОВО]" : "");
            opText.Text = string.IsNullOrEmpty(step.Operation) ? "" :  "Операция: " + step.Operation;

            ivBox.Text = FormatUlongs(step.Iv);
            hBox.Text = FormatUlongs(step.H);
            nBox.Text = FormatUlongs(step.N);
            sigmaBox.Text = FormatUlongs(step.Sigma);
            mBox.Text = FormatUlongs(step.M);
            dataBox.Text = FormatUlongs(step.Data);
            kiBox.Text = FormatUlongs(step.Ki);

            UpdateStateGrid(step.H);

            string log = (step.Phase ?? "") + " | " + (step.Description ?? "");
            if (!string.IsNullOrEmpty(step.Operation)) log += " [" + step.Operation + "]";
            logBox.Items.Add(log);
            if (logBox.Items.Count > 0) logBox.ScrollIntoView(logBox.Items[logBox.Items.Count - 1]);
        }

        private void UpdateStateGrid(ulong[] h)
        {
            byte[] bytes = new byte[64];
            if (h != null)
            {
                for (int i = 0; i < 8 && i < h.Length; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        bytes[i * 8 + j] = (byte)((h[i] >> (8 * j)) & 0xFF);
                    }
                }
            }
            for (int i = 0; i < 64; i++)
            {
                string hex = bytes[i].ToString("X2");
                if (stateCells[i].Text != hex)
                {
                    stateCells[i].Text = hex;
                    stateCells[i].Background = Brushes.LightYellow;
                }
                else
                {
                    stateCells[i].Background = Brushes.Transparent;
                }
            }
        }

        private void ClearStateGrid()
        {
            for (int i = 0; i < 64; i++)
            {
                stateCells[i].Text = "00";
                stateCells[i].Background = Brushes.Transparent;
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            auto = false;
            skipg = false;
            autoButton.Content = "Авто";
            nextEvent.Set();
        }

        private void Auto_Click(object sender, RoutedEventArgs e)
        {
            auto = !auto;
            skipg = false;
            autoButton.Content = auto ? "Пауза" : "Авто";
            if (auto) nextEvent.Set();
        }

        private void Skipg_Click(object sender, RoutedEventArgs e)
        {
            auto = false;
            autoButton.Content = "Авто";
            skipg = true;
            nextEvent.Set();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            skip = true;
            skipg = false;
            auto = false;
            autoButton.Content = "Авто";
            nextEvent.Set();
        }

        private void ShowTable(string kind)
        {
            string title;
            string content;
            if (kind == "S")
            {
                title = "Таблица S (подстановка)";
                StringBuilder sb = new StringBuilder();
                sb.Append("     ");
                for (int c = 0; c < 16; c++) sb.AppendFormat(" {0:X}", c);
                sb.AppendLine();
                for (int r = 0; r < 16; r++)
                {
                    sb.AppendFormat("{0:X2}: ", r * 16);
                    for (int c = 0; c < 16; c++)
                    {
                        int idx = r * 16 + c;
                        sb.AppendFormat(" {0:X2}", StreebogTables.Pi[idx]);
                    }
                    sb.AppendLine();
                }
                content = sb.ToString();
            }
            else if (kind == "P")
            {
                title = "Таблица P (перестановка байт)";
                StringBuilder sb = new StringBuilder();
                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        int k = r * 8 + c;
                        sb.AppendFormat("{0,2}->{1,2}  ", k, StreebogTables.Tau[k]);
                    }
                    sb.AppendLine();
                }
                content = sb.ToString();
            }
            else
            {
                title = "Таблица L (матрица A, 64x64)";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("L: линейное преобразование над GF(2), умножение на матрицу A (64 строки по 64 бита).");
                sb.AppendLine();
                for (int j = 0; j < StreebogTables.A.Length; j++)
                {
                    sb.AppendFormat("A[{0,2}] = {1:X16}", j, StreebogTables.A[j]);
                    sb.AppendLine();
                }
                content = sb.ToString();
            }

            Window w = new Window
            {
                Title = title,
                Width = kind == "L" ? 560 : 640,
                Height = 520,
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBox
                {
                    Text = content,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 13,
                    IsReadOnly = true,
                    TextWrapping = TextWrapping.NoWrap,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(8)
                }
            };
            w.Show();
        }

        private void RunOnUi(Action action)
        {
            if (Dispatcher.CheckAccess()) { action(); return; }
            Dispatcher.Invoke(action, DispatcherPriority.Background);
        }

        private static string FormatUlongs(ulong[] v)
        {
            if (v == null) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < v.Length; i++)
            {
                if (i > 0) sb.Append(" ");
                for (int j = 0; j < 8; j++)
                {
                    sb.Append(((byte)((v[i] >> (8 * j)) & 0xFFUL)).ToString("X2"));
                }
            }
            return sb.ToString();
        }

        private static string ToHex(byte[] value)
        {
            StringBuilder sb = new StringBuilder(value.Length * 2);
            for (int i = 0; i < value.Length; i++) sb.Append(value[i].ToString("X2"));
            return sb.ToString();
        }
    }
}
