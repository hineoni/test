﻿﻿namespace pleifer.Services;

public class PlayfairCipher
{
    private char[,] _matrix;
    private string _key;
    private string _alphabet;
    private string _replacementChar;
    private int _matrixRows;
    private int _matrixCols;

    private void Init(string plaintext, string key)
    {
        _key = key.ToUpper().Replace(" ", "");

        if (IsCyrillic(plaintext))
        {
            _alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЫЭЮЯЬ";
            _matrixRows = 4;
            _matrixCols = 8;
            _replacementChar = "Ю";
        }
        else
        {
            _alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
            _matrixRows = 5;
            _matrixCols = 5;
            _replacementChar = "X";
        }

        CreateMatrix();
    }

    private static bool IsCyrillic(string input)
    {
        for (var index = 0; index < input.Length; index++)
        {
            var c = input[index];
            if (c >= 'А' && c <= 'я' || c == 'Ё' || c == 'ё')
            {
                return true;
            }
        }

        return false;
    }

    // Метод для создания матрицы
    private void CreateMatrix()
    {
        HashSet<char> usedChars = new HashSet<char>();
        _matrix = new char[_matrixRows, _matrixCols];
        int index = 0;

        // Заполняем матрицу ключом (удаляем повторяющиеся символы)
        foreach (char c in _key)
        {
            if (!usedChars.Contains(c) && _alphabet.Contains(c.ToString()))
            {
                usedChars.Add(c);
                _matrix[index / _matrixCols, index % _matrixCols] = c;
                index++;
            }
        }

        // Заполняем оставшиеся клетки матрицы буквами из алфавита
        foreach (char c in _alphabet)
        {
            if (!usedChars.Contains(c))
            {
                usedChars.Add(c);
                _matrix[index / _matrixCols, index % _matrixCols] = c;
                index++;
            }
        }
    }

    // Метод для нахождения координат буквы в матрице
    private (int, int) GetPosition(char c)
    {
        for (int row = 0; row < _matrixRows; row++)
        {
            for (int col = 0; col < _matrixCols; col++)
            {
                if (_matrix[row, col] == c)
                {
                    return (row, col);
                }
            }
        }
        throw new ArgumentException($"Character '{c}' not found in the matrix.");
    }

    // Метод для шифрования текста
    public string Encrypt(string plaintext, string key)
    {
        Init(plaintext, key);

        plaintext = plaintext.ToUpper().Replace("J", "I").Replace(" ", "");

        // Если длина нечетная, добавляем символ
        if (plaintext.Length % 2 != 0)
        {
            plaintext += _replacementChar;
        }

        // Разделяем на биграммы (пары символов)
        List<string> bigrams = new List<string>();
        for (int i = 0; i < plaintext.Length; i += 2)
        {
            if (i + 1 >= plaintext.Length) // Если последний символ, и он один
            {
                bigrams.Add(plaintext[i] + "" + _replacementChar); // Добавляем первый символ
            }
            else if (plaintext[i] == plaintext[i + 1]) // Если две одинаковые буквы подряд
            {
                bigrams.Add(plaintext[i] + "" + _replacementChar); // Добавляем первый символ
                i--; // Сдвигаем индекс, чтобы правильно обработать следующую пару
            }
            else
            {
                bigrams.Add(plaintext[i] + "" + plaintext[i + 1]);
            }
        }

        // Шифруем биграммы
        string ciphertext = "";
        foreach (var bigram in bigrams)
        {
            char first = bigram[0];
            char second = bigram[1];

            var (row1, col1) = GetPosition(first);
            var (row2, col2) = GetPosition(second);

            // Правила шифрования
            if (row1 == row2)
            {
                // Одинаковая строка: сдвигаем вправо
                col1 = (col1 + 1) % _matrixCols;
                col2 = (col2 + 1) % _matrixCols;
            }
            else if (col1 == col2)
            {
                // Одинаковый столбец: сдвигаем вниз
                row1 = (row1 + 1) % _matrixRows;
                row2 = (row2 + 1) % _matrixRows;
            }
            else
            {
                // Прямоугольник: меняем столбцы
                (col1, col2) = (col2, col1);
            }

            ciphertext += _matrix[row1, col1];
            ciphertext += _matrix[row2, col2];
        }

        return ciphertext;
    }

    // Метод для дешифрования текста (аналогичен Encrypt, только сдвигаем влево/вверх)
    public string Decrypt(string ciphertext, string key)
    {
        Init(ciphertext, key);

        ciphertext = ciphertext.ToUpper().Replace(" ", "");

        // Разделяем на биграммы
        List<string> bigrams = new List<string>();
        for (int i = 0; i < ciphertext.Length; i += 2)
        {
            bigrams.Add(ciphertext[i] + "" + ciphertext[i + 1]);
        }

        // Дешифруем биграммы
        string plaintext = "";
        foreach (var bigram in bigrams)
        {
            char first = bigram[0];
            char second = bigram[1];

            var (row1, col1) = GetPosition(first);
            var (row2, col2) = GetPosition(second);

            // Правила дешифрования
            if (row1 == row2)
            {
                // Одинаковая строка: сдвигаем влево
                col1 = (col1 - 1 + _matrixCols) % _matrixCols;
                col2 = (col2 - 1 + _matrixCols) % _matrixCols;
            }
            else if (col1 == col2)
            {
                // Одинаковый столбец: сдвигаем вверх
                row1 = (row1 - 1 + _matrixRows) % _matrixRows;
                row2 = (row2 - 1 + _matrixRows) % _matrixRows;
            }
            else
            {
                // Прямоугольник: меняем столбцы
                (col1, col2) = (col2, col1);
            }

            plaintext += _matrix[row1, col1];
            plaintext += _matrix[row2, col2];
        }

        return plaintext.Replace(_replacementChar, "");
    }
}