namespace ValidacionEventos.Logica;

public static class DataPathHelper
{
    private static string? _dataDirectory;

    public static void SetDataDirectory(string path)
    {
        _dataDirectory = path;
    }

    public static string GetDataDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_dataDirectory) && Directory.Exists(_dataDirectory))
        {
            return _dataDirectory;
        }

        // Search upwards from BaseDirectory
        string? dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            string candidate = Path.Combine(dir, "data");
            if (Directory.Exists(candidate))
            {
                _dataDirectory = candidate;
                return _dataDirectory;
            }

            string candidateSub = Path.Combine(dir, "programacionTrabajo", "data");
            if (Directory.Exists(candidateSub))
            {
                _dataDirectory = candidateSub;
                return _dataDirectory;
            }

            var parent = Directory.GetParent(dir);
            dir = parent?.FullName;
        }

        // Search upwards from CurrentDirectory
        dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(dir))
        {
            string candidate = Path.Combine(dir, "data");
            if (Directory.Exists(candidate))
            {
                _dataDirectory = candidate;
                return _dataDirectory;
            }

            string candidateSub = Path.Combine(dir, "programacionTrabajo", "data");
            if (Directory.Exists(candidateSub))
            {
                _dataDirectory = candidateSub;
                return _dataDirectory;
            }

            var parent = Directory.GetParent(dir);
            dir = parent?.FullName;
        }

        // Fallback: create in current directory
        string fallback = Path.Combine(Directory.GetCurrentDirectory(), "data");
        if (!Directory.Exists(fallback))
        {
            Directory.CreateDirectory(fallback);
        }
        _dataDirectory = fallback;
        return _dataDirectory;
    }

    public static string GetFilePath(string fileName)
    {
        return Path.Combine(GetDataDirectory(), fileName);
    }
}
