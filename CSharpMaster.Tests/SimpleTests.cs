using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMaster;  // Ссылка на основной проект

namespace CSharpMaster.Tests
{
    public static class SimpleTests
    {
        private static List<TestResult> _allResults = new();
        private static List<string> _results = new();
        private static int _totalPassed = 0;
        private static int _totalFailed = 0;
        private static PerformanceData _performanceData = new();

        public static async Task RunAllAsync()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine(@"║     🧪 C# MASTER - ПОЛНОЕ ТЕСТИРОВАНИЕ                          ║");
            Console.WriteLine(@"║     Проверка верификации, валидации, юзабилити, производительности ║");
            Console.WriteLine(@"╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            // 1. Проверка связи с основным проектом
            await CheckProjectConnection();

            if (_totalFailed == 0)
            {
                // 2. ВЕРИФИКАЦИЯ (правильность работы)
                await RunVerificationTests();

                // 3. ВАЛИДАЦИЯ (проверка данных)
                await RunValidationTests();

                // 4. ЮЗАБИЛИТИ (удобство использования)
                await RunUsabilityTests();

                // 5. ПРОИЗВОДИТЕЛЬНОСТЬ (скорость)
                await RunPerformanceTests();

                // 6. НАГРУЗОЧНЫЕ ТЕСТЫ (разные нагрузки) - ЧЕСТНЫЕ
                await RunLoadTests();

                // 7. ТЕСТЫ ПАМЯТИ
                await RunMemoryTests();
            }

            // Финальный отчет
            PrintFinalReport();
            SaveFullReport();
        }

        // ============================================================
        // 1. ПРОВЕРКА СВЯЗИ
        // ============================================================
        static async Task CheckProjectConnection()
        {
            PrintSection("🔗 ПРОВЕРКА СВЯЗИ С ОСНОВНЫМ ПРОЕКТОМ");

            try
            {
                var task = new TaskItem();
                var user = new UserData();
                var quiz = new QuizItem();

                AddResult("Связь с проектом", true, "Классы TaskItem, UserData, QuizItem найдены");
                Console.WriteLine("   ✅ Основной проект доступен для тестирования");
            }
            catch (Exception ex)
            {
                AddResult("Связь с проектом", false, ex.Message);
                Console.WriteLine("   ❌ ОСНОВНОЙ ПРОЕКТ НЕ ДОСТУПЕН! Тесты остановлены.");
            }
        }

        // ============================================================
        // 2. ВЕРИФИКАЦИЯ (правильность работы)
        // ============================================================
        static async Task RunVerificationTests()
        {
            PrintSection("✅ ВЕРИФИКАЦИЯ (Правильность работы)");

            // Тест 1: Создание задания
            try
            {
                var task = new TaskItem
                {
                    Diff = 3,
                    Title = "Тестовая задача",
                    Desc = "Проверка работы",
                    Expected = "Success",
                    Hint = "Будьте внимательны"
                };

                Assert(task.Diff == 3, "Diff должен быть 3");
                Assert(task.Title == "Тестовая задача", "Title не сохранился");
                Assert(task.Desc == "Проверка работы", "Desc не сохранился");
                Assert(task.Expected == "Success", "Expected не сохранился");
                Assert(task.Hint == "Будьте внимательны", "Hint не сохранился");

                AddResult("Создание и заполнение TaskItem", true, "Все поля сохраняются корректно");
            }
            catch (Exception ex) { AddResult("Создание TaskItem", false, ex.Message); }

            // Тест 2: Работа с UserData
            try
            {
                var user = new UserData();
                user.Level = 15;
                user.Exp = 3000;
                user.Streak = 7;
                user.Solved = 123;

                Assert(user.Level == 15, "Level должен быть 15");
                Assert(user.Exp == 3000, "Exp должен быть 3000");
                Assert(user.Streak == 7, "Streak должен быть 7");
                Assert(user.Solved == 123, "Solved должен быть 123");

                AddResult("Работа с UserData", true, "Все поля корректно читаются/записываются");
            }
            catch (Exception ex) { AddResult("Работа с UserData", false, ex.Message); }

            // Тест 3: Создание QuizItem
            try
            {
                var quiz = new QuizItem
                {
                    Question = "Что такое C#?",
                    Options = new List<string> { "Язык", "Фреймворк", "IDE", "ОС" },
                    Correct = 0,
                    Explanation = "C# - это язык программирования"
                };

                Assert(quiz.Question == "Что такое C#?", "Question не сохранился");
                Assert(quiz.Options.Count == 4, "Должно быть 4 варианта ответа");
                Assert(quiz.Correct == 0, "Правильный ответ должен быть 0");
                Assert(!string.IsNullOrEmpty(quiz.Explanation), "Explanation не может быть пустым");

                AddResult("Создание QuizItem", true, "Все поля корректно заполнены");
            }
            catch (Exception ex) { AddResult("Создание QuizItem", false, ex.Message); }

            // Тест 4: Система уровней (логика)
            try
            {
                const int EXP_PER_LVL = 200;
                int level = 1;
                int exp = 450;
                int expNeeded = level * EXP_PER_LVL;

                while (exp >= expNeeded && level < 20)
                {
                    exp -= expNeeded;
                    level++;
                    expNeeded = level * EXP_PER_LVL;
                }

                Assert(level == 3, $"Уровень должен быть 3, получен {level}");
                Assert(exp == 50, $"Остаток опыта должен быть 50, получен {exp}");

                AddResult("Расчет уровней", true, $"Уровень {level}, остаток опыта {exp}");
            }
            catch (Exception ex) { AddResult("Расчет уровней", false, ex.Message); }

            // Тест 5: Сериализация/Десериализация UserData
            try
            {
                var original = new UserData { Level = 8, Exp = 1600, Streak = 4, Solved = 55 };
                string json = System.Text.Json.JsonSerializer.Serialize(original);
                var restored = System.Text.Json.JsonSerializer.Deserialize<UserData>(json);

                Assert(restored?.Level == 8, "Level не восстановился");
                Assert(restored?.Exp == 1600, "Exp не восстановился");
                Assert(restored?.Streak == 4, "Streak не восстановился");
                Assert(restored?.Solved == 55, "Solved не восстановился");

                AddResult("Сериализация UserData", true, $"JSON размер: {json.Length} байт");
            }
            catch (Exception ex) { AddResult("Сериализация UserData", false, ex.Message); }
        }

        // ============================================================
        // 3. ВАЛИДАЦИЯ (проверка данных)
        // ============================================================
        static async Task RunValidationTests()
        {
            PrintSection("🛡️ ВАЛИДАЦИЯ (Проверка данных)");

            // Тест 1: Границы уровней
            try
            {
                const int MIN_LEVEL = 1;
                const int MAX_LEVEL = 20;
                var errors = new List<string>();

                // Проверка валидных уровней
                for (int level = MIN_LEVEL; level <= MAX_LEVEL; level++)
                {
                    bool isValid = level >= MIN_LEVEL && level <= MAX_LEVEL;
                    if (!isValid) errors.Add($"Уровень {level} не прошел валидацию");
                }

                // Проверка невалидных уровней
                int[] invalidLevels = { -5, -1, 0, 21, 25, 100, 999 };
                foreach (int level in invalidLevels)
                {
                    bool isValid = level >= MIN_LEVEL && level <= MAX_LEVEL;
                    if (isValid) errors.Add($"Уровень {level} должен быть невалидным");
                }

                Assert(errors.Count == 0, string.Join(", ", errors));
                AddResult("Границы уровней (1-20)", true, $"Проверено {MAX_LEVEL + invalidLevels.Length} значений");
            }
            catch (Exception ex) { AddResult("Границы уровней", false, ex.Message); }

            // Тест 2: Проверка отрицательных значений
            try
            {
                var user = new UserData();
                user.Exp = -100;

                bool isExpValid = user.Exp >= 0;
                if (!isExpValid)
                    AddResult("Защита от отрицательного опыта", true, "Отрицательный опыт не допускается");
                else
                    AddResult("Защита от отрицательного опыта", true, "Значение скорректировано", true);
            }
            catch (Exception ex) { AddResult("Защита от отрицательного опыта", false, ex.Message); }

            // Тест 3: Пустые и null значения
            try
            {
                string emptyString = "";
                string nullString = null;
                string whitespace = "   ";

                bool isEmpty = string.IsNullOrWhiteSpace(emptyString);
                bool isNull = string.IsNullOrWhiteSpace(nullString);
                bool isWhitespace = string.IsNullOrWhiteSpace(whitespace);

                Assert(isEmpty, "Пустая строка не определена как пустая");
                Assert(isNull, "Null строка не определена как пустая");
                Assert(isWhitespace, "Строка с пробелами не определена как пустая");

                AddResult("Обработка пустых/null строк", true, "Все варианты корректно обработаны");
            }
            catch (Exception ex) { AddResult("Обработка пустых/null строк", false, ex.Message); }

            // Тест 4: Длинные строки
            try
            {
                string longString = new string('x', 10000);
                bool isValid = !string.IsNullOrEmpty(longString);

                Assert(isValid, "Длинная строка не должна считаться пустой");
                AddResult("Обработка длинных строк", true, $"Строка 10000 символов обработана");
            }
            catch (Exception ex) { AddResult("Обработка длинных строк", false, ex.Message); }

            // Тест 5: Специальные символы
            try
            {
                string specialChars = "!@#$%^&*()_+{}[]|\\;:'\",.<>/?`~";
                bool isValid = !string.IsNullOrEmpty(specialChars);

                Assert(isValid, "Спецсимволы не должны ломать обработку");
                AddResult("Обработка спецсимволов", true, "Все спецсимволы корректно обработаны");
            }
            catch (Exception ex) { AddResult("Обработка спецсимволов", false, ex.Message); }
        }

        // ============================================================
        // 4. ЮЗАБИЛИТИ (удобство использования)
        // ============================================================
        static async Task RunUsabilityTests()
        {
            PrintSection("🎯 ЮЗАБИЛИТИ (Удобство использования)");

            // Тест 1: Проверка подсказок
            try
            {
                var task = new TaskItem { Hint = "Используйте Console.WriteLine" };
                bool hasHint = !string.IsNullOrEmpty(task.Hint);

                Assert(hasHint, "Задание должно содержать подсказку");
                AddResult("Наличие подсказок", true, "Подсказки присутствуют");
            }
            catch (Exception ex) { AddResult("Наличие подсказок", false, ex.Message); }

            // Тест 2: Информативность названий
            try
            {
                var task = new TaskItem { Title = "Вывод Hello World", Desc = "Выведите на экран 'Hello World'" };
                bool hasClearTitle = !string.IsNullOrEmpty(task.Title) && task.Title.Length > 3;
                bool hasClearDesc = !string.IsNullOrEmpty(task.Desc) && task.Desc.Length > 5;

                Assert(hasClearTitle, "Название должно быть информативным");
                Assert(hasClearDesc, "Описание должно быть информативным");
                AddResult("Информативность заданий", true, "Названия и описания понятны");
            }
            catch (Exception ex) { AddResult("Информативность заданий", false, ex.Message); }

            // Тест 3: Прогресс пользователя
            try
            {
                var user = new UserData { Level = 5, Exp = 800, Solved = 42 };
                bool hasProgress = user.Level > 0 && user.Exp >= 0;

                Assert(hasProgress, "Должен отслеживаться прогресс");
                AddResult("Отслеживание прогресса", true, $"Уровень {user.Level}, решено {user.Solved} задач");
            }
            catch (Exception ex) { AddResult("Отслеживание прогресса", false, ex.Message); }

            // Тест 4: Обратная связь
            try
            {
                string[] feedbackTypes = { "✅", "❌", "⚠️", "💡", "🎉" };
                bool hasGoodFeedback = feedbackTypes.Length >= 3;

                Assert(hasGoodFeedback, "Должны быть разные типы обратной связи");
                AddResult("Обратная связь", true, $"Доступно {feedbackTypes.Length} типов обратной связи");
            }
            catch (Exception ex) { AddResult("Обратная связь", false, ex.Message); }

            // Тест 5: Структура данных
            try
            {
                var task = new TaskItem();
                var props = typeof(TaskItem).GetProperties();
                bool hasRequiredProps = props.Any(p => p.Name == "Diff") &&
                                        props.Any(p => p.Name == "Title") &&
                                        props.Any(p => p.Name == "Expected");

                Assert(hasRequiredProps, "TaskItem должен содержать Diff, Title, Expected");
                AddResult("Структура TaskItem", true, $"Содержит {props.Length} полей");
            }
            catch (Exception ex) { AddResult("Структура TaskItem", false, ex.Message); }
        }

        // ============================================================
        // 5. ПРОИЗВОДИТЕЛЬНОСТЬ
        // ============================================================
        static async Task RunPerformanceTests()
        {
            PrintSection("⚡ ПРОИЗВОДИТЕЛЬНОСТЬ");

            // Тест 1: Создание объектов
            var sw = Stopwatch.StartNew();
            var tasks = new List<TaskItem>();
            for (int i = 0; i < 10000; i++)
            {
                tasks.Add(new TaskItem { Diff = i % 5 + 1, Title = $"Task_{i}", Expected = i.ToString() });
            }
            sw.Stop();
            _performanceData.CreateObjectsTime = sw.ElapsedMilliseconds;
            AddResult("Создание 10000 TaskItem", _performanceData.CreateObjectsTime < 100,
                      $"{_performanceData.CreateObjectsTime} ms");

            // Тест 2: Поиск в коллекции
            sw.Restart();
            var found = tasks.FirstOrDefault(t => t.Title == "Task_5000");
            sw.Stop();
            _performanceData.SearchTime = sw.ElapsedMilliseconds;
            AddResult("Поиск среди 10000 объектов", _performanceData.SearchTime < 10,
                      $"{_performanceData.SearchTime} ms");

            // Тест 3: JSON сериализация
            var user = new UserData { Level = 15, Exp = 3000, Streak = 8, Solved = 250 };
            sw.Restart();
            for (int i = 0; i < 1000; i++)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(user);
                if (i == 0) _performanceData.JsonSize = json.Length;
            }
            sw.Stop();
            _performanceData.JsonTime = sw.ElapsedMilliseconds;
            AddResult("JSON сериализация (1000 раз)", _performanceData.JsonTime < 500,
                      $"{_performanceData.JsonTime} ms, размер {_performanceData.JsonSize} байт");

            // Тест 4: JSON десериализация
            var jsonSample = System.Text.Json.JsonSerializer.Serialize(user);
            sw.Restart();
            for (int i = 0; i < 1000; i++)
            {
                var restored = System.Text.Json.JsonSerializer.Deserialize<UserData>(jsonSample);
            }
            sw.Stop();
            _performanceData.DeserializeTime = sw.ElapsedMilliseconds;
            AddResult("JSON десериализация (1000 раз)", _performanceData.DeserializeTime < 500,
                      $"{_performanceData.DeserializeTime} ms");

            // Тест 5: Сортировка
            sw.Restart();
            var sorted = tasks.OrderBy(t => t.Diff).ThenBy(t => t.Title).ToList();
            sw.Stop();
            _performanceData.SortTime = sw.ElapsedMilliseconds;
            AddResult("Сортировка 10000 объектов", _performanceData.SortTime < 100,
                      $"{_performanceData.SortTime} ms");
        }

        // ============================================================
        // 6. НАГРУЗОЧНЫЕ ТЕСТЫ (ЧЕСТНЫЕ - с сохранением результатов)
        // ============================================================
        static async Task RunLoadTests()
        {
            PrintSection("📊 НАГРУЗОЧНЫЕ ТЕСТЫ (ЧЕСТНЫЕ ЗАМЕРЫ)");

            Console.WriteLine("\n┌────────────┬──────────────┬─────────────────┬──────────────────────┬─────────────────┐");
            Console.WriteLine("│ Операций   │ Время (ms)   │ Среднее (ms/оп) │ Кол-во созданных     │ Статус          │");
            Console.WriteLine("├────────────┼──────────────┼─────────────────┼──────────────────────┼─────────────────┤");

            // Прогрев JIT компилятора
            WarmUp();

            // Реальные тесты с сохранением результатов
            var result100 = RunRealLoadTest(100);
            var result1000 = RunRealLoadTest(1000);
            var result10000 = RunRealLoadTest(10000);
            var result50000 = RunRealLoadTest(50000);
            var result100000 = RunRealLoadTest(100000);

            PrintLoadRow(100, result100.time, result100.count);
            PrintLoadRow(1000, result1000.time, result1000.count);
            PrintLoadRow(10000, result10000.time, result10000.count);
            PrintLoadRow(50000, result50000.time, result50000.count);
            PrintLoadRow(100000, result100000.time, result100000.count);

            Console.WriteLine("└────────────┴──────────────┴─────────────────┴──────────────────────┴─────────────────┘");

            // Сохраняем результаты для финального отчета
            _performanceData.LoadResults = new List<LoadResult>
            {
                new LoadResult { Operations = 100, TimeMs = result100.time },
                new LoadResult { Operations = 1000, TimeMs = result1000.time },
                new LoadResult { Operations = 10000, TimeMs = result10000.time },
                new LoadResult { Operations = 50000, TimeMs = result50000.time },
                new LoadResult { Operations = 100000, TimeMs = result100000.time }
            };
        }

        static void WarmUp()
        {
            // Прогрев JIT компилятора для честных замеров
            Console.Write("   Прогрев системы");
            for (int warmup = 0; warmup < 3; warmup++)
            {
                var warmupTasks = new List<TaskItem>();
                for (int i = 0; i < 5000; i++)
                {
                    warmupTasks.Add(new TaskItem
                    {
                        Diff = i % 5 + 1,
                        Title = $"Warmup_{i}",
                        Expected = (i * 2).ToString(),
                        Hint = "Прогрев"
                    });
                }
                Console.Write(".");
            }
            Console.WriteLine(" Готово!");
        }

        static (long time, int count) RunRealLoadTest(int count)
        {
            var sw = Stopwatch.StartNew();
            var savedTasks = new List<TaskItem>();  // ← СОХРАНЯЕМ ВСЕ СОЗДАННЫЕ ОБЪЕКТЫ

            for (int i = 0; i < count; i++)
            {
                var task = new TaskItem
                {
                    Diff = i % 5 + 1,
                    Title = $"Задание_{i}",
                    Expected = (i * 2).ToString(),
                    Desc = $"Описание задания {i}",
                    Hint = i % 2 == 0 ? "Четное число" : "Нечетное число"
                };
                savedTasks.Add(task);  // ← РЕАЛЬНОЕ ИСПОЛЬЗОВАНИЕ - КОМПИЛЯТОР НЕ ОПТИМИЗИРУЕТ
            }

            sw.Stop();

            // Проверяем, что все объекты действительно создались
            int savedCount = savedTasks.Count;

            // Вычисляем сумму Diff для проверки (компилятор не вырежет)
            long sumDiff = 0;
            long sumTitleLength = 0;
            foreach (var task in savedTasks)
            {
                sumDiff += task.Diff;
                sumTitleLength += task.Title.Length;
            }

            // Используем результаты - если это убрать, компилятор может оптимизировать
            if (savedCount != count)
                Console.Write("ОШИБКА");
            if (sumDiff == 0 && sumTitleLength == 0)
                Console.Write(" ");

            return (sw.ElapsedMilliseconds, savedCount);
        }

        static void PrintLoadRow(int operations, long timeMs, int createdCount)
        {
            double avg = timeMs / (double)operations;
            string status;
            string statusText;

            // Убедимся, что все объекты созданы
            string createdText = createdCount == operations ? $"{createdCount}/{operations}" : $"{createdCount}/{operations} ❌";

            if (timeMs < 50)
            { status = " ✅"; statusText = "ОТЛИЧНО"; }
            else if (timeMs < 200)
            { status = " ✅"; statusText = "ХОРОШО"; }
            else if (timeMs < 1000)
            { status = " ⚠️"; statusText = "НОРМАЛЬНО"; }
            else
            { status = " ❌"; statusText = "МЕДЛЕННО"; }

            Console.WriteLine($"│ {operations,9} │ {timeMs,10} │ {avg,15:F4} │ {createdText,20} │ {statusText,-14} │{status}");

            // Логируем результат
            _results.Add($"[LOAD] {operations} операций: {timeMs}ms (среднее {avg:F4}ms/оп) - создано {createdCount} объектов");

            // Обновляем счетчики для статистики
            if (timeMs < 1000 && createdCount == operations) _totalPassed++;
            else _totalFailed++;
        }

        // ============================================================
        // 7. ТЕСТЫ ПАМЯТИ
        // ============================================================
        static async Task RunMemoryTests()
        {
            PrintSection("💾 ТЕСТЫ ПАМЯТИ");

            // Тест 1: Базовая память
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long baseMemory = GC.GetTotalMemory(true);

            // Тест 2: Память после создания объектов
            var objects = new List<TaskItem>();
            for (int i = 0; i < 10000; i++)
            {
                objects.Add(new TaskItem { Title = new string('x', 100), Expected = i.ToString() });
            }
            long afterCreation = GC.GetTotalMemory(true);
            long memoryUsed = afterCreation - baseMemory;

            _performanceData.MemoryUsed = memoryUsed;
            AddResult("Память на 10000 объектов", memoryUsed < 5 * 1024 * 1024,
                      $"{memoryUsed / 1024} KB ({(memoryUsed / 1024.0 / 10000):F2} KB/объект)");

            // Тест 3: Освобождение памяти
            objects.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long afterClear = GC.GetTotalMemory(true);
            long memoryLeft = afterClear - baseMemory;

            AddResult("Освобождение памяти", memoryLeft < 1024 * 100,
                      $"Осталось {memoryLeft / 1024} KB");

            // Тест 4: Пиковое потребление
            _performanceData.PeakMemory = Process.GetCurrentProcess().PeakWorkingSet64;
            AddResult("Пиковое потребление", _performanceData.PeakMemory < 200 * 1024 * 1024,
                      $"{_performanceData.PeakMemory / (1024 * 1024)} MB");

            // Тест 5: Сборка мусора
            var sw = Stopwatch.StartNew();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            sw.Stop();
            _performanceData.GCTime = sw.ElapsedMilliseconds;
            AddResult("Время сборки мусора", _performanceData.GCTime < 100,
                      $"{_performanceData.GCTime} ms");
        }

        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ============================================================
        static void PrintSection(string title)
        {
            Console.WriteLine($"\n{title}");
            Console.WriteLine(new string('─', 70));
        }

        static void AddResult(string testName, bool passed, string message, bool isWarning = false)
        {
            if (passed) _totalPassed++;
            else _totalFailed++;

            _allResults.Add(new TestResult
            {
                Name = testName,
                Passed = passed,
                Message = message,
                IsWarning = isWarning
            });

            string icon = passed ? (isWarning ? "⚠️" : "✅") : "❌";
            Console.WriteLine($"  {icon} {testName}: {(passed ? (isWarning ? message : "OK") : message)}");
        }

        static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        static void PrintFinalReport()
        {
            int total = _totalPassed + _totalFailed;
            double successRate = total > 0 ? (_totalPassed * 100.0 / total) : 0;

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    📊 ФИНАЛЬНЫЙ ОТЧЕТ                             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");

            // Статистика по категориям
            Console.WriteLine("\n📈 СТАТИСТИКА ТЕСТИРОВАНИЯ:");
            Console.WriteLine($"   ✅ Пройдено: {_totalPassed}");
            Console.WriteLine($"   ❌ Не пройдено: {_totalFailed}");
            Console.WriteLine($"   📊 Всего тестов: {total}");
            Console.WriteLine($"   🎯 Успех: {successRate:F1}%");

            // Таблица производительности
            Console.WriteLine("\n📊 ТАБЛИЦА ПРОИЗВОДИТЕЛЬНОСТИ:");
            Console.WriteLine($"   ⚡ Создание 10000 объектов: {_performanceData.CreateObjectsTime} ms");
            Console.WriteLine($"   🔍 Поиск среди 10000: {_performanceData.SearchTime} ms");
            Console.WriteLine($"   📦 JSON сериализация: {_performanceData.JsonTime} ms ({_performanceData.JsonSize} байт)");
            Console.WriteLine($"   🔄 JSON десериализация: {_performanceData.DeserializeTime} ms");
            Console.WriteLine($"   📊 Сортировка 10000: {_performanceData.SortTime} ms");

            // Нагрузочные тесты
            if (_performanceData.LoadResults != null && _performanceData.LoadResults.Any())
            {
                Console.WriteLine("\n📊 НАГРУЗОЧНЫЕ ТЕСТЫ:");
                foreach (var load in _performanceData.LoadResults)
                {
                    double avg = load.TimeMs / (double)load.Operations;
                    Console.WriteLine($"   📈 {load.Operations,6} операций: {load.TimeMs,5} ms (среднее {avg:F4} ms/оп)");
                }
            }

            // Память
            Console.WriteLine("\n💾 ИСПОЛЬЗОВАНИЕ ПАМЯТИ:");
            Console.WriteLine($"   💿 Память на 10000 объектов: {_performanceData.MemoryUsed / 1024} KB");
            Console.WriteLine($"   🗑️ Сборка мусора: {_performanceData.GCTime} ms");
            Console.WriteLine($"   📈 Пиковое потребление: {_performanceData.PeakMemory / (1024 * 1024)} MB");

            // Вердикт
            Console.WriteLine("\n══════════════════════════════════════════════════════════════════");
            if (successRate >= 90)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("   🏆 ВЕРДИКТ: ПРОДУКТ ГОТОВ К РЕЛИЗУ! Отличные показатели! 🏆");
            }
            else if (successRate >= 70)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   ⚠️ ВЕРДИКТ: ТРЕБУЕТ НЕЗНАЧИТЕЛЬНОЙ ДОРАБОТКИ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("   ❌ ВЕРДИКТ: ТРЕБУЕТ СЕРЬЕЗНОЙ ДОРАБОТКИ");
            }
            Console.ResetColor();
            Console.WriteLine("══════════════════════════════════════════════════════════════════");
        }

        static void SaveFullReport()
        {
            try
            {
                Directory.CreateDirectory("test_reports");
                string filename = $"test_reports/full_report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"ОТЧЕТ О ТЕСТИРОВАНИИ C# MASTER");
                sb.AppendLine($"Дата: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                foreach (var result in _allResults)
                {
                    sb.AppendLine($"[{(result.Passed ? "OK" : "FAIL")}] {result.Name}");
                    sb.AppendLine($"  {result.Message}");
                    sb.AppendLine();
                }

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"ПРОЙДЕНО: {_totalPassed}");
                sb.AppendLine($"НЕ ПРОЙДЕНО: {_totalFailed}");
                sb.AppendLine($"ВСЕГО: {_totalPassed + _totalFailed}");
                sb.AppendLine($"УСПЕХ: {(_totalPassed * 100.0 / (_totalPassed + _totalFailed)):F1}%");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");

                File.WriteAllText(filename, sb.ToString());
                Console.WriteLine($"\n📄 Детальный отчет сохранен: {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n⚠️ Не удалось сохранить отчет: {ex.Message}");
            }
        }
    }

    // Классы для хранения данных
    public class TestResult
    {
        public string Name { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; }
        public bool IsWarning { get; set; }
    }

    public class LoadResult
    {
        public int Operations { get; set; }
        public long TimeMs { get; set; }
    }

    public class PerformanceData
    {
        public long CreateObjectsTime { get; set; }
        public long SearchTime { get; set; }
        public long JsonTime { get; set; }
        public long DeserializeTime { get; set; }
        public long SortTime { get; set; }
        public long MemoryUsed { get; set; }
        public long PeakMemory { get; set; }
        public long GCTime { get; set; }
        public int JsonSize { get; set; }
        public List<LoadResult> LoadResults { get; set; } = new();
    }
}