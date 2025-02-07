﻿using System.Text;

namespace ConsoleApp1;

class Program
{
    private static ApiService apiService = new ApiService("https://localhost:5001");
    private static bool _isAuth = false;

    static async Task Main(string[] args)
    {
        bool exit = false;

        while (!exit)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.Clear();
            Console.WriteLine("Выберите операцию:");
            Console.WriteLine("1.  Регистрация");
            Console.WriteLine("2.  Логин");
            Console.WriteLine("3.   Выход");
            Console.Write("Введите номер операции: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
            
                case "1":
                    await RegisterUser();
                    break;
                case "2":
                    await LoginUser();
                    break;
                case "3":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }

            if (!exit && _isAuth != false)
            {
                await ShowAuthenticatedMenu();
            }
        }
    }

    static async Task ShowAuthenticatedMenu()
    {
        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("Меню пользователя:");
            Console.WriteLine("1.  Работа с текстом");
            Console.WriteLine("");
            Console.WriteLine(" ACCOUNT ACTIONS");
            Console.WriteLine("2.  Смена пароля");
            Console.WriteLine("3.  Получить историю запросов пользователя");
            Console.WriteLine("4.  Удалить историю запросов пользователя");
            Console.WriteLine("5.   Выйти из аккаунта");
            Console.Write("Введите номер операции: ");
            string? choice = Console.ReadLine();
            bool isTextMenuShow = false;

            switch (choice)
            {
                case "1":
                    isTextMenuShow = true;
                    await ShowTextMenu();
                    break;
                case "2":
                    await ChangePassword();
                    break;
                case "3":
                    await GetUserRequests();
                    break;
                case "4":
                    await DeleteUserRequests();
                    break;
                case "5":
                    UnloginUser();
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }

            if (!exit && !isTextMenuShow)
            {
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }
    }

    static async Task ShowTextMenu()
    {
        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("Меню работы с текстом:");
            Console.WriteLine("0.  Вернуться");
            Console.WriteLine("1.   Добавить текст");
            Console.WriteLine("2.  Изменить текст");
            Console.WriteLine("3.  Удалить текст");
            Console.WriteLine("4.  Получить текст по id");
            Console.WriteLine("5.  Получить все текста");
            Console.WriteLine("6.  Зашифровать текст");
            Console.WriteLine("7.  Расшифровать текст");
            Console.Write("Введите номер операции: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await AddText();
                    break;
                case "2":
                    await UpdateText();
                    break;
                case "3":
                    await DeleteText();
                    break;
                case "4":
                    await GetTextById();
                    break;
                case "5":
                    await GetAllTexts();
                    break;
                case "6":
                    await EncryptText();
                    break;
                case "7":
                    await DecryptText();
                    break;
                case "0":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }

            if (!exit)
            {
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }
    }

    static async Task RegisterUser()
    {
        Console.Write("Введите имя пользователя: ");
        string username = Console.ReadLine();

        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        var newUser = new User
        {
            Username = username,
            Password = password
        };

        var isAuth = await apiService.RegisterUser(newUser);
        if (isAuth)
        {
            Console.WriteLine("Для продолжения нажмите любую клавишу...");
            Console.ReadKey();
            _isAuth = true;
        }
    }

    static async Task LoginUser()
    {
        Console.Write("Введите имя пользователя: ");
        string username = Console.ReadLine();

        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        var loginUser = new User
        {
            Username = username,
            Password = password
        };

        _isAuth = await apiService.LoginUser(loginUser);

        if (!_isAuth)
        {
            Console.WriteLine("Для продолжения нажмите любую клавишу...");
            Console.ReadKey();
        }
    }

    static async Task LoginTestUser()
    {
        var loginUser = new User
        {
            Username = "admin",
            Password = "sdfsdfgds4gsd45"
        };

        _isAuth = await apiService.LoginUser(loginUser);
    }

    static async Task ChangePassword()
    {
        Console.Write("Введите имя пользователя: ");
        string username = Console.ReadLine();

        Console.Write("Введите старый пароль: ");
        string oldPassword = Console.ReadLine();

        Console.Write("Введите новый пароль: ");
        string newPassword = Console.ReadLine();

        var changePassword = new ChangePassword
        {
            Username = username,
            OldPassword = oldPassword,
            NewPassword = newPassword
        };

        _isAuth = await apiService.ChangePassword(changePassword);
    }

    static void UnloginUser()
    {
        _isAuth = false;
        apiService.UnloginUser();
    }

    static async Task GetUserRequests()
    {
        await apiService.GetUserRequests();
    }

    static async Task DeleteUserRequests()
    {
        await apiService.DeleteUserRequests();
    }

    static async Task AddText()
    {
        Console.Write("Введите текст: ");
        string content = Console.ReadLine();

        var text = new Text
        {
            Content = content,
        };

        await apiService.AddText(text);
    }

    static async Task UpdateText()
    {
        await apiService.GetAllTexts();

        Console.Write("Введите id текста для обновления: ");
        int id = int.Parse(Console.ReadLine());

        Console.Write("Введите новый текст: ");
        string content = Console.ReadLine();

        var text = new Text
        {
            Id = id,
            Content = content
        };

        await apiService.UpdateText(id, text);
    }

    static async Task DeleteText()
    {
        await apiService.GetAllTexts();

        Console.Write("Введите id текста для удаления: ");
        int id = int.Parse(Console.ReadLine());

        await apiService.DeleteText(id);
    }

    static async Task GetTextById()
    {
        await apiService.GetAllTexts(true);

        Console.Write("Введите id текста для получения: ");
        int id = int.Parse(Console.ReadLine());

        await apiService.GetTextById(id);
    }

    static async Task GetAllTexts()
    {
        await apiService.GetAllTexts();
    }

    static async Task EncryptText()
    {
        await apiService.GetAllTexts();

        Console.Write("Введите id текста для шифрования: ");
        int id = int.Parse(Console.ReadLine());

        Console.Write("Введите ключ для шифрования: ");
        string key = Console.ReadLine();

        await apiService.EncryptText(id, key);
    }

    static async Task DecryptText()
    {
        await apiService.GetAllTexts();

        Console.Write("Введите id текста для расшифровки: ");
        int id = int.Parse(Console.ReadLine());

        Console.Write("Введите ключ для расшифровки: ");
        string key = Console.ReadLine();

        await apiService.DecryptText(id, key);
    }
}