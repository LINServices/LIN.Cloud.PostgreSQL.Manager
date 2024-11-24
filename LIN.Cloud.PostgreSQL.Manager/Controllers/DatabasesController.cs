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
    /// Crear una base de datos y su usuario asociado
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateDatabaseWithUser([FromBody] CreateDatabaseRequest request, [FromHeader] string cloud, [FromHeader] string key)
    {
        try
        {

            // Validar token.
            var (autenticated, _, project) = LIN.Cloud.Identity.Utilities.JwtCloud.Validate(cloud);

            if (!autenticated)
            {
                return BadRequest();
            }


            var billing = await LIN.Access.Developer.Controllers.Billings.Create(key, 24);

            if (billing.Response != Types.Responses.Responses.Success)
                return BadRequest("Error al cobrar");

            await managementService.CreateDatabaseWithUserAsync(request.DatabaseName, request.Username, request.Password, project);
            return CreatedAtAction(nameof(GetDatabase), new { databaseName = request.DatabaseName }, null);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    /// <summary>
    /// Eliminar una base de datos y su usuario asociado
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    [HttpDelete("{databaseName}")]
    public async Task<IActionResult> DeleteDatabase(string databaseName)
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


    /// <summary>
    /// Obtener la lista de bases de datos
    /// </summary>
    [HttpGet]
    public async Task<HttpReadAllResponse<DataBaseModel>> GetDatabases([FromHeader] string token)
    {

        var authentication = await LIN.Access.Developer.Controllers.Authentication.Login(token);

        if (authentication.Response != Types.Responses.Responses.Success)
            return new();

        // Bases de datos relacionadas al usuario.
        var all = await manager.ReadAll(authentication.Model.Profile.Id);

        // Lógica para obtener las bases de datos disponibles
        return new LIN.Types.Responses.ReadAllResponse<DataBaseModel>()
        {
            Response = Types.Responses.Responses.Success,
            Models = all
        };
    }


    /// <summary>
    /// Obtener la lista de bases de datos
    /// </summary>
    [HttpGet("one")]
    public async Task<HttpReadOneResponse<DataBaseModel>> GetDatabase([FromHeader] string cloud)
    {

        // Validar token.
        var (autenticated, _, project) = LIN.Cloud.Identity.Utilities.JwtCloud.Validate(cloud);

        if (!autenticated)
        {
            return new();
        }

        // Bases de datos relacionadas al usuario.
        var all = await manager.Read(project);

        // Lógica para obtener las bases de datos disponibles
        return new LIN.Types.Responses.ReadOneResponse<DataBaseModel>()
        {
            Response = Types.Responses.Responses.Success,
            Model = all
        };
    }

}