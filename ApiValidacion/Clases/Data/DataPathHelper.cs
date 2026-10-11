namespace ValidacionEventos.Data;

/// <summary>
/// Resuelve la ruta (relativa al proyecto) de los archivos compartidos en la carpeta "data" de la raíz
/// del repositorio. Sube por los directorios padres hasta encontrarla, así que funciona lanzando la API
/// desde la raíz, desde la carpeta de la solución o desde la carpeta Api.
/// </summary>
public static class DataPathHelper
{
    public static string ObtenerRuta(string nombreArchivo)
    {
        string? carpetaData = BuscarCarpetaData(Directory.GetCurrentDirectory())
                              ?? BuscarCarpetaData(AppContext.BaseDirectory);

        if (carpetaData == null)
        {
            // Fallar fuerte: si cada API creara su propia carpeta "data" en silencio, dejarían de compartir datos.
            throw new DirectoryNotFoundException(
                "No se encontró la carpeta 'data' subiendo desde '" + Directory.GetCurrentDirectory() +
                "'. Lance la API desde dentro del repositorio (ver README).");
        }

        return Path.Combine(carpetaData, nombreArchivo);
    }

    private static string? BuscarCarpetaData(string directorioInicial)
    {
        DirectoryInfo? directorio = new DirectoryInfo(directorioInicial);

        while (directorio != null)
        {
            string candidata = Path.Combine(directorio.FullName, "data");
            if (Directory.Exists(candidata))
            {
                return candidata;
            }

            directorio = directorio.Parent;
        }

        return null;
    }
}
