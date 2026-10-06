namespace ValidacionEventos.Data;

public static class DataPathHelper
{
    public static string ObtenerRuta(string nombreArchivo)
    {
        // 1. Buscar carpeta "data" hacia arriba desde el directorio de trabajo actual
        string? dir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(dir))
        {
            string rutaCandidata = Path.Combine(dir, "data", nombreArchivo);
            if (File.Exists(rutaCandidata) || Directory.Exists(Path.Combine(dir, "data")))
            {
                return Path.GetFullPath(rutaCandidata);
            }
            dir = Directory.GetParent(dir)?.FullName;
        }

        // 2. Buscar hacia arriba desde el directorio base de la aplicación (bin/.../net8.0)
        dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            string rutaCandidata = Path.Combine(dir, "data", nombreArchivo);
            if (File.Exists(rutaCandidata) || Directory.Exists(Path.Combine(dir, "data")))
            {
                return Path.GetFullPath(rutaCandidata);
            }
            dir = Directory.GetParent(dir)?.FullName;
        }

        // 3. Fallback relativo
        return Path.Combine("data", nombreArchivo);
    }
}
