using System.Security.Cryptography;

namespace GestionEventos.Logica;

public static class GeneradorCodigoEntrada
{
    private const string Caracteres = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int LongitudCodigo = 6;
    private static readonly object Bloqueo = new();

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
            int index = RandomNumberGenerator.GetInt32(Caracteres.Length);
            chars[i] = Caracteres[index];
        }
        return new string(chars);
    }
}
