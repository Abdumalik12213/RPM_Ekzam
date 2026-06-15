using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Timer = System.Windows.Forms.Timer;

namespace CSharpMaster
{
    public partial class Form1 : Form
    {
        // === ДАННЫЕ ===
        private UserData user = new UserData();
        private Random rand = new Random();
        private List<TaskItem> tasks = new List<TaskItem>();
        private List<QuizItem> quizzes = new List<QuizItem>();
        private TaskItem currentTask;
        private QuizItem currentQuiz;
        private string saveFile;
        private Panel currentPanel;
        private RichTextBox codeBox, outputBox, theoryBox, exampleBox, exampleOut;
        private Label lblLevel, lblExp, lblStreak, lblSolved, taskTitle, taskDesc, tipLabel, timerLabel, exTitle;
        private ProgressBar streakBar, levelBar;
        private RadioButton[] quizOpts = new RadioButton[4];
        private Label quizFeedback, quizQuestion;
        private Timer gameTimer;
        private int seconds = 0;
        private bool timerRun = false;
        private enum ThemeType { Dark, Light, Blue }
        private ThemeType currentTheme = ThemeType.Dark;

        private const int EXP_PER_LVL = 200, MAX_LVL = 20;

        public Form1()
        {
            // Настройки окна
            this.Text = "⚡ C# MASTER - Профессиональное обучение программированию";
            this.Size = new Size(1400, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 750);
            this.BackColor = Color.FromArgb(18, 18, 24);

            saveFile = Path.Combine(Application.StartupPath, "save.json");

            GenerateAllContent();
            LoadProgress();
            ShowMenu();
            if (user.LastPlayed.Date < DateTime.Today) { user.Exp += 100; user.LastPlayed = DateTime.Now; SaveProgress(); }
            CheckAchievements();

            tipLabel = new Label { Text = GetTip(), Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 100, 110), Dock = DockStyle.Bottom, Height = 28, TextAlign = ContentAlignment.MiddleCenter };
            this.Controls.Add(tipLabel);
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ДИЗАЙНА ===
        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private Button CreateModernButton(string text, string icon, Color color, int x, int y, int w, int h, Action action)
        {
            var btn = new Button
            {
                Text = $"{icon}  {text}",
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            btn.FlatAppearance.BorderSize = 0;

            // Эффект наведения
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color, 0.2f);
            btn.MouseLeave += (s, e) => btn.BackColor = color;
            btn.Click += (s, e) => action();

            return btn;
        }

        private void ChangeTheme()
        {
            currentTheme = (ThemeType)(((int)currentTheme + 1) % 3);

            Color backColor, panelColor;

            switch (currentTheme)
            {
                case ThemeType.Dark:
                    backColor = Color.FromArgb(18, 18, 24);
                    panelColor = Color.FromArgb(30, 30, 40);
                    break;
                case ThemeType.Light:
                    backColor = Color.FromArgb(240, 240, 245);
                    panelColor = Color.FromArgb(255, 255, 255);
                    break;
                case ThemeType.Blue:
                    backColor = Color.FromArgb(25, 50, 80);
                    panelColor = Color.FromArgb(35, 65, 100);
                    break;
                default:
                    backColor = Color.FromArgb(18, 18, 24);
                    panelColor = Color.FromArgb(30, 30, 40);
                    break;
            }

            this.BackColor = backColor;
            MessageBox.Show($"Тема изменена на {currentTheme}", "Тема",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            ShowMenu();
        }

        // === СИСТЕМА ДОСТИЖЕНИЙ ===
        private void CheckAchievements()
        {
            var newAchievements = new List<(string name, string description)>();

            if (user.Solved >= 1 && !user.Achievements.Contains("🌱 Первые шаги"))
                newAchievements.Add(("🌱 Первые шаги", "Решить первую задачу"));

            if (user.Solved >= 10 && !user.Achievements.Contains("⭐ Новичок"))
                newAchievements.Add(("⭐ Новичок", "Решить 10 задач"));

            if (user.Solved >= 50 && !user.Achievements.Contains("📚 Ученик"))
                newAchievements.Add(("📚 Ученик", "Решить 50 задач"));

            if (user.Solved >= 100 && !user.Achievements.Contains("💪 Профи"))
                newAchievements.Add(("💪 Профи", "Решить 100 задач"));

            if (user.Solved >= 200 && !user.Achievements.Contains("⚡ Мастер"))
                newAchievements.Add(("⚡ Мастер", "Решить 200 задач"));

            if (user.Streak >= 5 && !user.Achievements.Contains("🔥 Серия 5"))
                newAchievements.Add(("🔥 Серия 5", "Серия из 5 правильных ответов"));

            if (user.Streak >= 10 && !user.Achievements.Contains("💯 Серия 10"))
                newAchievements.Add(("💯 Серия 10", "Серия из 10 правильных ответов"));

            if (user.Level >= 10 && !user.Achievements.Contains("🎯 Уровень 10"))
                newAchievements.Add(("🎯 Уровень 10", "Достичь 10 уровня"));

            if (user.Level >= 20 && !user.Achievements.Contains("👑 Легенда"))
                newAchievements.Add(("👑 Легенда", "Достичь 20 уровня"));

            foreach (var ach in newAchievements)
            {
                user.Achievements.Add(ach.name);
                ShowAchievementNotification(ach.name, ach.description);
            }

            if (newAchievements.Any())
                SaveProgress();
        }

        private void ShowAchievementNotification(string title, string description)
        {
            var notification = new Form
            {
                Text = "🏆 ДОСТИЖЕНИЕ!",
                Size = new Size(350, 120),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(50, 50, 60),
                TopMost = true
            };

            var label = new Label
            {
                Text = $"🏆 {title}\n\n{description}",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gold,
                BackColor = Color.FromArgb(50, 50, 60)
            };

            notification.Controls.Add(label);
            notification.Show();

            var timer = new Timer { Interval = 3000 };
            timer.Tick += (s, e) => { notification.Close(); timer.Stop(); };
            timer.Start();

            System.Media.SystemSounds.Asterisk.Play();
        }

        private string GetRewardForLevel(int level)
        {
            if (level == 5) return "🎁 Награда: +100 опыта!";
            if (level == 10) return "🎁 Награда: Новая тема 'LINQ' открыта!";
            if (level == 15) return "🎁 Награда: +500 опыта!";
            if (level == 20) return "👑 ТЫ ЛЕГЕНДА! Все темы открыты!";
            return "Продолжай в том же духе!";
        }

        // === ПРОФИЛЬ ПОЛЬЗОВАТЕЛЯ ===
        private void ShowProfile()
        {
            var profileForm = new Form
            {
                Text = "👤 МОЙ ПРОФИЛЬ",
                Size = new Size(500, 600),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(30, 30, 40),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = Color.FromArgb(30, 30, 40) };

            double correctPercent = user.CorrectAnswers + user.WrongAnswers > 0 ?
                (user.CorrectAnswers * 100.0 / (user.CorrectAnswers + user.WrongAnswers)) : 0;

            var info = new Label
            {
                Text = $"📊 СТАТИСТИКА\n\n" +
                       $"🏆 Ранг: {GetRank()}\n" +
                       $"⭐ Уровень: {user.Level}\n" +
                       $"💫 Опыт: {user.Exp} / {user.Level * EXP_PER_LVL}\n" +
                       $"🔥 Серия: {user.Streak}\n" +
                       $"✅ Решено задач: {user.Solved}\n" +
                       $"📈 Точность: {correctPercent:F1}%\n" +
                       $"⏱️ Время в приложении: {user.TotalTimeSpent} сек\n" +
                       $"📅 Последний визит: {user.LastPlayed:dd.MM.yyyy}\n" +
                       $"🏅 Достижений: {user.Achievements.Count}",
                Location = new Point(20, 20),
                Size = new Size(440, 300),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White
            };

            var achievementsList = new ListBox
            {
                Location = new Point(20, 330),
                Size = new Size(440, 130),
                BackColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10)
            };

            foreach (var ach in user.Achievements)
            {
                achievementsList.Items.Add($"🏆 {ach}");
            }

            if (user.Achievements.Count == 0)
                achievementsList.Items.Add("🎯 Пока нет достижений. Решайте задачи!");

            var closeBtn = new Button
            {
                Text = "ЗАКРЫТЬ",
                Location = new Point(180, 470),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(0, 150, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            closeBtn.Click += (s, e) => profileForm.Close();

            panel.Controls.Add(info);
            panel.Controls.Add(achievementsList);
            panel.Controls.Add(closeBtn);
            profileForm.Controls.Add(panel);

            profileForm.ShowDialog();
        }

        // === ГЕНЕРАЦИЯ ВСЕГО КОНТЕНТА ===
        private void GenerateAllContent()
        {
            tasks = new List<TaskItem>();
            quizzes = new List<QuizItem>();

            string[] hellos = { "Hello, World!", "Привет, мир!", "Welcome!", "Hi there!", "Greetings!", "Salut!", "Hola!", "Ciao!" };
            foreach (string h in hellos) tasks.Add(new TaskItem { Diff = 1, Title = $"Вывод '{h}'", Desc = $"Выведите '{h}'", Expected = h, Hint = $"Console.WriteLine(\"{h}\");" });

            for (int i = 1; i <= 50; i++)
            {
                tasks.Add(new TaskItem { Diff = 1, Title = $"Сумма {i}+{i + 1}", Desc = $"{i} + {i + 1} = ?", Expected = (i + i + 1).ToString(), Hint = $"Console.WriteLine({i}+{i + 1});" });
                tasks.Add(new TaskItem { Diff = 1, Title = $"Разность {i + 5}-{i}", Desc = $"{i + 5} - {i} = ?", Expected = (5).ToString(), Hint = $"Console.WriteLine({i + 5}-{i});" });
                tasks.Add(new TaskItem { Diff = 1, Title = $"Произведение {i}*{i + 1}", Desc = $"{i} * {i + 1} = ?", Expected = (i * (i + 1)).ToString(), Hint = $"Console.WriteLine({i}*{i + 1});" });
                if (i <= 30) tasks.Add(new TaskItem { Diff = 2, Title = $"Сравнение {i}>{i + 5}", Desc = $"{i} > {i + 5} ?", Expected = (i > i + 5).ToString(), Hint = $"Console.WriteLine({i}>{i + 5});" });
            }

            for (int i = 2; i <= 25; i++)
            {
                tasks.Add(new TaskItem { Diff = 2, Title = $"Чётное/нечётное {i}", Desc = $"Число {i} чётное?", Expected = i % 2 == 0 ? "Четное" : "Нечетное", Hint = $"if ({i} % 2 == 0)" });
                tasks.Add(new TaskItem { Diff = 2, Title = $"Числа 1-{i}", Desc = $"Выведите 1 до {i}", Expected = string.Join(" ", Range(1, i)), Hint = "for (int i=1; i<=...; i++)" });
                if (i <= 20) tasks.Add(new TaskItem { Diff = 3, Title = $"Сумма 1-{i}", Desc = $"1+...+{i} = ?", Expected = Sum(1, i).ToString(), Hint = "Сумма в цикле" });
                if (i <= 12) tasks.Add(new TaskItem { Diff = 3, Title = $"Факториал {i}", Desc = $"{i}! = ?", Expected = Fact(i).ToString(), Hint = "Умножение в цикле" });
            }

            int[] arrSizes = { 3, 4, 5, 6, 7, 8, 9, 10 };
            foreach (int s in arrSizes)
            {
                tasks.Add(new TaskItem { Diff = 3, Title = $"Сумма [1-{s}]", Desc = $"Сумма 1..{s}", Expected = Sum(1, s).ToString(), Hint = "Сумма в цикле" });
                tasks.Add(new TaskItem { Diff = 4, Title = $"Реверс [1-{s}]", Desc = $"Переверните 1..{s}", Expected = string.Join(" ", Reverse(Range(1, s))), Hint = "Цикл от конца к началу" });
            }

            string[] words = { "hello", "world", "csharp", "programming", "dotnet", "visual", "studio", "developer", "code", "master", "learning", "practice" };
            foreach (string w in words)
            {
                tasks.Add(new TaskItem { Diff = 4, Title = $"Длина '{w}'", Desc = $"Длина строки '{w}'", Expected = w.Length.ToString(), Hint = ".Length" });
                tasks.Add(new TaskItem { Diff = 4, Title = $"Верхний регистр '{w}'", Desc = $"'{w}' → верхний", Expected = w.ToUpper(), Hint = ".ToUpper()" });
            }

            // Вопросы
            string[] qBase = { "Кто создал C#?", "Год создания C#?", "Платформа для C#?", "Тип для целых чисел?" };
            string[] aBase = { "Хейлсберг", "2000", ".NET", "int" };
            for (int i = 0; i < qBase.Length; i++) quizzes.Add(new QuizItem { Question = qBase[i], Options = new List<string> { aBase[i], "Другое", "Не знаю", "Возможно" }, Correct = 0, Explanation = $"Ответ: {aBase[i]}" });

            string[] ops = { "&&", "||", "%", "++", "--", "==", "!=" };
            foreach (string op in ops) quizzes.Add(new QuizItem { Question = $"Что делает '{op}'?", Options = new List<string> { "Оператор", "Логический", "Арифметический", "Сравнения" }, Correct = 0, Explanation = $"Оператор {op}" });
        }

        private int[] Range(int start, int end) { int[] r = new int[end - start + 1]; for (int i = 0; i < r.Length; i++) r[i] = start + i; return r; }
        private int Sum(int start, int end) { int s = 0; for (int i = start; i <= end; i++) s += i; return s; }
        private int Fact(int n) { int f = 1; for (int i = 2; i <= n; i++) f *= i; return f; }
        private int[] Reverse(int[] arr) { int[] r = new int[arr.Length]; for (int i = 0; i < arr.Length; i++) r[i] = arr[arr.Length - 1 - i]; return r; }

        // === МЕНЮ ===
        private void ShowMenu()
        {
            ClearPanel();
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 18, 24), Padding = new Padding(40) };

            var logo = new Label
            {
                Text = "⚡ C# MASTER",
                Font = new Font("Segoe UI", 48, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 255),
                Location = new Point(40, 30),
                AutoSize = true
            };

            var slogan = new Label
            {
                Text = "Профессиональное обучение программированию на C#",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(150, 150, 170),
                Location = new Point(45, 95),
                AutoSize = true
            };

            p.Controls.Add(logo);
            p.Controls.Add(slogan);

            var statsCard = new Panel
            {
                Location = new Point(40, 140),
                Size = new Size(550, 120),
                BackColor = Color.FromArgb(30, 30, 40),
                Padding = new Padding(15)
            };

            var statsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2
            };

            statsLayout.Controls.Add(new Label { Text = "🏆 РАНГ", ForeColor = Color.Gray, Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.None }, 0, 0);
            statsLayout.Controls.Add(new Label { Text = "⭐ ОПЫТ", ForeColor = Color.Gray, Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.None }, 1, 0);
            statsLayout.Controls.Add(new Label { Text = "🔥 СЕРИЯ", ForeColor = Color.Gray, Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.None }, 2, 0);
            statsLayout.Controls.Add(new Label { Text = "✅ РЕШЕНО", ForeColor = Color.Gray, Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.None }, 3, 0);

            statsLayout.Controls.Add(new Label { Text = GetRank(), ForeColor = Color.Gold, Font = new Font("Segoe UI", 12, FontStyle.Bold), Anchor = AnchorStyles.None }, 0, 1);
            statsLayout.Controls.Add(new Label { Text = $"{user.Exp}/{user.Level * EXP_PER_LVL}", ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Anchor = AnchorStyles.None }, 1, 1);
            statsLayout.Controls.Add(new Label { Text = user.Streak.ToString(), ForeColor = Color.OrangeRed, Font = new Font("Segoe UI", 12, FontStyle.Bold), Anchor = AnchorStyles.None }, 2, 1);
            statsLayout.Controls.Add(new Label { Text = user.Solved.ToString(), ForeColor = Color.LightGreen, Font = new Font("Segoe UI", 12, FontStyle.Bold), Anchor = AnchorStyles.None }, 3, 1);

            statsCard.Controls.Add(statsLayout);
            p.Controls.Add(statsCard);

            levelBar = new ProgressBar
            {
                Location = new Point(40, 275),
                Size = new Size(550, 10),
                Maximum = 100,
                Value = GetProgress(),
                BackColor = Color.FromArgb(45, 45, 55),
                ForeColor = Color.FromArgb(0, 150, 255),
                Style = ProgressBarStyle.Continuous
            };
            p.Controls.Add(levelBar);

            int btnW = 240, btnH = 100, gap = 25;
            int startY = 320;

            p.Controls.Add(CreateModernButton("ПРАКТИКА", "💻", Color.FromArgb(0, 150, 255), 40, startY, btnW, btnH, () => StartGame("practice")));
            p.Controls.Add(CreateModernButton("ВОПРОСЫ", "❓", Color.FromArgb(230, 100, 60), 40 + btnW + gap, startY, btnW, btnH, () => StartGame("quiz")));
            p.Controls.Add(CreateModernButton("ТЕОРИЯ", "📖", Color.FromArgb(150, 100, 210), 40 + 2 * (btnW + gap), startY, btnW, btnH, ShowTheory));

            p.Controls.Add(CreateModernButton("ПРИМЕРЫ", "📁", Color.FromArgb(50, 180, 130), 40, startY + btnH + gap, btnW, btnH, ShowExamples));
            p.Controls.Add(CreateModernButton("ПРОФИЛЬ", "👤", Color.FromArgb(100, 100, 200), 40 + btnW + gap, startY + btnH + gap, btnW, btnH, ShowProfile));
            p.Controls.Add(CreateModernButton("СТАТИСТИКА", "📊", Color.FromArgb(210, 170, 40), 40 + 2 * (btnW + gap), startY + btnH + gap, btnW, btnH, ShowStats));

            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(25, 25, 30)
            };

            var themeBtn = new Button
            {
                Text = "🎨 СМЕНИТЬ ТЕМУ",
                Location = new Point(20, 8),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            themeBtn.Click += (s, e) => ChangeTheme();

            var resetBtn = new Button
            {
                Text = "🔄 СБРОСИТЬ ПРОГРЕСС",
                Location = new Point(160, 8),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(200, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            resetBtn.Click += (s, e) => ResetProgress();

            var exitBtn = new Button
            {
                Text = "❌ ВЫХОД",
                Location = new Point(1200, 8),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(80, 80, 90),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            exitBtn.Click += (s, e) => Application.Exit();

            bottomBar.Controls.Add(themeBtn);
            bottomBar.Controls.Add(resetBtn);
            bottomBar.Controls.Add(exitBtn);
            p.Controls.Add(bottomBar);

            currentPanel = p;
            this.Controls.Add(p);
        }

        private void StartGame(string m) { ClearPanel(); if (m == "practice") CreatePractice(); else CreateQuiz(); }

        // === ПРАКТИКА ===
        private void CreatePractice()
        {
            var game = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 18, 24) };
            var top = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.FromArgb(30, 30, 40) };
            var back = new Button { Text = "← МЕНЮ", Location = new Point(15, 10), Size = new Size(90, 35), BackColor = Color.FromArgb(60, 60, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            back.Click += (s, e) => { StopTimer(); SaveProgress(); ShowMenu(); };
            top.Controls.Add(back);
            timerLabel = new Label { Text = "⏱️ 0 сек", Location = new Point(1280, 12), Size = new Size(100, 30), ForeColor = Color.FromArgb(0, 200, 255), Font = new Font("Segoe UI", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };
            top.Controls.Add(timerLabel);
            game.Controls.Add(top);

            var left = new Panel { Location = new Point(20, 70), Size = new Size(380, 700), BackColor = Color.FromArgb(30, 30, 40) };
            left.Controls.Add(new Label { Text = "📋 ЗАДАНИЕ", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.FromArgb(0, 150, 255), Location = new Point(20, 20), AutoSize = true });
            taskTitle = new Label { Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 55), Size = new Size(340, 50) };
            taskDesc = new Label { Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(180, 180, 200), Location = new Point(20, 115), Size = new Size(340, 120) };
            left.Controls.Add(taskTitle); left.Controls.Add(taskDesc);
            var hint = new Button { Text = "💡 ПОДСКАЗКА", Location = new Point(20, 260), Size = new Size(340, 38), BackColor = Color.FromArgb(100, 80, 180), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            hint.Click += (s, e) => MessageBox.Show(currentTask.Hint, "Подсказка");
            left.Controls.Add(hint);
            game.Controls.Add(left);

            var right = new Panel { Location = new Point(420, 70), Size = new Size(950, 700), BackColor = Color.FromArgb(30, 30, 40) };
            codeBox = new RichTextBox { Location = new Point(20, 20), Size = new Size(910, 280), Font = new Font("Consolas", 11), BackColor = Color.FromArgb(25, 25, 35), ForeColor = Color.White, BorderStyle = BorderStyle.None };
            right.Controls.Add(codeBox);

            var run = new Button { Text = "▶ ЗАПУСТИТЬ", Location = new Point(20, 315), Size = new Size(130, 38), BackColor = Color.FromArgb(50, 180, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            run.Click += RunCode;
            var check = new Button { Text = "✅ ПРОВЕРИТЬ", Location = new Point(160, 315), Size = new Size(130, 38), BackColor = Color.FromArgb(0, 120, 210), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            check.Click += (s, e) => CheckCode();
            var next = new Button { Text = "⏩ ДАЛЕЕ", Location = new Point(300, 315), Size = new Size(130, 38), BackColor = Color.FromArgb(230, 140, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            next.Click += (s, e) => LoadNewTask();
            right.Controls.Add(run); right.Controls.Add(check); right.Controls.Add(next);

            right.Controls.Add(new Label { Text = "🔥 Серия:", Location = new Point(20, 370), AutoSize = true, ForeColor = Color.White });
            streakBar = new ProgressBar { Location = new Point(90, 368), Size = new Size(150, 12), Maximum = 5, Value = Math.Min(user.Streak, 5), BackColor = Color.FromArgb(45, 45, 55), ForeColor = Color.FromArgb(255, 80, 80) };
            right.Controls.Add(streakBar);
            right.Controls.Add(new Label { Text = "📤 ВЫВОД", Location = new Point(20, 405), AutoSize = true, ForeColor = Color.White });
            outputBox = new RichTextBox { Location = new Point(20, 430), Size = new Size(910, 245), Font = new Font("Consolas", 10), BackColor = Color.FromArgb(25, 25, 35), ForeColor = Color.FromArgb(100, 255, 150), ReadOnly = true, BorderStyle = BorderStyle.None, Text = "✨ Готов к работе!" };
            right.Controls.Add(outputBox);

            game.Controls.Add(right);
            currentPanel = game;
            this.Controls.Add(game);
            LoadNewTask();
        }

        private void LoadNewTask() { StopTimer(); currentTask = tasks[rand.Next(tasks.Count)]; taskTitle.Text = currentTask.Title; taskDesc.Text = currentTask.Desc; codeBox.Text = "// Ваш код"; outputBox.Text = "✨ Готов!"; StartTimer(); }

        private async void RunCode(object sender, EventArgs e) { if (string.IsNullOrWhiteSpace(codeBox.Text)) { outputBox.Text = "⚠️ Напишите код!"; return; } outputBox.Text = "🔨 Выполнение..."; try { outputBox.Text = await Execute(codeBox.Text); outputBox.ForeColor = Color.FromArgb(100, 255, 150); } catch (Exception ex) { outputBox.Text = $"❌ {ex.Message}"; outputBox.ForeColor = Color.FromArgb(255, 100, 100); } }

        private async void CheckCode()
        {
            if (!timerRun) StartTimer();
            string res = await Execute(codeBox.Text);

            if (res.Trim() == currentTask.Expected)
            {
                StopTimer(); int gain = 50 + currentTask.Diff * 10;
                user.Exp += gain; user.Streak++; user.Solved++;
                user.CorrectAnswers++;
                user.TotalTimeSpent += seconds;

                CheckAchievements();

                outputBox.Text = $"✅ ПРАВИЛЬНО! +{gain} опыта\n⏱️ {seconds} сек\n\n{res}";
                System.Media.SystemSounds.Asterisk.Play();
                UpdateStats(); streakBar.Value = Math.Min(user.Streak, 5);

                if (user.Exp >= user.Level * EXP_PER_LVL && user.Level < MAX_LVL)
                {
                    user.Level++;
                    System.Media.SystemSounds.Exclamation.Play();
                    MessageBox.Show($"🎉 ПОЗДРАВЛЯЮ! УРОВЕНЬ {user.Level}!\n\n{GetRewardForLevel(user.Level)}", "🎉 ПОВЫШЕНИЕ УРОВНЯ!");
                    UpdateStats();
                }
                SaveProgress(); LoadNewTask();
            }
            else
            {
                user.Streak = 0;
                user.WrongAnswers++;
                streakBar.Value = 0;
                outputBox.Text = $"❌ НЕПРАВИЛЬНО\n\nВывод: {res}\nОжидалось: {currentTask.Expected}\n\n💡 {currentTask.Hint}";
                outputBox.ForeColor = Color.FromArgb(255, 100, 100);
                System.Media.SystemSounds.Hand.Play(); UpdateStats();
            }
            UpdateProgress();
        }

        // === КВИЗ ===
        private void CreateQuiz()
        {
            var game = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 18, 24) };
            var top = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.FromArgb(30, 30, 40) };
            var back = new Button { Text = "← МЕНЮ", Location = new Point(15, 10), Size = new Size(90, 35), BackColor = Color.FromArgb(60, 60, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            back.Click += (s, e) => { SaveProgress(); ShowMenu(); };
            top.Controls.Add(back);
            game.Controls.Add(top);

            var qp = new Panel { Location = new Point(200, 80), Size = new Size(1000, 650), BackColor = Color.FromArgb(30, 30, 40) };
            qp.Controls.Add(new Label { Text = "❓ ВОПРОС", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(0, 150, 255), Location = new Point(30, 25), AutoSize = true });
            quizQuestion = new Label { Font = new Font("Segoe UI", 13), ForeColor = Color.White, Location = new Point(30, 65), Size = new Size(940, 90) };
            qp.Controls.Add(quizQuestion);
            for (int i = 0; i < 4; i++) { quizOpts[i] = new RadioButton { Font = new Font("Segoe UI", 12), ForeColor = Color.White, Location = new Point(50, 175 + i * 55), Size = new Size(920, 35), AutoSize = true }; qp.Controls.Add(quizOpts[i]); }
            var sub = new Button { Text = "✅ ОТВЕТИТЬ", Location = new Point(50, 420), Size = new Size(200, 45), BackColor = Color.FromArgb(0, 120, 210), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            sub.Click += (s, e) => SubmitAnswer();
            qp.Controls.Add(sub);
            quizFeedback = new Label { Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(255, 200, 50), Location = new Point(50, 490), Size = new Size(920, 130), AutoSize = false };
            qp.Controls.Add(quizFeedback);
            game.Controls.Add(qp);
            currentPanel = game;
            this.Controls.Add(game);
            LoadNewQuiz();
        }

        private void LoadNewQuiz() { currentQuiz = quizzes[rand.Next(quizzes.Count)]; quizQuestion.Text = currentQuiz.Question; for (int i = 0; i < currentQuiz.Options.Count; i++) quizOpts[i].Text = currentQuiz.Options[i]; for (int i = currentQuiz.Options.Count; i < 4; i++) quizOpts[i].Text = ""; foreach (var o in quizOpts) o.Checked = false; quizFeedback.Text = ""; }

        private async void SubmitAnswer()
        {
            int sel = -1; for (int i = 0; i < 4; i++) if (quizOpts[i].Checked) { sel = i; break; }
            if (sel == -1) { quizFeedback.Text = "⚠️ Выберите ответ!"; return; }
            if (sel == currentQuiz.Correct)
            {
                int gain = 30; user.Exp += gain; user.Streak++; user.Solved++; user.CorrectAnswers++;
                quizFeedback.Text = $"✅ ПРАВИЛЬНО! +{gain} опыта\n\n{currentQuiz.Explanation}";
                quizFeedback.ForeColor = Color.FromArgb(100, 255, 100);
                System.Media.SystemSounds.Asterisk.Play();
                CheckAchievements();
                UpdateStats(); SaveProgress(); await Task.Delay(1500); LoadNewQuiz();
            }
            else
            {
                user.Streak = 0; user.WrongAnswers++;
                quizFeedback.Text = $"❌ НЕПРАВИЛЬНО!\n\nОтвет: {currentQuiz.Options[currentQuiz.Correct]}\n\n{currentQuiz.Explanation}";
                quizFeedback.ForeColor = Color.FromArgb(255, 100, 100);
                System.Media.SystemSounds.Hand.Play(); UpdateStats();
            }
            UpdateProgress();
        }

        // === ТЕОРИЯ ===
        private void ShowTheory()
        {
            ClearPanel();
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 18, 24), Padding = new Padding(40) };
            p.Controls.Add(new Label { Text = "📚 ТЕОРИЯ C#", Font = new Font("Segoe UI", 32, FontStyle.Bold), ForeColor = Color.FromArgb(0, 150, 255), Location = new Point(40, 20), AutoSize = true });
            var back = new Button { Text = "← МЕНЮ", Size = new Size(100, 38), Location = new Point(40, 80), BackColor = Color.FromArgb(60, 60, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; back.Click += (s, e) => ShowMenu(); p.Controls.Add(back);

            var list = new Panel { Location = new Point(40, 130), Size = new Size(250, 620), BackColor = Color.FromArgb(30, 30, 40), AutoScroll = true };
            string[] topics = { "Введение", "Типы данных", "Операторы", "Условия", "Циклы", "Массивы", "Строки", "Методы", "Классы", "Наследование", "Полиморфизм", "Интерфейсы", "Коллекции", "Ошибки", "LINQ", "Асинхронность" };
            for (int i = 0; i < topics.Length; i++) { int idx = i + 1; var btn = new Button { Text = topics[i], Size = new Size(220, 38), Location = new Point(15, 10 + i * 44), BackColor = Color.FromArgb(45, 45, 55), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0), Cursor = Cursors.Hand }; btn.Click += (s, e) => ShowTheoryContent(idx); list.Controls.Add(btn); }
            p.Controls.Add(list);

            var content = new Panel { Location = new Point(310, 130), Size = new Size(1020, 620), BackColor = Color.FromArgb(30, 30, 40) };
            theoryBox = new RichTextBox { Location = new Point(20, 20), Size = new Size(980, 580), Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(25, 25, 35), ForeColor = Color.White, ReadOnly = true, BorderStyle = BorderStyle.None };
            content.Controls.Add(theoryBox);
            p.Controls.Add(content);
            currentPanel = p;
            this.Controls.Add(p);
            ShowTheoryContent(1);
        }

        private void ShowTheoryContent(int t)
        {
            string[] c = new string[17];
            c[1] = "📖 ВВЕДЕНИЕ В C#\n\nC# - язык Microsoft (2000)\n• Строгая типизация\n• ООП\n• Автосборка мусора\n\nПример:\nConsole.WriteLine(\"Hello!\");";
            c[2] = "📖 ТИПЫ ДАННЫХ\n\nint (целые)\ndouble (дробные)\nbool (true/false)\nstring (строки)\n\nint age = 25;\ndouble price = 19.99;";
            c[3] = "📖 ОПЕРАТОРЫ\n\n+ - * / %\n== != > < >= <=\n&& || !\n\nint sum = 10 + 5;\nbool isEqual = (5 == 5);";
            c[4] = "📖 УСЛОВИЯ\n\nif (условие) { }\nelse if { }\nelse { }\n\nint age = 18;\nif (age >= 18)\n    Console.WriteLine(\"Взрослый\");\nelse\n    Console.WriteLine(\"Ребёнок\");";
            c[5] = "📖 ЦИКЛЫ\n\nfor (int i=0; i<5; i++)\nwhile (условие)\ndo-while\nforeach\n\nfor (int i=1; i<=5; i++)\n    Console.Write(i + \" \");";
            c[6] = "📖 МАССИВЫ\n\nint[] arr = new int[5];\nint[] nums = {1,2,3,4,5};\narr[0] = 10;\nint len = arr.Length;\n\nforeach (int n in nums)\n    Console.WriteLine(n);";
            c[7] = "📖 СТРОКИ\n\nstring s = \"Hello\";\n.Length, .ToUpper(), .ToLower()\n.Contains(), .Replace(), .Split()\n\nstring upper = s.ToUpper();\nbool has = s.Contains(\"ell\");";
            c[8] = "📖 МЕТОДЫ\n\nint Add(int a, int b) { return a + b; }\nvoid SayHello() { Console.WriteLine(\"Hi!\"); }\n\nint res = Add(5, 3);\nSayHello();";
            c[9] = "📖 КЛАССЫ\n\nclass Person {\n    public string Name { get; set; }\n    public int Age { get; set; }\n}\n\nPerson p = new Person();\np.Name = \"Анна\";\np.Age = 25;";
            c[10] = "📖 НАСЛЕДОВАНИЕ\n\nclass Animal { }\nclass Dog : Animal { }\n\n// базовый класс\nclass Animal { public void Eat() { } }\n// производный\nclass Dog : Animal { public void Bark() { } }";
            c[11] = "📖 ПОЛИМОРФИЗМ\n\nvirtual - виртуальный метод\noverride - переопределение\nabstract - абстрактный\n\npublic virtual void MakeSound() { }\npublic override void MakeSound() { }";
            c[12] = "📖 ИНТЕРФЕЙСЫ\n\ninterface IAnimal { void MakeSound(); }\nclass Dog : IAnimal { public void MakeSound() { } }\n\n// множественное наследование\nclass Bird : IAnimal, IFlyable { }";
            c[13] = "📖 КОЛЛЕКЦИИ\n\nList<T> - динамический массив\nDictionary<K,V> - ключ-значение\nStack<T> - LIFO\nQueue<T> - FIFO\nHashSet<T> - уникальные";
            c[14] = "📖 ОБРАБОТКА ОШИБОК\n\ntry {\n    // опасный код\n}\ncatch (Exception ex) {\n    // обработка\n}\nfinally {\n    // всегда\n}";
            c[15] = "📖 LINQ\n\nLanguage Integrated Query\n\nМетоды:\nWhere() - фильтр\nSelect() - проекция\nOrderBy() - сортировка\n\nvar result = numbers.Where(n => n > 5);";
            c[16] = "📖 АСИНХРОННОСТЬ\n\nasync Task<int> GetDataAsync() {\n    await Task.Delay(1000);\n    return 42;\n}\n\nint result = await GetDataAsync();\n// Не блокирует поток";
            theoryBox.Text = c[t];
            theoryBox.SelectionStart = 0;
        }

        // === ПРИМЕРЫ ===
        private void ShowExamples()
        {
            ClearPanel();
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(18, 18, 24), Padding = new Padding(40) };
            p.Controls.Add(new Label { Text = "📁 ПРИМЕРЫ ПРОГРАММ", Font = new Font("Segoe UI", 32, FontStyle.Bold), ForeColor = Color.FromArgb(50, 180, 130), Location = new Point(40, 20), AutoSize = true });
            var back = new Button { Text = "← МЕНЮ", Size = new Size(100, 38), Location = new Point(40, 80), BackColor = Color.FromArgb(60, 60, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; back.Click += (s, e) => ShowMenu(); p.Controls.Add(back);

            var list = new Panel { Location = new Point(40, 130), Size = new Size(260, 620), BackColor = Color.FromArgb(30, 30, 40), AutoScroll = true };
            string[] exs = { "Калькулятор", "Угадай число", "Генератор паролей", "Палиндром", "Конвертер температур", "Анализатор текста", "ToDo список", "Секундомер", "BMI", "Шифр Цезаря", "Таблица умножения", "Факториал", "Фибоначчи", "Поиск максимума", "Сортировка пузырьком", "Калькулятор кредита", "Конвертер валют", "Таймер", "Калькулятор возраста", "Площадь фигур" };
            for (int i = 0; i < exs.Length; i++) { string e = exs[i]; var btn = new Button { Text = e, Size = new Size(230, 36), Location = new Point(15, 10 + i * 42), BackColor = Color.FromArgb(45, 45, 55), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0), Cursor = Cursors.Hand }; btn.Click += (s, ev) => ShowExampleCode(e); list.Controls.Add(btn); }
            p.Controls.Add(list);

            var codePanel = new Panel { Location = new Point(320, 130), Size = new Size(1020, 620), BackColor = Color.FromArgb(30, 30, 40) };
            exTitle = new Label { Text = "📄 Выберите пример", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.FromArgb(50, 180, 130), Location = new Point(20, 15), AutoSize = true };
            codePanel.Controls.Add(exTitle);
            exampleBox = new RichTextBox { Location = new Point(20, 50), Size = new Size(980, 350), Font = new Font("Consolas", 10), BackColor = Color.FromArgb(25, 25, 35), ForeColor = Color.FromArgb(200, 220, 200), ReadOnly = true, BorderStyle = BorderStyle.None };
            codePanel.Controls.Add(exampleBox);
            var run = new Button { Text = "▶ ЗАПУСТИТЬ", Size = new Size(980, 40), Location = new Point(20, 415), BackColor = Color.FromArgb(50, 180, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            run.Click += (s, e) => RunExample();
            codePanel.Controls.Add(run);
            exampleOut = new RichTextBox { Location = new Point(20, 470), Size = new Size(980, 125), Font = new Font("Consolas", 10), BackColor = Color.FromArgb(25, 25, 35), ForeColor = Color.FromArgb(100, 255, 150), ReadOnly = true, BorderStyle = BorderStyle.None, Text = "📌 Выберите пример" };
            codePanel.Controls.Add(exampleOut);
            p.Controls.Add(codePanel);
            currentPanel = p;
            this.Controls.Add(p);
        }

        private void ShowExampleCode(string name)
        {
            var codes = new Dictionary<string, string>
            {
                ["Калькулятор"] = "double a=15,b=7;\nConsole.WriteLine($\"{a}+{b}={a+b}\");\nConsole.WriteLine($\"{a}-{b}={a-b}\");\nConsole.WriteLine($\"{a}*{b}={a*b}\");\nConsole.WriteLine($\"{a}/{b}={a/b}\");",
                ["Угадай число"] = "Random r=new Random();int s=r.Next(1,21);\nConsole.Write(\"Ваш вариант: \");int g=int.Parse(Console.ReadLine());\nConsole.WriteLine(g==s?\"Угадали!\":$\"Не угадали! Было {s}\");",
                ["Генератор паролей"] = "Random r=new Random();string c=\"ABCabc123!@#\";string p=\"\";\nfor(int i=0;i<10;i++)p+=c[r.Next(c.Length)];\nConsole.WriteLine($\"Пароль: {p}\");",
                ["Палиндром"] = "string w=\"radar\";char[] a=w.ToCharArray();Array.Reverse(a);\nstring r=new string(a);\nConsole.WriteLine($\"{w} - {(w==r?\"палиндром\":\"не палиндром\")}\");",
                ["Конвертер температур"] = "double c=25;double f=c*9/5+32;\nConsole.WriteLine($\"{c}C = {f:F1}F\");\n\ndouble f2=77;double c2=(f2-32)*5/9;\nConsole.WriteLine($\"{f2}F = {c2:F1}C\");",
                ["Анализатор текста"] = "string t=\"Hello123!\";int l=0,d=0,s=0;\nforeach(char ch in t)\n{if(ch>='A'&&ch<='z')l++;else if(ch>='0'&&ch<='9')d++;else s++;}\nConsole.WriteLine($\"Букв:{l} Цифр:{d} Символов:{s}\");",
                ["ToDo список"] = "var t=new System.Collections.Generic.List<string>();\nt.Add(\"Выучить C#\");\nt.Add(\"Сделать проект\");\nt.Add(\"Практика\");\nfor(int i=0;i<t.Count;i++)\nConsole.WriteLine($\"{i+1}.{t[i]}\");",
                ["Секундомер"] = "var sw=System.Diagnostics.Stopwatch.StartNew();\nConsole.WriteLine(\"Нажмите Enter\");Console.ReadLine();\nsw.Stop();\nConsole.WriteLine($\"Прошло: {sw.Elapsed.TotalSeconds:F1} сек\");",
                ["BMI"] = "double w=75,h=1.75,bmi=w/(h*h);\nConsole.WriteLine($\"BMI:{bmi:F1}\");\nif(bmi<18.5)Console.WriteLine(\"Недостаток\");\nelse if(bmi<25)Console.WriteLine(\"Норма\");\nelse if(bmi<30)Console.WriteLine(\"Избыток\");\nelse Console.WriteLine(\"Ожирение\");"
            };
            exTitle.Text = $"📄 {name}";
            exampleBox.Text = codes.ContainsKey(name) ? codes[name] : "Console.WriteLine(\"Hello!\");";
            exampleOut.Text = "📌 Нажмите 'Запустить'";
        }

        private async void RunExample()
        {
            if (string.IsNullOrWhiteSpace(exampleBox.Text)) { exampleOut.Text = "⚠️ Выберите пример!"; return; }
            exampleOut.Text = "🔨 Выполнение...";
            try { exampleOut.Text = await Execute(exampleBox.Text); exampleOut.ForeColor = Color.FromArgb(100, 255, 150); }
            catch (Exception ex) { exampleOut.Text = $"❌ {ex.Message}"; exampleOut.ForeColor = Color.FromArgb(255, 100, 100); }
        }

        // === ВЫПОЛНЕНИЕ КОДА ===
        private async Task<string> Execute(string c)
        {
            string full = $@"
using System;
using System.Text;
using System.IO;

class Program
{{
    public static string Run()
    {{
        var output = new StringBuilder();
        var writer = new StringWriter(output);
        Console.SetOut(writer);
        try
        {{
            {c}
        }}
        catch (Exception ex)
        {{
            return ""Ошибка: "" + ex.Message;
        }}
        writer.Flush();
        return output.ToString();
    }}
}}

Program.Run()";
            var options = ScriptOptions.Default.WithReferences(typeof(object).Assembly, typeof(Console).Assembly).WithImports("System");
            try { return await CSharpScript.EvaluateAsync<string>(full, options); }
            catch (CompilationErrorException ex) { return $"Ошибка: {ex.Message}"; }
        }

        // === ВСПОМОГАТЕЛЬНЫЕ ===
        private void StartTimer() { if (gameTimer != null) { gameTimer.Stop(); gameTimer.Dispose(); } seconds = 0; if (timerLabel != null) timerLabel.Text = "⏱️ 0 сек"; gameTimer = new Timer { Interval = 1000 }; gameTimer.Tick += (s, e) => { seconds++; if (timerLabel != null && !timerLabel.IsDisposed) timerLabel.Text = $"⏱️ {seconds} сек"; }; gameTimer.Start(); timerRun = true; }
        private void StopTimer() { if (gameTimer != null) { gameTimer.Stop(); gameTimer.Dispose(); gameTimer = null; } timerRun = false; }
        private void UpdateStats() { if (lblLevel != null) lblLevel.Text = $"🏆 {GetRank()} [{user.Level}]"; if (lblExp != null) lblExp.Text = $"⭐ Опыт: {user.Exp}/{user.Level * EXP_PER_LVL}"; if (lblStreak != null) lblStreak.Text = $"🔥 Серия: {user.Streak}"; if (lblSolved != null) lblSolved.Text = $"✅ Решено: {user.Solved}"; if (tipLabel != null) tipLabel.Text = GetTip(); }
        private void UpdateProgress() { if (levelBar != null) levelBar.Value = GetProgress(); }
        private int GetProgress() => (user.Exp % (user.Level * EXP_PER_LVL)) * 100 / (user.Level * EXP_PER_LVL);
        private string GetRank() => user.Level >= 20 ? "👑 Легенда" : user.Level >= 18 ? "🚀 Грандмастер" : user.Level >= 15 ? "⚡ Мастер" : user.Level >= 12 ? "💪 Профи" : user.Level >= 10 ? "📚 Продвинутый" : user.Level >= 7 ? "🌱 Ученик" : user.Level >= 4 ? "⭐ Новичок" : "🪄 Начинающий";
        private string GetTip() => new[] { "💡 Используйте отступы", "💡 Имена переменных понятными", "💡 Избегайте магических чисел", "💡 Используйте var", "💡 Проверяйте граничные случаи", "💡 Комментируйте сложный код", "💡 Практикуйтесь каждый день" }[rand.Next(7)];
        private void ClearPanel() { if (currentPanel != null) { this.Controls.Remove(currentPanel); currentPanel.Dispose(); } }
        private void ShowStats() => MessageBox.Show($"📊 СТАТИСТИКА\n\n🏆 {GetRank()}\n⭐ Опыт: {user.Exp}\n🔥 Серия: {user.Streak}\n✅ Решено: {user.Solved}\n📈 Точность: {(user.CorrectAnswers + user.WrongAnswers > 0 ? (user.CorrectAnswers * 100.0 / (user.CorrectAnswers + user.WrongAnswers)).ToString("F1") : "0")}%\n⏱️ Время в приложении: {user.TotalTimeSpent} сек", "Статистика", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void ResetProgress() { if (MessageBox.Show("Сбросить прогресс?", "Сброс", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { user = new UserData(); SaveProgress(); UpdateStats(); UpdateProgress(); ShowMenu(); } }
        private void LoadProgress() { if (File.Exists(saveFile)) try { user = JsonSerializer.Deserialize<UserData>(File.ReadAllText(saveFile)) ?? new UserData(); if (user.Achievements == null) user.Achievements = new List<string>(); } catch { } UpdateStats(); CheckAchievements(); }
        private void SaveProgress() => File.WriteAllText(saveFile, JsonSerializer.Serialize(user));
        protected override void OnFormClosed(FormClosedEventArgs e) { StopTimer(); SaveProgress(); base.OnFormClosed(e); }
    }

    // ===== КЛАССЫ =====
    public class TaskItem
    {
        public int Diff { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public string Expected { get; set; }
        public string Hint { get; set; }
    }

    public class QuizItem
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int Correct { get; set; }
        public string Explanation { get; set; }
    }

    public class UserData
    {
        public int Level { get; set; } = 1;
        public int Exp { get; set; } = 0;
        public int Streak { get; set; } = 0;
        public int Solved { get; set; } = 0;
        public List<string> Achievements { get; set; } = new List<string>();
        public DateTime LastPlayed { get; set; } = DateTime.Now;
        public int TotalTimeSpent { get; set; } = 0;
        public int CorrectAnswers { get; set; } = 0;
        public int WrongAnswers { get; set; } = 0;
    }
}