using System.Security.Cryptography;

namespace GestionEventos.Logica;

public static class GeneradorCodigoEntrada
{
    private const string Caracteres = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int LongitudCodigo = 6;
    private static readonly object Bloqueo = new();

    /// <summary>
    /// Genera un código alfanumérico de 6 caracteres que no está en <paramref name="codigosExistentes"/>
    /// (que debe contener los códigos de TODAS las compras) y lo agrega al conjunto, de modo que
    /// tampoco se repita dentro de la misma operación.
    /// </summary>
    public static string GenerarCodigoUnico(ISet<string> codigosExistentes)
    {
        lock (Bloqueo)
        {
            string codigo;
            do
            {
                codigo = GenerarCodigoAleatorio();
            }
            while (codigosExistentes.Contains(codigo));

            codigosExistentes.Add(codigo);
            return codigo;
        }
    }

    private static string GenerarCodigoAleatorio()
    {
        char[] chars = new char[LongitudCodigo];
        for (int i = 0; i < LongitudCodigo; i++)
        {
            chars[i] = Caracteres[RandomNumberGenerator.GetInt32(Caracteres.Length)];
        }
        return new string(chars);
    }
}
