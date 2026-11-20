using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia;

namespace Avalo;

public partial class MainWindow : Window
{
    // Определяем цвета для текста
    private readonly IBrush _textDarkBrown = new SolidColorBrush(Color.Parse("#3E2723"));
    private readonly IBrush _textLight = new SolidColorBrush(Color.Parse("#D7CCC8"));

    public MainWindow()
    {
        InitializeComponent();
        ShowLogin(); 

        BtnClub.Click += (_, _) => ShowClubPage();
        BtnHippodrome.Click += (_, _) => ShowHippodromePage();
    }

    private void ShowPopupError(Control target, string message)
    {
        var popup = new Popup
        {
            PlacementTarget = target,
            Placement = PlacementMode.Bottom,
            IsLightDismissEnabled = true,
            Child = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse("#E65100")), // Оранжевая рамка ошибки
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10, 5),
                Child = new TextBlock
                {
                    Text = message,
                    Foreground = new SolidColorBrush(Color.Parse("#E65100")), // Оранжевый текст ошибки
                    FontSize = 13,
                    FontWeight = FontWeight.Bold
                }
            }
        };

        popup.Open();
        
        var timer = new System.Timers.Timer(3000);
        timer.Elapsed += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => popup.Close());
            timer.Dispose();
        };
        timer.AutoReset = false;
        timer.Start();
    }

    // ==========================================
    // СТРАНИЦА: АВТОРИЗАЦИЯ
    // ==========================================
    private void ShowLogin()
    {
        TopPanel.IsVisible = false;

        var formContainer = new StackPanel
        {
            Width = 320,
            Spacing = 0,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var title = new TextBlock
        {
            Text = "Авторизация",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = _textDarkBrown,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };

        var txtLogin = new TextBox
        {
            PlaceholderText = "Введите логин...",
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        var txtPassword = new TextBox
        {
            PlaceholderText = "Введите пароль...",
            PasswordChar = '•',
            Margin = new Avalonia.Thickness(0, 35, 0, 0)
        };

        var btnLogin = new Button
        {
            Content = "Войти",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Avalonia.Thickness(0, 35, 0, 0)
        };

        var linkRegister = new TextBlock
        {
            Text = "Нет аккаунта? Создать",
            Foreground = _textDarkBrown,
            TextDecorations = TextDecorations.Underline,
            HorizontalAlignment = HorizontalAlignment.Center,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Margin = new Avalonia.Thickness(0, 15, 0, 0)
        };

        btnLogin.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                ShowPopupError(txtLogin, "Пустая строка");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowPopupError(txtPassword, "Пустая строка");
                return;
            }

            OnLoginSuccess();
        };

        linkRegister.PointerPressed += (_, _) => ShowRegister();

        formContainer.Children.Add(title);
        formContainer.Children.Add(txtLogin);
        formContainer.Children.Add(txtPassword);
        formContainer.Children.Add(btnLogin);
        formContainer.Children.Add(linkRegister);

        MainContent.Content = formContainer;
    }

    // ==========================================
    // СТРАНИЦА: РЕГИСТРАЦИЯ
    // ==========================================
    private void ShowRegister()
    {
        TopPanel.IsVisible = false;

        var formContainer = new StackPanel
        {
            Width = 320,
            Spacing = 0,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var title = new TextBlock
        {
            Text = "Регистрация",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = _textDarkBrown,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };

        var txtFullName = CreateTextBox("Введите полное ФИО...");
        var txtLogin = CreateTextBox("Введите логин...");
        var txtPassword = CreateTextBox("Введите пароль...", true);

        var btnRegister = new Button
        {
            Content = "Зарегистрироваться",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Avalonia.Thickness(0, 15, 0, 0)
        };

        var linkLogin = new TextBlock
        {
            Text = "Уже есть аккаунт? Войти",
            Foreground = _textDarkBrown,
            TextDecorations = TextDecorations.Underline,
            HorizontalAlignment = HorizontalAlignment.Center,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Margin = new Avalonia.Thickness(0, 15, 0, 0)
        };

        btnRegister.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowPopupError(txtFullName, "Пустая строка");
                return;
            }
            if (txtFullName.Text.Trim().Split(' ').Length < 2)
            {
                ShowPopupError(txtFullName, "Введите имя и фамилию");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                ShowPopupError(txtLogin, "Пустая строка");
                return;
            }
            if (txtLogin.Text.Length < 4)
            {
                ShowPopupError(txtLogin, "Логин должен быть не менее 4 символов");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowPopupError(txtPassword, "Пустая строка");
                return;
            }
            if (txtPassword.Text.Length < 6)
            {
                ShowPopupError(txtPassword, "Пароль должен быть не менее 6 символов");
                return;
            }

            OnLoginSuccess();
        };

        linkLogin.PointerPressed += (_, _) => ShowLogin();

        formContainer.Children.Add(title);
        formContainer.Children.Add(txtFullName);
        formContainer.Children.Add(txtLogin);
        formContainer.Children.Add(txtPassword);
        formContainer.Children.Add(btnRegister);
        formContainer.Children.Add(linkLogin);

        MainContent.Content = formContainer;
    }

    private TextBox CreateTextBox(string placeholder, bool isPassword = false)
    {
        return new TextBox
        {
            PlaceholderText = placeholder,
            PasswordChar = isPassword ? '•' : '\0',
            Margin = new Avalonia.Thickness(0, 10, 0, 0)
        };
    }

    // ==========================================
    // СТРАНИЦА: КЛУБ АЛЛЮР
    // ==========================================
    private void ShowClubPage()
    {
        BtnClub.Classes.Add("active");
        BtnHippodrome.Classes.Remove("active");

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };

        var mainStack = new StackPanel
        {
            Spacing = 20,
            Margin = new Thickness(40),
            HorizontalAlignment = HorizontalAlignment.Center,
            MaxWidth = 850
        };

        var title = new TextBlock
        {
            Text = "Конноспортивный клуб «Аллюр»",
            FontSize = 36,
            FontWeight = FontWeight.Bold,
            Foreground = _textDarkBrown,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var subtitle = new TextBlock
        {
            Text = "Мир верховой езды и гармонии с лошадьми",
            FontSize = 20,
            FontStyle = FontStyle.Italic,
            Foreground = new SolidColorBrush(Color.Parse("#8D6E63")), // Светло-коричневый
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };

        var separator = new Border
        {
            Height = 2,
            Background = new SolidColorBrush(Color.Parse("#8D6E63")),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var aboutTitle = new TextBlock
        {
            Text = "О нас",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = _textDarkBrown,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var aboutText = new TextBlock
        {
            Text = "Конноспортивный клуб «Аллюр» — это место, где каждый может прикоснуться к прекрасному миру лошадей. " +
                   "Мы предлагаем обучение верховой езде для взрослых и детей, прогулки на свежем воздухе, а также " +
                   "профессиональную подготовку спортсменов.",
            FontSize = 16,
            Foreground = _textDarkBrown,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 24
        };

        var horsesTitle = new TextBlock
        {
            Text = "Наши лошади",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = _textDarkBrown,
            Margin = new Thickness(0, 20, 0, 10)
        };

        var btnAddHorse = new Button
        {
            Content = "+ Добавить лошадь",
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        btnAddHorse.Click += (_, _) => ShowAddDialog("Добавление лошади");

        var horsesGrid = CreateTable(
            new[] { "Кличка", "Порода", "Возраст", "Окрас" },
            new[]
            {
                new[] { "Гром", "Орловский рысак", "5 лет", "Вороной" },
                new[] { "Заря", "Арабская", "7 лет", "Гнедая" },
                new[] { "Ветер", "Владимирский тяжеловоз", "4 года", "Рыжий" }
            }
        );

        var trainersTitle = new TextBlock
        {
            Text = "Наши тренеры",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = _textDarkBrown,
            Margin = new Thickness(0, 20, 0, 10)
        };

        var btnAddTrainer = new Button
        {
            Content = "+ Добавить тренера",
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        btnAddTrainer.Click += (_, _) => ShowAddDialog("Добавление тренера");

        var trainersGrid = CreateTable(
            new[] { "ФИО", "Специализация", "Опыт" },
            new[]
            {
                new[] { "Иванов И.И.", "Конкур", "10 лет" },
                new[] { "Петрова А.С.", "Выездка", "8 лет" },
                new[] { "Сидоров П.П.", "Обучение новичков", "5 лет" }
            }
        );

        mainStack.Children.Add(title);
        mainStack.Children.Add(subtitle);
        mainStack.Children.Add(separator);
        mainStack.Children.Add(aboutTitle);
        mainStack.Children.Add(aboutText);
        mainStack.Children.Add(horsesTitle);
        mainStack.Children.Add(btnAddHorse);
        mainStack.Children.Add(horsesGrid);
        mainStack.Children.Add(trainersTitle);
        mainStack.Children.Add(btnAddTrainer);
        mainStack.Children.Add(trainersGrid);

        scrollViewer.Content = mainStack;
        MainContent.Content = scrollViewer;
    }

    private Border CreateTable(string[] headers, string[][] rows)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            RowDefinitions = new RowDefinitions(),
            Margin = new Thickness(0, 0, 0, 20)
        };

        for (int i = 0; i <= rows.Length; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        for (int col = 0; col < headers.Length; col++)
        {
            var header = new TextBlock
            {
                Text = headers[col],
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.White, // Белый текст на коричневом фоне
                Margin = new Thickness(8, 5, 8, 5),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(header, col);
            Grid.SetRow(header, 0);
            grid.Children.Add(header);
        }

        // Фон для заголовка таблицы
        var headerBackground = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#5D4037")), // Тёмно-коричневый
            CornerRadius = new CornerRadius(8, 8, 0, 0),
            Padding = new Thickness(0)
        };
        Grid.SetColumnSpan(headerBackground, headers.Length);
        Grid.SetRow(headerBackground, 0);
        grid.Children.Add(headerBackground);

        for (int row = 0; row < rows.Length; row++)
        {
            for (int col = 0; col < rows[row].Length; col++)
            {
                var cell = new TextBlock
                {
                    Text = rows[row][col],
                    Foreground = _textDarkBrown,
                    Margin = new Thickness(8, 8, 8, 8),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(cell, col);
                Grid.SetRow(cell, row + 1);
                grid.Children.Add(cell);
            }
        }

        return new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#8D6E63")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Background = Brushes.White,
            Child = grid
        };
    }

    private void ShowAddDialog(string title)
    {
        var window = new Window
        {
            Title = title,
            Width = 400,
            Height = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = new SolidColorBrush(Color.Parse("#F5F0EB")), // Светло-бежевый фон
            CanResize = false
        };

        var stack = new StackPanel
        {
            Spacing = 15,
            Margin = new Thickness(20)
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = _textDarkBrown,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };

        var input1 = new TextBox { PlaceholderText = "Введите значение..." };
        var input2 = new TextBox { PlaceholderText = "Введите значение..." };
        var input3 = new TextBox { PlaceholderText = "Введите значение..." };

        var btnSave = new Button
        {
            Content = "Сохранить (визуально)",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 15, 0, 0)
        };

        btnSave.Click += (_, _) => window.Close();

        stack.Children.Add(titleText);
        stack.Children.Add(input1);
        stack.Children.Add(input2);
        stack.Children.Add(input3);
        stack.Children.Add(btnSave);

        window.Content = stack;
        window.ShowDialog(this);
    }

    private void ShowHippodromePage()
    {
        BtnHippodrome.Classes.Add("active");
        BtnClub.Classes.Remove("active");

        var grid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var text = new TextBlock
        {
            Text = "Страница: Ипподром (в разработке)",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = _textDarkBrown
        };

        grid.Children.Add(text);
        MainContent.Content = grid;
    }

    private void OnLoginSuccess()
    {
        TopPanel.IsVisible = true; 
        ShowClubPage(); 
    }
}