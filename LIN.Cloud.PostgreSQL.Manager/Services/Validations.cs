using LIN.Types.Cloud.PostgreSQL.Models;
using System.Text.RegularExpressions;

namespace LIN.Cloud.PostgreSQL.Manager.Services;

public class Validations
{

    /// <summary>
    /// Validar.
    /// </summary>
    public static List<Types.Models.ErrorModel> Validate(CreateDatabaseRequest request)
    {

        List<LIN.Types.Models.ErrorModel> errors = [];

        // Validar nombre de la BD.
        if (!IsValidNameForDb(request.DatabaseName, out string message))
            errors.Add(new LIN.Types.Models.ErrorModel
            {
                Tittle = "DatabaseName",
                Description = message,
                Type = Types.Enumerations.ErrorTypes.User
            });

        // Validar nombre de usuario.
        if (!IsValidNameForUser(request.Username, out message))
            errors.Add(new LIN.Types.Models.ErrorModel
            {
                Tittle = "Username",
                Description = message,
                Type = Types.Enumerations.ErrorTypes.User
            });

        return errors;
    }


    /// <summary>
    /// Validar si un nombre es válido para una base de datos.
    /// </summary>
    /// <param name="name">Nombre de la base de datos.</param>
    public static bool IsValidNameForDb(string name, out string message)
    {
        // Validaciones para nombres en la base de datos
        // 1. Longitud entre 1 y 64 caracteres.
        if (string.IsNullOrWhiteSpace(name) || name.Length > 64)
        {
            message = "El nombre para la base de datos debe tener entre 1 y 64 caracteres.";
            return false;
        }

        // 2. No debe contener caracteres especiales prohibidos
        string patron = @"^[a-zA-Z0-9_.]+$"; // Solo letras, números, guiones bajos y puntos
        if (!Regex.IsMatch(name, patron))
        {
            message = "El nombre para la base de datos contiene caracteres no permitidos.";
            return false;
        }

        message = string.Empty;
        return true;
    }


    /// <summary>
    /// Validar nombre de usuario.
    /// </summary>
    /// <param name="name">Nombre.</param>
    public static bool IsValidNameForUser(string name, out string message)
    {
        // Validaciones para nombres de usuario
        // 1. Longitud mínima y máxima (por ejemplo, 3-20 caracteres)
        if (name.Length is < 3 or > 20)
        {
            message = "El nombre de usuario debe tener entre 3 y 20 caracteres.";
            return false;
        }

        // 2. No debe empezar ni terminar con un punto o guion bajo
        if (name.StartsWith(".") || name.StartsWith("_") || name.EndsWith(".") || name.EndsWith("_"))
        {
            message = "El nombre de usuario no debe empezar ni terminar con un punto o guion bajo.";
            return false;
        }

        // 3. Validar que no haya múltiples puntos o guiones bajos consecutivos
        if (name.Contains("..") || name.Contains("__") || name.Contains("._") || name.Contains("_."))
        {
            message = "El nombre de usuario no debe contener puntos o guiones bajos consecutivos.";
            return false;
        }

        message = string.Empty;
        return true;
    }

}