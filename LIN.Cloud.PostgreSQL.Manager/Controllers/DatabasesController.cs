using Http.ResponsesList;
using LIN.Cloud.PostgreSQL.Manager.Services;
using LIN.Cloud.PostgreSQL.Manager.Services.Local;
using LIN.Types.Cloud.PostgreSQL.Models;
using Microsoft.AspNetCore.Mvc;

namespace LIN.Cloud.PostgreSQL.Manager.Controllers;

[Route("api/[controller]")]
public class DatabasesController(ManagementService managementService, DatabasesManager manager) : ControllerBase
{

    /// <summary>
    /// Crear nueva base de datos.
    /// </summary>
    /// <param name="request">Contenido.</param>
    /// <param name="cloud">Token cloud.</param>
    /// <param name="key">Key.</param>
    [HttpPost]
    public async Task<HttpCreateResponse> Create([FromBody] CreateDatabaseRequest request, [FromHeader] string cloud, [FromHeader] string key)
    {

        // Validar token.
        var (authenticated, _, project) = Identity.Utilities.JwtCloud.Validate(cloud);

        // Si no está autenticado, retornar error.
        if (!authenticated)
            return new()
            {
                Message = "Token invalido",
                Response = Types.Responses.Responses.Unauthorized
            };

        // Validar parámetros.
        var validationResults = Validations.Validate(request);

        if (validationResults.Count > 0)
            return new()
            {
                Message = "Error en los parámetros",
                Response = Types.Responses.Responses.InvalidParam,
                Errors = validationResults
            };

        // Generar el cobro de aprovisionamiento de la base de datos.
        var billing = await Access.Developer.Controllers.Billings.Create(key, 100);

        if (billing.Response != Types.Responses.Responses.Success)
            return new()
            {
                Response = Types.Responses.Responses.WithoutCredits,
                Message = "Error al generar el cobro.",
            };

        // Crear la base de datos.
        var create = await managementService.CreateDatabaseWithUserAsync(request.DatabaseName, request.Username, request.Password, project);

        return create;

    }


    /// <summary>
    /// Obtener una base de datos.
    /// </summary>
    /// <param name="cloud">Token cloud.</param>
    [HttpGet]
    public async Task<HttpReadOneResponse<DataBaseModel>> Get([FromHeader] string cloud)
    {

        // Validar token.
        var (authenticated, _, project) = Identity.Utilities.JwtCloud.Validate(cloud);

        // Si no está autenticado, retornar error.
        if (!authenticated)
            return new()
            {
                Response = Types.Responses.Responses.Unauthorized,
                Message = "Token invalido"
            };

        // Bases de datos relacionadas al usuario.
        var all = await manager.Read(project);

        // Lógica para obtener las bases de datos disponibles
        return new Types.Responses.ReadOneResponse<DataBaseModel>()
        {
            Response = Types.Responses.Responses.Success,
            Model = all
        };
    }


    /// <summary>
    /// Eliminar una base de datos y su usuario asociado
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    [HttpDelete("{databaseName}")]
    public async Task<IActionResult> Delete(string databaseName)
    {
        try
        {
            await managementService.DeleteDatabaseWithUserAsync(databaseName, databaseName);  // Suponiendo que el nombre de usuario es igual al de la base de datos.
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}